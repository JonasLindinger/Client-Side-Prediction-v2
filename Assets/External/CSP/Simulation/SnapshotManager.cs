namespace CSP.Simulation
{
    public static class SnapshotManager
    {
        public static uint TickRate => PhysicsTickSystem.TickRate;
        public static uint CurrentTick => PhysicsTickSystem.CurrentTick;
            
        public static TickSystem PhysicsTickSystem;

        public static void KeepTrack(uint tickRate, uint startingTickOffset = 0)
        {
            PhysicsTickSystem = new TickSystem(tickRate, startingTickOffset);
            
            PhysicsTickSystem.OnTick += OnTick;
        }

        public static void StopTracking()
        {
            PhysicsTickSystem.OnTick -= OnTick;
            
            PhysicsTickSystem = null;
        }

        public static void Update(float deltaTime) => PhysicsTickSystem?.Update(deltaTime);
        
        private static void OnTick(uint tick)
        {
            
        }
    }
}