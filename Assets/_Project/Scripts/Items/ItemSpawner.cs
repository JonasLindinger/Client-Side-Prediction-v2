using CSP.Items;
using Singletons;
using UnityEngine;

namespace _Project.Scripts.Items
{
    public class ItemSpawner : MonoBehaviourSingleton<ItemSpawner>
    {
        [SerializeField] private Transform itemSpawnPoint;
        [SerializeField] private PickUpItem itemPrefab;

        #if Server
        public void SpawnItems()
        {
            PickUpItem item = Instantiate(itemPrefab, itemSpawnPoint.position, itemSpawnPoint.rotation);
            item.NetworkObject.Spawn();
            item = Instantiate(itemPrefab, itemSpawnPoint.position, itemSpawnPoint.rotation);
            item.NetworkObject.Spawn();
            item = Instantiate(itemPrefab, itemSpawnPoint.position, itemSpawnPoint.rotation);
            item.NetworkObject.Spawn();
        }
        #endif
    }
}