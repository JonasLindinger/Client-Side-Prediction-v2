using _Project.Scripts.CSP.Connection.Approval;
using _Project.Scripts.CSP.Data;
using Unity.Netcode;

namespace _Project.Scripts.Connection
{
    public class DefaultConnectionApproval : ConnectionApproval
    {
        public override void OnConnectionRequest(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
        {
            ConnectionPayload payload = new ConnectionPayload();
            payload.LoadFromPayload(request.Payload);

            // Check if Payload is InValid
            if (!IsValidPayload(payload))
            {
                response.Reason = "Invalid payload";
                response.Approved = false;
            }
            
            // Todo: Check Reconnect
            
            response.Approved = true;
        }

        /// <summary>
        /// Takes in a payload. If the payload is valid, we return true. If it's not, we return false
        /// </summary>
        /// <param name="payload"></param>
        /// <returns></returns>
        private bool IsValidPayload(ConnectionPayload payload)
        {
            if (payload.ClientId == 0)
                return false;
            
            if (string.IsNullOrEmpty(payload.DisplayName.Value))
                return false;

            return true;
        }
    }
}