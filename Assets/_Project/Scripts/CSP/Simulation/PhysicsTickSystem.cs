using _Project.Scripts.CSP.Data;
using _Project.Scripts.CSP.Input;

namespace _Project.Scripts.CSP.Simulation
{
    public class PhysicsTickSystem : TickSystem
    {
        private InputCollector _inputCollector;

        public override void OnTick(uint tick)
        {
            #if Client
            ClientInputState clientInputState = GetInputState(tick);
            #elif Server
            
            #endif
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