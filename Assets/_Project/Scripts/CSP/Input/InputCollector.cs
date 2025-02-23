using System.Collections.Generic;
using _Project.Scripts.CSP.Data;
using _Project.Scripts.Utility;
using UnityEngine;
using UnityEngine.InputSystem;

namespace _Project.Scripts.CSP.Input
{
    [RequireComponent(typeof(PlayerInput))]
    public class InputCollector : MonoBehaviourSingleton<InputCollector>
    {
        private string[] _directionalInputs;
        private string[] _inputFlags;
        
        private Queue<ClientInputState> _lastInputStates = new Queue<ClientInputState>();
        
        private PlayerInput _playerInput;
        
        private void Start()
        {
            _playerInput = GetComponent<PlayerInput>();

            GetInputsByName();
        }

        private void GetInputsByName()
        {
            List<string> directionalInputs = new List<string>();
            List<string> inputFlags = new List<string>();
            foreach (var action in _playerInput.actions)
            {
                switch (action.type)
                {
                    case InputActionType.Button:
                        inputFlags.Add(action.name);
                        break;
                    case InputActionType.Value:
                        if (action.expectedControlType == "Vector2")
                            directionalInputs.Add(action.name);
                        break;
                    case InputActionType.PassThrough:
                        Debug.LogWarning("Can't handle this input: " + action.name);
                        break;
                }
            }
            
            _directionalInputs = directionalInputs.ToArray();
            _inputFlags = inputFlags.ToArray();
        }

        public ClientInputState GetClientInputState(uint tick)
        {
            Vector2[] vector2Inputs = new Vector2[_directionalInputs.Length];
            bool[] booleanInputs = new bool[_inputFlags.Length];
            
            // Check Vector2's
            int i = 0;
            foreach (string input in _directionalInputs)
            {
                Vector2 inputVector = _playerInput.actions[input].ReadValue<Vector2>();
                inputVector = Vector2.ClampMagnitude(inputVector, 1f); // Limit to -1 and 1
                vector2Inputs[i] = inputVector;
            }
            
            // Check boolean's
            i = 0;
            foreach (string input in _inputFlags)
                booleanInputs[i] = _playerInput.actions[input].ReadValue<float>() >= 0.4f;

            ClientInputState clientInputState = new ClientInputState()
            {
                DirectionalInputs = vector2Inputs,
                InputFlags = booleanInputs,
                Tick = tick,
            };
            
            return clientInputState;
        }

        public void AddInputState(ClientInputState clientInputState)
        {
            _lastInputStates.Enqueue(clientInputState);
        }
        
        public ClientInputState[] GetLastInputStates(int amount)
        {
            // Remove inputs if we have too much
            if (_lastInputStates.Count > amount)
                for (int i = 0; i < _lastInputStates.Count - amount; ++i)
                    _lastInputStates.Dequeue();
            
            return _lastInputStates.ToArray();
        }
    }
}