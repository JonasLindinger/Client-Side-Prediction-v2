using Unity.Netcode;
using UnityEngine;

namespace _Project.Scripts.Network
{
    public class PlayerSpawner : NetworkBehaviour
    {
        [Header("References")]
        [SerializeField] private NetworkObject networkPlayerPrefab;
        #if Server
        public override void OnNetworkSpawn()
        {
            Spawner.SpawnObjectPublicWithOwnerPermanent(networkPlayerPrefab.gameObject, OwnerClientId);
        }
        #endif
    }
}