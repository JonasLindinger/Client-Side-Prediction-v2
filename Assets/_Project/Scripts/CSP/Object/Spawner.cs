using _Project.Scripts.Utility;
using Unity.Netcode;
using UnityEngine;

namespace _Project.Scripts.CSP.Object
{
    public class Spawner : MonoBehaviourSingleton<Spawner>
    {
        [Header("References")]
        [SerializeField] private NetworkClient networkClientPrefab;
        
        #if Server

        private void Start()
        {
            #if Server
            // Auto spawn Client object for communication
            NetworkManager.Singleton.OnConnectionEvent += (manager, eventData) =>
            {
                if (eventData.EventType == ConnectionEvent.ClientConnected)
                    SpawnObjectAnonymousWithOwner(networkClientPrefab.gameObject, eventData.ClientId);
            };
            #endif
        }
        
        public static void _SpawnObjectAnonymousWithOwner(GameObject networkObject, ulong clientId)
        {
            var newObject = Instantiate(networkObject.gameObject).GetComponent<NetworkObject>();
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
        
        public static void _SpawnObjectAnonymousWithOwnerPermanent(GameObject networkObject, ulong clientId)
        {
            var newObject = Instantiate(networkObject.gameObject).GetComponent<NetworkObject>();
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
        
        public static void _SpawnObjectPublicWithOwner(GameObject networkObject, ulong clientId)
        {
            var newObject = Instantiate(networkObject.gameObject).GetComponent<NetworkObject>();
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
        
        public static void _SpawnObjectPublicWithOwnerPermanent(GameObject networkObject, ulong clientId)
        {
            var newObject = Instantiate(networkObject.gameObject).GetComponent<NetworkObject>();
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