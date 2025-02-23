using _Project.Scripts.CSP.Data;
using _Project.Scripts.CSP.Input;
using _Project.Scripts.CSP.Object;

namespace _Project.Scripts.CSP.Simulation
{
    public class NetworkTickSystem : TickSystem
    {
        private InputCollector _inputCollector;
        
        public override void OnTick(uint tick)
        {
            #if Client
            SendLocalInputs();
            #endif
        }

        #if Client
        private void SendLocalInputs()
        {
            if (!_inputCollector)
                _inputCollector = InputCollector.GetInstance();

            // Todo: Replace the 15 with a dynamic amount (Add in Settings?)
            ClientInputState[] inputsToSend = _inputCollector.GetLastInputStates(15);

            // Actually send the inputs
            NetworkClient.LocalClient.OnInputRPC(inputsToSend);
        }
        #endif
    }
}