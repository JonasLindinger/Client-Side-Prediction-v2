using Unity.Netcode;
using UnityEngine;

namespace _Project.Scripts.CSP.Connection.Approval
{
    public abstract class ConnectionApproval : MonoBehaviour
    {
        private protected void Awake()
        {
            DontDestroyOnLoad(gameObject);
        }

        public abstract void OnConnectionRequest(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response);
    }
}