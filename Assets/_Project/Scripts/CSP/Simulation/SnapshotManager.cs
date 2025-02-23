using System.Collections.Generic;
using _Project.Scripts.CSP.Data;
using _Project.Scripts.CSP.Input;
using UnityEngine;

namespace _Project.Scripts.CSP.Simulation
{
    public static class SnapshotManager
    {
        private static ClientInputState[] _inputStates;

        private static ClientInputState _emptyInputState;
        
        public static void RegisterInputState(ClientInputState input)
        {
            _inputStates = new ClientInputState[NetworkRunner.NetworkSettings.inputBufferSize];
            _inputStates[input.Tick % _inputStates.Length] = input;
        }

        public static ClientInputState GetInputState(uint tick)
        {
            ClientInputState input = _inputStates[tick % _inputStates.Length];
            if (input.Tick == tick) return input;

            // Check if last tick's input null is. If it isn't reuse it and save it for this tick
            if (_inputStates[(tick - 1) % _inputStates.Length] != null)
            {
                input = _inputStates[(tick - 1) % _inputStates.Length];
                input.Tick = tick;
                _inputStates[(tick) % _inputStates.Length] = input;
                return input;
            }
            else
            {
                if (_emptyInputState == null)
                {
                    Dictionary<string, bool> inputFlags = new Dictionary<string, bool>();
                    foreach (string inputName in InputCollector.InputFlagNames)
                        inputFlags.Add(inputName, false);
                    
                    Dictionary<string, Vector2> directionalInputs = new Dictionary<string, Vector2>();
                    foreach (string inputName in InputCollector.DirectionalInputNames)
                        directionalInputs.Add(inputName, Vector2.zero);
                    
                    _emptyInputState = new ClientInputState()
                    {
                        InputFlags = inputFlags,
                        DirectionalInputs = directionalInputs,
                    };
                }
                
                ClientInputState emptyInputForThisTick = _emptyInputState;
                emptyInputForThisTick.Tick = tick;
                
                return emptyInputForThisTick;
            }
        }
    }
}