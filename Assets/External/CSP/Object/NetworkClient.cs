using System;
using System.Collections.Generic;
using _Project.Scripts.Network;
using CSP.Connection;
using CSP.Data;
using CSP.Input;
using CSP.Player;
using CSP.ScriptableObjects;
using CSP.Simulation;
using CSP.TextDebug;
using Unity.Netcode;
using UnityEngine;

namespace CSP.Object
{
    public class NetworkClient : NetworkBehaviour
    {
        #if Client
        public static NetworkClient LocalClient;

        private uint _latestReceivedGameStateTick;
        #elif Server
        /// <summary>
        /// OwnerClientId - NetworkClient
        /// </summary>
        public static Dictionary<ulong, NetworkClient> ClientsByOwnerId = new Dictionary<ulong, NetworkClient>();
        
        private ClientInputState[] _inputStates;
        #endif
        public bool sentInput = false;
        
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
                SnapshotManager.TickRate,
                CommunicationManager.TickRate,
                SnapshotManager.CurrentTick
                );
            #endif
        }

        public override void OnNetworkDespawn()
        {
            #if Server
            ClientsByOwnerId.Remove(OwnerClientId);
            #endif
        }

        #region RPC's
        
        [Rpc(SendTo.Owner, Delivery = RpcDelivery.Reliable)]
        private void OnTickSystemInfoRPC(uint physicsTickRate, uint networkTickRate, uint tickOffset)
        {
            #if Client
            
            ulong ms = NetworkRunner.GetInstance().UnityTransport.GetCurrentRtt(NetworkRunner.ServerClientId) / 2;
            float msPerTick = 1000f / physicsTickRate;
            int passedTicks = (int)(ms / msPerTick);
            
            uint theServerTickNow = (uint)(tickOffset + passedTicks);
            
            SnapshotManager.KeepTrack(physicsTickRate, theServerTickNow + NetworkRunner.NetworkSettings.tickOffsetBuffer);
            CommunicationManager.StartCommunication(networkTickRate);
            
            #endif
        }

        [Rpc(SendTo.Owner, Delivery = RpcDelivery.Reliable)]
        public void OnSyncRPC(uint serverTick)
        {
            #if Client
            // Calculating the amount of ticks,
            // that happen between the time that the server sends the RPC and the Client received the RPC.
            ulong ms = NetworkRunner.GetInstance().UnityTransport.GetCurrentRtt(NetworkRunner.ServerClientId) / 2;
            float msPerTick = 1000f / SnapshotManager.TickRate;
            int passedTicks = (int)(ms / msPerTick);
            
            uint theLocalTickAtTheTimeWhereThisRPCWasSent = (uint)(SnapshotManager.CurrentTick - passedTicks);
            uint theServerTickNow = (uint)(serverTick + passedTicks);

            int difference = (int)(theLocalTickAtTheTimeWhereThisRPCWasSent - serverTick);

            if (difference < 0)
            {
                // Skip to the server tick if we have to calculate too many ticks to get to the server tick
                if (Mathf.Abs(difference) > 6)
                {
                    Debug.LogWarning("Setting tick, because we are too far behind the server");
                    SnapshotManager.PhysicsTickSystem.SetTick(theServerTickNow + NetworkRunner.NetworkSettings.tickOffsetBuffer);
                }
                // Calculate extra ticks if the difference to the server tick isn't that big
                else
                {
                    Debug.LogWarning("Calculating extra ticks, because we are a bit behind the server");
                    SnapshotManager.PhysicsTickSystem.CalculateExtraTicks((int)(difference + NetworkRunner.NetworkSettings.tickOffsetBuffer));
                }
            }
            else if (difference > NetworkRunner.NetworkSettings.tickOffsetBuffer)
            {
                // Skip to the server tick if we have to calculate too many ticks to get to the server tick
                if (Mathf.Abs(difference) > 6)
                {
                    Debug.LogWarning("Setting tick, because we are too far in front of the server");
                    SnapshotManager.PhysicsTickSystem.SetTick(theServerTickNow + NetworkRunner.NetworkSettings.tickOffsetBuffer);
                }
                // Calculate extra ticks if the difference to the server tick isn't that big
                else
                {
                    Debug.LogWarning("Skipping ticks, because we are a bit in front of the server");
                    SnapshotManager.PhysicsTickSystem.SkipTick((int)(difference - NetworkRunner.NetworkSettings.tickOffsetBuffer + 1));
                }
            }
            else
            {
                // Do nothing, because we are in the sweet spot of tick offset.
                // Debug.Log("We are in the sweet spot");
            }
            
            #endif
        }

        [Rpc(SendTo.Server, Delivery = RpcDelivery.Unreliable)]
        public void OnClientInputsRPC(ClientInputState[] clientInputStates)
        {
            #if Server
            sentInput = true;
            
            foreach (var input in clientInputStates)
            {
                // If this is an "old" input we skip
                if (input.Tick < SnapshotManager.CurrentTick) continue;
                
                if (_inputStates[input.Tick % _inputStates.Length] != null)
                {
                    // We already have the right input, so we skip it
                    if (_inputStates[input.Tick % _inputStates.Length].Tick == input.Tick) continue;
                }
                
                // Save the new input
                _inputStates[input.Tick % _inputStates.Length] = input;
                // Debug - TextWriter.Update(OwnerClientId, input.Tick, input.DirectionalInputs["Move"]);
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
            
            Reconcile(latestGameState);
            #endif
        }
        
        #endregion
        
        #if Server
        public ClientInputState GetInputState(uint tick)
        {
            if (_inputStates == null)
                _inputStates = new ClientInputState[NetworkRunner.NetworkSettings.inputBufferSize];
            
            ClientInputState input = _inputStates[tick % _inputStates.Length];
            if (input != null)
                if (input.Tick == tick)
                {
                    return input;
                }
            
            // Check if last tick's input null is. If it isn't reuse it and save it for this tick
            if (_inputStates[(tick - 1) % _inputStates.Length] != null)
            {
                input = _inputStates[(tick - 1) % _inputStates.Length];
                input.Tick = tick;
                _inputStates[(tick) % _inputStates.Length] = input;
                Debug.Log("USING WRONG (Last) INPUT STATE!!!!!!!!!!!!!");
                return input;
            }
            else
            {
                return null;
            }
        }
        #endif
        
        #if Client
        private void Reconcile(GameState serverGameState)
        {
            GameState localGameState = SnapshotManager.GetGameState(serverGameState.Tick);
            if (localGameState == null) return;
            if (localGameState.States == null) return;
            if (localGameState.States.Count == 0) return;

            Dictionary<PredictedNetworkedObject, IState> predictedStates = new Dictionary<PredictedNetworkedObject, IState>();
            Dictionary<PredictedNetworkedObject, IState> serverStates = new Dictionary<PredictedNetworkedObject, IState>();
            bool shouldReconcile = false;
            
            // Apply all not predicted states.
            ApplyStates(serverGameState, true);
            SnapshotManager.SaveGameState(serverGameState);
            
            foreach (var kvp in serverGameState.States)
            {
                ulong objectId = kvp.Key;
                IState state = kvp.Value;
                
                if (!SnapshotManager.NetworkedObjects.ContainsKey(objectId)) continue;
                
                NetworkedObject networkedObject = SnapshotManager.NetworkedObjects[objectId];
                PredictedNetworkedObject predictedNetworkedObject = null;
                try
                {
                    predictedNetworkedObject = networkedObject as PredictedNetworkedObject;
                }
                catch (Exception e)
                {
                    // Skip this. This isn't a predicted networked object.
                    Debug.Log("Skip this. This isn't a predicted networked object.");
                    continue;
                }
                
                if (predictedNetworkedObject == null) continue;
                if (predictedNetworkedObject.canBeIgnored) continue;
                
                bool canComparePredictedState = true;

                // Try to get the predicted Player State and server Player State
                IState predictedState = null;
                IState serverState = null;
                
                try
                {
                    if (!localGameState.States.TryGetValue(objectId, out predictedState))
                        canComparePredictedState = false;
                    
                    // Try to find our Player in the Server State
                    if (!serverGameState.States.TryGetValue(objectId, out serverState))
                        canComparePredictedState = false;
                }
                catch (Exception)
                {
                    canComparePredictedState = false;
                }
                
                if (canComparePredictedState)
                {
                    predictedStates.Add(predictedNetworkedObject, predictedState);
                    serverStates.Add(predictedNetworkedObject, serverState);

                    ReconciliationType reconciliationType = predictedNetworkedObject.DoWeNeedToReconcile(predictedState, serverState);

                    bool doWeNeedToReconcile = reconciliationType == ReconciliationType.Everything;
                    
                    if (reconciliationType == ReconciliationType.SingleObject)
                        SnapshotManager.ApplyState(objectId, serverGameState.Tick, serverState);
                    
                    shouldReconcile = doWeNeedToReconcile ? doWeNeedToReconcile : shouldReconcile;
                }
                else
                {
                    // If we can't compare, just set it to be save.
                    SnapshotManager.ApplyState(objectId, serverGameState.Tick, serverState);
                }
            }

            if (shouldReconcile)
            {
                // -- RECONCILIATION --
                ApplyStates(serverGameState, false);
                SnapshotManager.TakeSnapshot(serverGameState.Tick);
                
                // Collect Input
                PlayerInputNetworkBehaviour.UpdatePlayersWithAuthority(serverGameState.Tick, true);
                
                // Check if the amount of ticks that we have to recalculate is too big, so that it potentially crashes the game or is bad player experience.
                uint ticksToRecalculate = SnapshotManager.CurrentTick - serverGameState.Tick + 1;
                if (ticksToRecalculate > 20)
                {
                    // Do nothing and leave the client in the past, because we will reconcile correct later.
                    // t ~ 1 second because of the OnSyncRPC method
                    Debug.LogWarning("Can't reconcile because of a potential crash!");
                }
                else
                {
                    // Recalculate every tick
                    for (uint tick = serverGameState.Tick + 1; tick <= SnapshotManager.CurrentTick; tick++)
                        SnapshotManager.RecalculatePhysicsTick(tick);
                
                    Debug.LogWarning("Reconciled!");
                }
            }
        }

        private void ApplyStates(GameState serverGameState, bool skipPredictedObjects)
        {
            foreach (var kvp in serverGameState.States)
            {
                ulong objectId = kvp.Key;
                IState state = kvp.Value;
                if (!SnapshotManager.NetworkedObjects.ContainsKey(objectId)) continue;
                
                NetworkedObject networkedObject = SnapshotManager.NetworkedObjects[objectId];
                PredictedNetworkedObject predictedNetworkedObject = null;
                try
                {
                    predictedNetworkedObject = networkedObject as PredictedNetworkedObject;
                }
                catch (Exception e)
                {
                    // not a predicted object.
                }
                
                bool isPredictedObject = predictedNetworkedObject != null;
                
                if (isPredictedObject && !predictedNetworkedObject.canBeIgnored && skipPredictedObjects) continue;
                
                SnapshotManager.ApplyState(objectId, serverGameState.Tick, state);
            }
        }
        #endif
    }
}