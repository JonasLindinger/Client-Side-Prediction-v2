using System.Collections.Generic;
using System.Linq;
using _Project.Scripts.CSP.Data;
using _Project.Scripts.Utility;
using UnityEngine;
using UnityEngine.InputSystem;

namespace _Project.Scripts.CSP.Input
{
    [RequireComponent(typeof(PlayerInput))]
    public class InputCollector : MonoBehaviourSingleton<InputCollector>
    {
        public static Dictionary<string, Vector2> DirectionalInputs = new Dictionary<string, Vector2>();
        public static Dictionary<string, bool> InputFlags = new Dictionary<string, bool>();
        public static List<string> DirectionalInputsNames = new List<string>();
        public static List<string> InputFlagsNames = new List<string>();
        
        private Queue<ClientInputState> _lastInputStates = new Queue<ClientInputState>();
        
        private PlayerInput _playerInput;
        
        private void Start()
        {
            _playerInput = GetComponent<PlayerInput>();

            GetInputsByName();
        }

        private void GetInputsByName()
        {
            DirectionalInputs = new Dictionary<string, Vector2>();
            InputFlags = new Dictionary<string, bool>();
            foreach (var action in _playerInput.actions)
            {
                switch (action.type)
                {
                    case InputActionType.Button:
                        InputFlags.Add(action.name, false);
                        InputFlagsNames.Add(action.name);
                        break;
                    case InputActionType.Value:
                        if (action.expectedControlType == "Vector2")
                        {
                            DirectionalInputs.Add(action.name, Vector2.zero);
                            DirectionalInputsNames.Add(action.name);
                        }
                        break;
                    case InputActionType.PassThrough:
                        Debug.LogWarning("Can't handle this input: " + action.name);
                        break;
                }
            }
        }

        public ClientInputState GetClientInputState(uint tick)
        {
            // Update boolean's
            foreach (var action in _playerInput.actions)
            {
                switch (action.type)
                {
                    case InputActionType.Button:
                        InputFlags[action.name] = _playerInput.actions[action.name].ReadValue<float>() >= 0.4f;
                        break;
                    case InputActionType.Value:
                        if (action.expectedControlType == "Vector2")
                        {
                            Vector2 input = _playerInput.actions[action.name].ReadValue<Vector2>();
                            input.x = ClampValue(input.x);
                            input.y = ClampValue(input.y);
                            DirectionalInputs[action.name] = input;
                        }
                        break;
                    case InputActionType.PassThrough:
                        break;
                }
            }

            ClientInputState clientInputState = new ClientInputState()
            {
                InputFlags = InputFlags,
                DirectionalInputs = DirectionalInputs,
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
        
        private static float ClampValue(float value)
        {
            if (value < -0.5f)
                return -1f;
            else if (value > 0.5f)
                return 1f;
            else
                return 0f;
        }
    }
}