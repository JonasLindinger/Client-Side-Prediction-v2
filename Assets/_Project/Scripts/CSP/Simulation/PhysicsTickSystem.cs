using _Project.Scripts.CSP.Data;
using _Project.Scripts.CSP.Input;

namespace _Project.Scripts.CSP.Simulation
{
    public class PhysicsTickSystem : TickSystem
    {
        private InputCollector _inputCollector;

        public override void OnTick(uint tick)
        {
            ClientInputState clientInputState = GetInputState(tick);
            
        }

        private ClientInputState GetInputState(uint tick)
        {
            if (!_inputCollector)
                _inputCollector = InputCollector.GetInstance();
            
            ClientInputState clientInputState = _inputCollector.GetClientInputState(tick);
            SnapshotManager.RegisterInputState(clientInputState);

            return clientInputState;
        }
    }
}