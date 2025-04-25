using CSP.Simulation;
using Unity.Netcode;
using NetworkClient = CSP.Object.NetworkClient;

namespace CSP.Connection
{
    public static class CommunicationManager
    {
        public static uint TickRate => _communicationTickSystem.TickRate;
        
        private static TickSystem _communicationTickSystem;
        
        #if Server
        // Syncs the physics tick to compare. informs the client about the amount of inputs are buffered.
        private static TickSystem _syncTickSystem; 
        #endif

        public static void StartCommunication(uint tickRate)
        {
            NetworkManager.Singleton.NetworkConfig.TickRate = tickRate;
            
            _communicationTickSystem = new TickSystem(tickRate);
            #if Server
            _syncTickSystem = new TickSystem(1);
            #endif
            
            _communicationTickSystem.OnTick += OnTick;
            #if Server
            _syncTickSystem.OnTick += OnSyncTick;
            #endif
        }

        public static void StopCommunication()
        {
            _communicationTickSystem.OnTick -= OnTick;
            #if Server
            _syncTickSystem.OnTick -= OnSyncTick;
            #endif
            
            _communicationTickSystem = null;
            #if Server
            _syncTickSystem = null;
            #endif
        }
        
        public static void Update(float deltaTime)
        {
            _communicationTickSystem?.Update(deltaTime);
            #if Server
            _syncTickSystem?.Update(deltaTime);
            #endif
        }

        private static void OnTick(uint tick)
        {
            
        }

        #if Server
        private static void OnSyncTick(uint tick)
        {
            foreach (var kvp in NetworkClient.ClientsByOwnerId) 
            {
                var clientId = kvp.Key;
                var client = kvp.Value;

                client.OnSyncRPC(SnapshotManager.CurrentTick);
            }
        }
        #endif
    }
}