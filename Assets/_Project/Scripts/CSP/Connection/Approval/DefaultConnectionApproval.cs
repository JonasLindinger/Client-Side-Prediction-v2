using Unity.Netcode;

namespace _Project.Scripts.CSP.Connection.Approval
{
    public class DefaultConnectionApproval : ConnectionApproval
    {
        public override void OnConnectionRequest(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
        {
            response.Approved = true;
        }
    }
}