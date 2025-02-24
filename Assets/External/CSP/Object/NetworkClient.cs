using System.Collections.Generic;
using CSP.Data;
using CSP.Input;
using CSP.Simulation;
using Unity.Netcode;
using UnityEngine;

namespace CSP.Object
{
    public class NetworkClient : NetworkBehaviour
    {
        #if Client
        public static NetworkClient LocalClient;
        public static uint WantedBufferSize = 4;
        public static uint WantedBufferSizePositiveTollerance = 3;
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
    }
}