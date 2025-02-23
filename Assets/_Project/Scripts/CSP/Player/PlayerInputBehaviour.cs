using System.Collections.Generic;
using _Project.Scripts.CSP.Data;
using _Project.Scripts.CSP.Simulation;
using Unity.Netcode;

namespace _Project.Scripts.CSP.Player
{
    public abstract class PlayerInputBehaviour : NetworkBehaviour
    {
        private static List<PlayerInputBehaviour> _playersWithAuthority;

        public override void OnNetworkSpawn()
        {
            if (IsOwner || IsServer)
                _playersWithAuthority.Add(this);

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
            foreach (PlayerInputBehaviour player in _playersWithAuthority)
            {
                player.OnTick(SnapshotManager.GetInputState(tick));
            }
        }

        public abstract void OnSpawn();
        public abstract void OnTick(ClientInputState input);
        public abstract void OnDespawn();
    }
}