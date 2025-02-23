namespace _Project.Scripts.CSP.Simulation
{
    public static class SnapshotManager
    {
        #if Client
        private static ClientInputState[] _inputStates;
        
        public static void RegisterInputState(ClientInputState input)
        {
            if (_inputStates == null)
                _inputStates = new ClientInputState[NetworkRunner.NetworkSettings.inputBufferSize];
            _inputStates[input.Tick % _inputStates.Length] = input;
        }

        public static ClientInputState GetInputState(uint tick)
        {
            if (_inputStates == null)
                _inputStates = new ClientInputState[NetworkRunner.NetworkSettings.inputBufferSize];
            return _inputStates[tick % _inputStates.Length];
        }
        #endif
    }
}