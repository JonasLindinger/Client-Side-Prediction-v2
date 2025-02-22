using _Project.Scripts.Utility;
using UnityEngine;

namespace _Project.Scripts.CSP.Object
{
    public class Spawner : MonoBehaviourSingleton<Spawner>
    {
        [Header("References")]
        [SerializeField] private NetworkClient networkClientPrefab;
        [SerializeField] private NetworkPlayer networkPlayerPrefab;

        #if Server
        public void SpawnClient(ulong clientId)
        {
            var client = Instantiate(networkClientPrefab);
            client.NetworkObject.AlwaysReplicateAsRoot = false;
            client.NetworkObject.SynchronizeTransform = false;
            client.NetworkObject.ActiveSceneSynchronization = false;
            client.NetworkObject.SceneMigrationSynchronization = false;
            client.NetworkObject.AutoObjectParentSync = false;
            client.NetworkObject.SyncOwnerTransformWhenParented = false;
            client.NetworkObject.AllowOwnerToParent = false;
            
            // Only the client and we as the server should see the object
            client.NetworkObject.SpawnWithObservers = false;
            
            // This object should be destroyed when the owner leaves
            client.NetworkObject.DontDestroyWithOwner = false;
            
            client.NetworkObject.SpawnWithOwnership(clientId);
            client.NetworkObject.NetworkShow(clientId);
        }
        
        public void SpawnPlayer(ulong clientId)
        {
            var player = Instantiate(networkPlayerPrefab);
            player.NetworkObject.AlwaysReplicateAsRoot = false;
            player.NetworkObject.SynchronizeTransform = false;
            player.NetworkObject.ActiveSceneSynchronization = false;
            player.NetworkObject.SceneMigrationSynchronization = false;
            player.NetworkObject.AutoObjectParentSync = false;
            player.NetworkObject.SyncOwnerTransformWhenParented = false;
            player.NetworkObject.AllowOwnerToParent = false;
            
            // This object should be seen by everyone
            player.NetworkObject.SpawnWithObservers = true;
            
            // This object should NOT be destroyed when the owner leaves
            player.NetworkObject.DontDestroyWithOwner = true;
            
            player.NetworkObject.SpawnWithOwnership(clientId);
        }
        
        #endif
    }
}