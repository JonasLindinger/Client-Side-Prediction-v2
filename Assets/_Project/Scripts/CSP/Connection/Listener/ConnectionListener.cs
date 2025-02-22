using Unity.Netcode;
using UnityEngine;

namespace _Project.Scripts.CSP.Connection.Listener
{
    public abstract class ConnectionListener : MonoBehaviour
    {
        private protected void Awake()
        {
            DontDestroyOnLoad(gameObject);
        }
        
        public void OnConnectionEvent(NetworkManager networkManager, ConnectionEventData eventData)
        {
            switch (eventData.EventType)
            {
                case ConnectionEvent.ClientConnected:
                    OnClientConnected(eventData);
                    break;
                case ConnectionEvent.ClientDisconnected:
                    OnClientDisconnected(eventData);
                    break;
            }
        }

        public abstract void OnClientConnected(ConnectionEventData eventData);
        public abstract void OnClientDisconnected(ConnectionEventData eventData);
    }
}