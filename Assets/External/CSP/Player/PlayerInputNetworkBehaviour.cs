using System.Collections.Generic;
using CSP.Data;
using CSP.Object;
using CSP.Simulation;
using UnityEngine;

namespace CSP.Player
{
    public abstract class PlayerInputNetworkBehaviour : NetworkedObject
    {
        #if Client
        public static PlayerInputNetworkBehaviour LocalPlayer;
        #endif
        
        private static List<PlayerInputNetworkBehaviour> _playersWithAuthority = new List<PlayerInputNetworkBehaviour>();

        private NetworkClient _networkClient;
        
        public override void OnNetworkSpawn()
        {
            if (IsOwner || IsServer)
                _playersWithAuthority.Add(this);

            #if Client
            _networkClient = NetworkClient.LocalClient;
            LocalPlayer = this;
            #elif Server
            _networkClient = NetworkClient.ClientsByOwnerId[OwnerClientId];
            #endif
            
            OnSpawn();
        }

        public override void OnNetworkDespawn()
        {
            if (IsOwner || IsServer)
                _playersWithAuthority.Remove(this);

            OnDespawn();
        }

        public static void UpdatePlayersWithAuthority(uint tick, bool isReconciliation)
        {
            SnapshotManager.TakeSnapshot(tick);

            foreach (PlayerInputNetworkBehaviour player in _playersWithAuthority)
            {
                #if Client
                player.OnTick(tick, SnapshotManager.GetInputState(tick), isReconciliation);
                #elif Server
                player.OnTick(tick, player._networkClient.GetInputState(tick), isReconciliation);
                #endif
            }
        }

        public abstract void OnSpawn();
        public abstract void OnTick(uint tick, ClientInputState input, bool isReconciliation);
        public abstract void OnDespawn();
    }
}