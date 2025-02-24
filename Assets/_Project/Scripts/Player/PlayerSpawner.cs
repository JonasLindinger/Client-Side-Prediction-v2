using CSP.Object;
using Unity.Netcode;
using UnityEngine;

namespace _Project.Scripts.Player
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