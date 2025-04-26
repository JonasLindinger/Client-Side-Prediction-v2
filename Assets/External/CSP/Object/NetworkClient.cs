using System;
using System.Collections.Generic;
using CSP.Connection;
using CSP.Data;
using CSP.Input;
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
                // Todo: While reconciliation, check, how many ticks we have to reconcile. If this is too many, just apply the serverState

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
        
        #endregion
    }
}