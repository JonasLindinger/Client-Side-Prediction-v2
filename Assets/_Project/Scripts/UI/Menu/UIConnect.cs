using _Project.Scripts.CSP;
using _Project.Scripts.CSP.Data;
using UnityEngine;

namespace _Project.Scripts.UI.Menu
{
    public class UIConnect : MonoBehaviour
    {
        public void Connect()
        {
            #if Client
            ulong id = (ulong) Random.Range(1111, 9999);
            
            ConnectionPayload payload = new ConnectionPayload();
            payload.DisplayName = "Client: " + id;
            payload.ClientId = id;
            
            NetworkRunner.GetInstance().Run("" , 0, payload);
            #endif
        }
    }
}