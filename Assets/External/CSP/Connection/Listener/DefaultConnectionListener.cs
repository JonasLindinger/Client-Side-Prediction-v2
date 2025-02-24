using CSP.Connection.Listener;
using Unity.Netcode;
using UnityEngine;

namespace CSP.Connection
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