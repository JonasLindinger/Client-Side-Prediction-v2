using Unity.Netcode;
using UnityEngine;

namespace _Project.Scripts.CSP.Connection.Listener
{
    public class DefaultConnectionListener : ConnectionListener
    {
        public override void OnClientConnected(ConnectionEventData eventData)
        {
            Debug.Log("Client: " + eventData.ClientId + " connected");
        }

        public override void OnClientDisconnected(ConnectionEventData eventData)
        {
            Debug.Log("Client: " + eventData.ClientId + " disconnected");
        }
    }
}