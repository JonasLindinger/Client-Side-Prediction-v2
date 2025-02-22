using Unity.Netcode;

namespace _Project.Scripts.CSP.Simulation
{
    public class TickSync : NetworkBehaviour
    {
        public override void OnNetworkSpawn()
        {
            #if Server
            OnTickSystemInfoRPC(
                TickSystemManager.PhysicsTickRate,
                TickSystemManager.NetworkTickRate,
                TickSystemManager.CurrentTick);
            #endif
        }

        [Rpc(SendTo.Owner, Delivery = RpcDelivery.Reliable)]
        private void OnTickSystemInfoRPC(uint physicsTickRate, uint networkTickRate, uint tickOffset)
        {
            #if Client
            TickSystemManager.GetInstance().StartTickSystems(physicsTickRate, networkTickRate, 1, tickOffset);
            NetworkManager.Singleton.NetworkConfig.TickRate = networkTickRate;
            #endif
        }
    }
}