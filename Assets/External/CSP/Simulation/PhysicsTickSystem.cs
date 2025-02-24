using System;
using CSP.Data;
using CSP.Input;
using CSP.Player;
using UnityEngine;

namespace CSP.Simulation
{
    public class PhysicsTickSystem : TickSystem
    {
        private InputCollector _inputCollector;

        private void Start()
        {
            // Force simulationMode to be script
            Physics.simulationMode = SimulationMode.Script;
        }

        public override void OnTick(uint tick)
        {
            // 1. Simulate Physics
            Physics.Simulate(TickSystemManager.PhysicsTimeBetweenTicks);
            
            #if Client
            // 1.5 Collect Client Input
            ClientInputState clientInputState = GetInputState(tick);
            #endif
            
            // 2. Update all Players (Server moves everyone, Client predicts his own player)
            PlayerInputBehaviour.UpdatePlayersWithAuthority(tick);
        }
        
        #if Client
        private ClientInputState GetInputState(uint tick)
        {
            if (!_inputCollector)
                _inputCollector = InputCollector.GetInstance();
            
            ClientInputState clientInputState = _inputCollector.GetClientInputState(tick);
            SnapshotManager.RegisterInputState(clientInputState);
            _inputCollector.AddInputState(clientInputState);

            return clientInputState;
        }
        #elif Server
        
        #endif
    }
}