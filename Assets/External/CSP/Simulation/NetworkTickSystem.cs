using CSP.Data;
using CSP.Input;
using CSP.Object;
using LindoNoxStudio.Network.Simulation;
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
            #elif Server
            SendGameState();
            #endif
        }

        #if Client
        private void SendLocalInputs()
        {
            if (!_inputCollector)
                _inputCollector = InputCollector.GetInstance();
            
            ClientInputState[] inputsToSend = _inputCollector.GetLastInputStates(
                (int) (NetworkClient.WantedBufferSize + NetworkClient.WantedBufferSizePositiveTollerance)
                );

            // Actually send the inputs
            NetworkClient.LocalClient.OnInputRPC(inputsToSend);
        }
        #elif Server
        private void SendGameState()
        {
            GameState latestGameState = SnapshotManager.GetLatestGameState();
            
            foreach (var kvp in NetworkClient.ClientsByOwnerId)
            {
                NetworkClient client = kvp.Value;
                client.OnServerStateRPC(latestGameState);
            }
        }
        #endif
    }
}