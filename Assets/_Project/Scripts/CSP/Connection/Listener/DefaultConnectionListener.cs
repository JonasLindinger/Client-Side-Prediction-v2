using _Project.Scripts.CSP.Object;
using Unity.Netcode;
using UnityEngine;

namespace _Project.Scripts.CSP.Connection.Listener
{
    public class DefaultConnectionListener : ConnectionListener
    {
        public override void OnClientConnected(ConnectionEventData eventData)
        {
            #if Server
            Debug.Log("Client: " + eventData.ClientId + " connected");
            
            // Spawn Client object for this client
            Spawner.GetInstance().SpawnClient(eventData.ClientId);
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