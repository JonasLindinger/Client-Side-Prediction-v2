using CSP.Data;
using CSP.Input;
using CSP.Object;
using UnityEngine;

namespace CSP.Simulation
{
    public class NetworkTickSystem : TickSystem
    {
        private InputCollector _inputCollector;
        
        public override void OnTick(uint tick)
        {
            #if Client
            // Send inputs to server
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