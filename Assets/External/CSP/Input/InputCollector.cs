using System.Collections.Generic;
using System.Linq;
using CSP.Data;
using _Project.Scripts.Utility;
using Singletons;
using UnityEngine;
using UnityEngine.InputSystem;

namespace CSP.Input
{
    [RequireComponent(typeof(PlayerInput))]
    public class InputCollector : MonoBehaviourSingleton<InputCollector>
    {
        private static Dictionary<string, Vector2> _directionalInputs = new Dictionary<string, Vector2>();
        private static Dictionary<string, bool> _inputFlags = new Dictionary<string, bool>();
        public static List<string> DirectionalInputNames = new List<string>();
        public static List<string> InputFlagNames = new List<string>();
        
        private Queue<ClientInputState> _lastInputStates = new Queue<ClientInputState>();
        
        private PlayerInput _playerInput;
        
        private void Start()
        {
            _playerInput = GetComponent<PlayerInput>();

            GetInputsByName();
        }

        private void GetInputsByName()
        {
            _directionalInputs = new Dictionary<string, Vector2>();
            _inputFlags = new Dictionary<string, bool>();
            foreach (var action in _playerInput.currentActionMap.actions)
            {
                switch (action.type)
                {
                    case InputActionType.Button:
                        _inputFlags.Add(action.name, false);
                        InputFlagNames.Add(action.name);
                        Debug.Log("Add: " + action.name);
                        break;
                    case InputActionType.Value:
                        if (action.expectedControlType == "Vector2")
                        {
                            _directionalInputs.Add(action.name, Vector2.zero);
                            DirectionalInputNames.Add(action.name);
                            Debug.Log("Add: " + action.name);
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
            foreach (var inputFlag in _inputFlags.Keys.ToArray())
            {
                _inputFlags[inputFlag] = _playerInput.actions[inputFlag].ReadValue<float>() >= 0.4f;
            }
            
            // Update Vector2's
            foreach (var directionalInput in _directionalInputs.Keys.ToArray())
            {
                Vector2 input = _playerInput.actions[directionalInput].ReadValue<Vector2>();
                input.Normalize();
                _directionalInputs[directionalInput] = input;
            }

            ClientInputState clientInputState = new ClientInputState()
            {
                InputFlags = _inputFlags,
                DirectionalInputs = _directionalInputs,
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