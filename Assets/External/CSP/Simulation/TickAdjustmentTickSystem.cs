using CSP.Object;

namespace CSP.Simulation
{
    public class TickAdjustmentTickSystem : TickSystem
    {
        public override void OnTick(uint tick)
        {
            #if Server
            SendTick();
            #endif
        }
        
        #if Server
        private void SendTick()
        {
            foreach (var kvp in NetworkClient.ClientsByOwnerId)
            {
                NetworkClient client = kvp.Value;
                client.OnServerTickRPC(TickSystemManager.CurrentTick);
            }
        }
        #endif
    }
}