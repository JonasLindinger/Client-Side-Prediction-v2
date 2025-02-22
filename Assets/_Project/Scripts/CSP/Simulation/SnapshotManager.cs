using _Project.Scripts.CSP.Data;

namespace _Project.Scripts.CSP.Simulation
{
    public static class SnapshotManager
    {
        private static ClientInputState[] _inputStates;

        public static void RegisterInputState(ClientInputState input)
        {
            _inputStates = new ClientInputState[NetworkRunner.NetworkSettings.inputBufferSize];
            _inputStates[input.Tick % _inputStates.Length] = input;
        }

        public static ClientInputState GetInputState(uint tick)
        {
            ClientInputState input = _inputStates[tick % _inputStates.Length];
            return input;
        }
    }
}