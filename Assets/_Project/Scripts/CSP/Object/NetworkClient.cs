using _Project.Scripts.CSP.Data;
using _Project.Scripts.CSP.Simulation;
using Unity.Netcode;
using UnityEngine;

namespace _Project.Scripts.CSP.Object
{
    [RequireComponent(typeof(TickSync))]
    public class NetworkClient : NetworkBehaviour
    {
        public static NetworkClient LocalClient;

        public override void OnNetworkSpawn()
        {
            #if Client
            // We don't need to check for isOwner, because this object is only seen by the server and the owner of this object.
            LocalClient = this;
            #endif
        }

        [Rpc(SendTo.Server, Delivery = RpcDelivery.Unreliable)]
        public void OnInputRPC(ClientInputState[] clientInputStates)
        {
            #if Server
            foreach (var input in clientInputStates)
            {
                // If this is an "old" input we skip
                if (input.Tick <= TickSystemManager.CurrentTick) continue;
                SnapshotManager.RegisterInputState(input);
                Debug.Log("Got Input");
            }

            if (clientInputStates.Length < 1) return;
            Debug.Log("Count: " + clientInputStates[0].InputFlags.Count);
            foreach (var inputFlag in clientInputStates[0].InputFlags)
            {
                Debug.Log("Flag: " + inputFlag);
            }
            #endif
        }
    }
}