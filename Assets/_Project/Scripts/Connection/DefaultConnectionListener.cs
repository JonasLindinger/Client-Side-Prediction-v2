using CSP.Connection.Listener;
using CSP.Object;
using Unity.Netcode;
using UnityEngine;

namespace _Project.Scripts.Connection
{
    public class DefaultConnectionListener : ConnectionListener
    {
        public override void OnClientConnected(ConnectionEventData eventData)
        {
            #if Server
            Debug.Log("Client: " + eventData.ClientId + " connected");
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