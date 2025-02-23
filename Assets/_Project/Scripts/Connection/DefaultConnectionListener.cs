using _Project.Scripts.CSP.Connection.Listener;
using _Project.Scripts.CSP.Object;
using Unity.Netcode;
using UnityEngine;

namespace _Project.Scripts.Connection
{
    public class DefaultConnectionListener : ConnectionListener
    {
        [Header("References")]
        [SerializeField] private NetworkObject networkPlayerPrefab;
        
        public override void OnClientConnected(ConnectionEventData eventData)
        {
            #if Server
            Debug.Log("Client: " + eventData.ClientId + " connected");
            Spawner.SpawnObjectPublicWithOwnerPermanent(networkPlayerPrefab, eventData.ClientId);
            #endif
        }

        public override void OnClientDisconnected(ConnectionEventData eventData)
        {
            #if Server
            Debug.Log("Client: " + eventData.ClientId + " disconnected");
            #endif
        }
    }
}