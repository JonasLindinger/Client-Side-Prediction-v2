using System;
using System.Collections.Generic;
using _Project.Scripts.Network.States;
using CSP.Data;
using CSP.Input;
using CSP.Player;
using CSP.Simulation;
using LindoNoxStudio.Network.Simulation;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using IState = CSP.Simulation.State.IState;

namespace CSP.Object
{
    public class NetworkClient : NetworkBehaviour
    {
        #if Client
        public static NetworkClient LocalClient;
        public static uint WantedBufferSize = 4;
        public static uint WantedBufferSizePositiveTollerance = 3;

        private uint _latestReceivedGameStateTick;
        #elif Server
        /// <summary>
        /// OwnerClientId - NetworkClient
        /// </summary>
        public static Dictionary<ulong, NetworkClient> ClientsByOwnerId = new Dictionary<ulong, NetworkClient>();
        
        private ClientInputState[] _inputStates;
        
        private ClientInputState _emptyInputState;
        #endif
        
        public override void OnNetworkSpawn()
        {
            #if Client
            // We don't need to check for isOwner, because this object is only seen by the server and the owner of this object.
            LocalClient = this;
            #elif Server
            if (_inputStates == null)
                _inputStates = new ClientInputState[NetworkRunner.NetworkSettings.inputBufferSize];
            
            ClientsByOwnerId.Add(OwnerClientId, this);
            
            OnTickSystemInfoRPC(
                TickSystemManager.PhysicsTickRate,
                TickSystemManager.NetworkTickRate,
                TickSystemManager.CurrentTick);
            #endif
        }

        public override void OnNetworkDespawn()
        {
            #if Server
            ClientsByOwnerId.Remove(OwnerClientId);
            #endif
        }

        #region RPC's
        [Rpc(SendTo.Server, Delivery = RpcDelivery.Unreliable)]
        public void OnInputRPC(ClientInputState[] clientInputStates)
        {
            #if Server
            foreach (var input in clientInputStates)
            {
                // If this is an "old" input we skip
                if (input.Tick <= TickSystemManager.CurrentTick) continue;
                _inputStates[input.Tick % _inputStates.Length] = input;
            }
            #endif
        }

        [Rpc(SendTo.Owner, Delivery = RpcDelivery.Unreliable)]
        public void OnServerStateRPC(GameState latestGameState)
        {
            #if Client
            if (latestGameState == null) 
                return;
            
            if (latestGameState.Tick <= _latestReceivedGameStateTick)
                _latestReceivedGameStateTick = latestGameState.Tick;

            if (latestGameState.States == null)
                return;

            if (latestGameState.States.Count == 0)
                return;
            
            ReconcileLocalPlayer(latestGameState);
            #endif
        }
        
        [Rpc(SendTo.Owner, Delivery = RpcDelivery.Reliable)]
        public void OnServerTickRPC(uint tick)
        {
            #if Client
            int rawTickDifference = (int) (TickSystemManager.CurrentTick - tick);
            int tickDifference = Mathf.RoundToInt(rawTickDifference / 2f);
            
            if (WantedBufferSize > tickDifference)
            {
                // Calculate extra ticks
                int amount = (int) (WantedBufferSize - tickDifference);
                Debug.Log("Extra: " + amount);
                TickSystemManager.GetInstance().CalculateExtraTicks(amount);
            }
            else if (tickDifference > WantedBufferSize + WantedBufferSizePositiveTollerance)
            {
                // Skip some ticks
                int amount = (int) (tickDifference - WantedBufferSize + Mathf.RoundToInt(WantedBufferSizePositiveTollerance / 2f));
                Debug.Log("Skipping: " + amount);
                TickSystemManager.GetInstance().SkipTicks(amount);
            }
            #endif
        }
        
        [Rpc(SendTo.Owner, Delivery = RpcDelivery.Reliable)]
        private void OnTickSystemInfoRPC(uint physicsTickRate, uint networkTickRate, uint tickOffset)
        {
            #if Client
            TickSystemManager.GetInstance().StartTickSystems(physicsTickRate, networkTickRate, 1, tickOffset);
            NetworkManager.Singleton.NetworkConfig.TickRate = networkTickRate;
            #endif
        }
        
        #if Server
        public ClientInputState GetInputState(uint tick)
        {
            if (_inputStates == null)
                _inputStates = new ClientInputState[NetworkRunner.NetworkSettings.inputBufferSize];
            
            ClientInputState input = _inputStates[tick % _inputStates.Length];
            if (input != null)
                if (input.Tick == tick) return input;

            // Check if last tick's input null is. If it isn't reuse it and save it for this tick
            if (_inputStates[(tick - 1) % _inputStates.Length] != null)
            {
                input = _inputStates[(tick - 1) % _inputStates.Length];
                input.Tick = tick;
                _inputStates[(tick) % _inputStates.Length] = input;
                return input;
            }
            else
            {
                if (_emptyInputState == null)
                {
                    Dictionary<string, bool> inputFlags = new Dictionary<string, bool>();
                    foreach (string inputName in InputCollector.InputFlagNames)
                        inputFlags.Add(inputName, false);
                    
                    Dictionary<string, Vector2> directionalInputs = new Dictionary<string, Vector2>();
                    foreach (string inputName in InputCollector.DirectionalInputNames)
                        directionalInputs.Add(inputName, Vector2.zero);
                    
                    _emptyInputState = new ClientInputState()
                    {
                        InputFlags = inputFlags,
                        DirectionalInputs = directionalInputs,
                    };
                }
                
                ClientInputState emptyInputForThisTick = _emptyInputState;
                emptyInputForThisTick.Tick = tick;
                
                return emptyInputForThisTick;
            }
        }
        #endif
        #endregion
        
        #region Reconcilation
        #if Client
        private void ReconcileLocalPlayer(GameState serverGameState)
        {
            // Get our local Player Object Id
            ulong localPlayerObjectId = PlayerInputBehaviour.LocalPlayer.NetworkObjectId;

            bool canComparePredictedState = true;
            
            // Try to get the predicted Player State and server Player State
            PlayerState predictedClientState = null;
            PlayerState serverClientState = null;
            try
            {
                if (!SnapshotManager.GetGameState(serverGameState.Tick).States
                        .TryGetValue(localPlayerObjectId, out var predictedState))
                    canComparePredictedState = false;

                predictedClientState = (PlayerState)predictedState;

                // Try to find our Player in the Server State
                if (!serverGameState.States.TryGetValue(localPlayerObjectId, out var serverState))
                    canComparePredictedState = false;

                serverClientState = (PlayerState)serverState;
            }
            catch (Exception)
            {
                canComparePredictedState = false;
            }

            if (!canComparePredictedState)
            {
                // Apply game state, even if our local player isn't in the game state
                ApplyNonLocalPlayersState(serverGameState, true);
                return;
            }

            bool weNeedToReconcile = PlayerInputBehaviour.LocalPlayer.DoWeNeedToReconcile(
                // Get the predicted state
                predictedClientState,
                
                // Get the local client state from serverGameState
                serverClientState
            );
            
            if (!weNeedToReconcile)
            {
                ApplyNonLocalPlayersState(serverGameState, true);
                SnapshotManager.SaveGameState(serverGameState);
                return;
            }
            
            // Apply all states
            ApplyNonLocalPlayersState(serverGameState, false);
            
            // Saving state for reconciliation in the future
            SnapshotManager.TakeSnapshot(serverGameState.Tick);
            
            // Recalculate every tick
            for (uint tick = serverGameState.Tick + 1; tick <= TickSystemManager.CurrentTick; tick++)
                TickSystemManager.RecalculatePhysicsTick(tick);
            
            Debug.LogWarning("Reconciled!");
        }

        private void ApplyNonLocalPlayersState(GameState serverGameState, bool skipLocalPlayer)
        {
            foreach (var kvp in serverGameState.States)
            {
                ulong objectId = kvp.Key;
                IState state = kvp.Value;
                
                // If it is local Player, we skip
                if (objectId == PlayerInputBehaviour.LocalPlayer.NetworkObjectId && skipLocalPlayer)
                    continue;
                
                SnapshotManager.ApplyState(objectId, state);
            }
        }
        #endif
        #endregion
    }
}