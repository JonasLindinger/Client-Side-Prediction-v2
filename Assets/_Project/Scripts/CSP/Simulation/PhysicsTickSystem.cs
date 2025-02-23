using _Project.Scripts.CSP.Data;
using _Project.Scripts.CSP.Input;
using _Project.Scripts.CSP.Player;

namespace _Project.Scripts.CSP.Simulation
{
    public class PhysicsTickSystem : TickSystem
    {
        private InputCollector _inputCollector;

        public override void OnTick(uint tick)
        {
            #if Client
            ClientInputState clientInputState = GetInputState(tick);
            #endif
            
            // Update all Players (Server moves everyone, Client predicts his own player)
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