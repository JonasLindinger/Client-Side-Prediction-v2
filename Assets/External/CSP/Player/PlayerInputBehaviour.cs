using System.Collections.Generic;
using CSP.Data;
using CSP.Simulation;
using CSP.Simulation.State;
using Unity.Netcode;
using NetworkClient = CSP.Object.NetworkClient;

namespace CSP.Player
{
    public abstract class PlayerInputBehaviour : NetworkedObject
    {
        #if Client
        public static PlayerInputBehaviour LocalPlayer;
        #endif
            
        private static List<PlayerInputBehaviour> _playersWithAuthority = new List<PlayerInputBehaviour>();

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

        public static void UpdatePlayersWithAuthority(uint tick)
        {
            SnapshotManager.TakeSnapshot(tick);
            
            foreach (PlayerInputBehaviour player in _playersWithAuthority)
            {
                #if Client
                player.OnTick(SnapshotManager.GetInputState(tick));
                #elif Server
                player.OnTick(player._networkClient.GetInputState(tick));
                #endif
            }
        }

        public abstract void OnSpawn();
        public abstract void OnTick(ClientInputState input);
        public abstract void OnDespawn();
    }
}