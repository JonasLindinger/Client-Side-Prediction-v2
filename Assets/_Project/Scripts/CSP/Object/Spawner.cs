using _Project.Scripts.Utility;
using Unity.Netcode;
using UnityEngine;

namespace _Project.Scripts.CSP.Object
{
    public class Spawner : MonoBehaviourSingleton<Spawner>
    {
        #if Server
        [Header("References")]
        [SerializeField] private NetworkClient networkClientPrefab;

        private void Start()
        {
            #if Server
            // Auto spawn Client object for communication
            NetworkManager.Singleton.OnConnectionEvent += (manager, eventData) =>
            {
                if (eventData.EventType == ConnectionEvent.ClientConnected)
                    SpawnObjectAnonymousWithOwner(networkClientPrefab.NetworkObject, eventData.ClientId);
            };
            #endif
        }

        public static void SpawnObjectAnonymousWithOwner(NetworkObject networkObject, ulong clientId)
        {
            var newObject = Instantiate(networkObject);
            newObject.AlwaysReplicateAsRoot = false;
            newObject.SynchronizeTransform = false;
            newObject.ActiveSceneSynchronization = false;
            newObject.SceneMigrationSynchronization = false;
            newObject.AutoObjectParentSync = false;
            newObject.SyncOwnerTransformWhenParented = false;
            newObject.AllowOwnerToParent = false;
            
            // Only the client and we as the server should see the object
            newObject.SpawnWithObservers = false;
            
            // This object should be destroyed when the owner leaves
            newObject.DontDestroyWithOwner = false;
            
            newObject.SpawnWithOwnership(clientId);
            newObject.NetworkShow(clientId);
        }
        public static void SpawnObjectAnonymousWithOwnerPermanent(NetworkObject networkObject, ulong clientId)
        {
            var newObject = Instantiate(networkObject);
            newObject.AlwaysReplicateAsRoot = false;
            newObject.SynchronizeTransform = false;
            newObject.ActiveSceneSynchronization = false;
            newObject.SceneMigrationSynchronization = false;
            newObject.AutoObjectParentSync = false;
            newObject.SyncOwnerTransformWhenParented = false;
            newObject.AllowOwnerToParent = false;
            
            // Only the client and we as the server should see the object
            newObject.SpawnWithObservers = false;
            
            // This object should not be destroyed when the owner leaves
            newObject.DontDestroyWithOwner = true;
            
            newObject.SpawnWithOwnership(clientId);
            newObject.NetworkShow(clientId);
        }
        public static void SpawnObjectPublicWithOwner(NetworkObject networkObject, ulong clientId)
        {
            var newObject = Instantiate(networkObject);
            newObject.AlwaysReplicateAsRoot = false;
            newObject.SynchronizeTransform = false;
            newObject.ActiveSceneSynchronization = false;
            newObject.SceneMigrationSynchronization = false;
            newObject.AutoObjectParentSync = false;
            newObject.SyncOwnerTransformWhenParented = false;
            newObject.AllowOwnerToParent = false;
            
            // This object should be seen by everyone
            newObject.SpawnWithObservers = true;
            
            // This object should be destroyed when the owner leaves
            newObject.DontDestroyWithOwner = false;
            
            newObject.SpawnWithOwnership(clientId);
        }
        public static void SpawnObjectPublicWithOwnerPermanent(NetworkObject networkObject, ulong clientId)
        {
            var newObject = Instantiate(networkObject);
            newObject.AlwaysReplicateAsRoot = false;
            newObject.SynchronizeTransform = false;
            newObject.ActiveSceneSynchronization = false;
            newObject.SceneMigrationSynchronization = false;
            newObject.AutoObjectParentSync = false;
            newObject.SyncOwnerTransformWhenParented = false;
            newObject.AllowOwnerToParent = false;
            
            // This object should be seen by everyone
            newObject.SpawnWithObservers = true;
            
            // This object should NOT be destroyed when the owner leaves
            newObject.DontDestroyWithOwner = true;
            
            newObject.SpawnWithOwnership(clientId);
        }
        #endif
    }
}