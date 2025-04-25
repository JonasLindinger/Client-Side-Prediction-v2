using CSP.Data;
using CSP.Input;
using UnityEngine;

namespace CSP.Simulation
{
    public static class SnapshotManager
    {
        public static uint TickRate => PhysicsTickSystem.TickRate;
        public static uint CurrentTick => PhysicsTickSystem.CurrentTick;
            
        public static TickSystem PhysicsTickSystem;

        #if Client
        // Local Client Input Saving
        private static InputCollector _inputCollector;
        private static ClientInputState[] _inputStates;
        #endif
        
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
            // 1. Simulate Physics
            Physics.Simulate(PhysicsTickSystem.TimeBetweenTicks);
            
            #if Client
            // 1.5 Collect Client Input
            ClientInputState clientInputState = GetInputState(tick);
            #endif
            
            // 2. Update all Players (Server moves everyone, Client predicts his own player)
            // Todo: Do Player Controlling
            // PlayerInputBehaviour.UpdatePlayersWithAuthority(tick, false);
        }
        
        #if Client
        private static ClientInputState GetInputState(uint tick)
        {
            if (!_inputCollector)
                _inputCollector = InputCollector.GetInstance();
            
            // If we already collected input for this tick, we reuse it.
            if (_inputStates[tick % _inputStates.Length].Tick == tick)
                return _inputStates[tick % _inputStates.Length];
            
            ClientInputState clientInputState = _inputCollector.GetClientInputState(tick);
            _inputStates[tick % _inputStates.Length] = clientInputState;

            return clientInputState;
        }
        #endif
    }
}