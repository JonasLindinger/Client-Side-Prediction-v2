using _Project.Scripts.CSP.Data;
using _Project.Scripts.Utility;
using UnityEngine;
using UnityEngine.InputSystem;

namespace _Project.Scripts.CSP.Input
{
    [RequireComponent(typeof(PlayerInput))]
    public class InputCollector : MonoBehaviourSingleton<InputCollector>
    {
        [Header("Inputs")]
        [SerializeField] private string[] directionalInputs;
        [SerializeField] private string[] inputFlags;
        
        private PlayerInput _playerInput;
        
        private void Start()
        {
            _playerInput = GetComponent<PlayerInput>();
        }

        public ClientInputState GetClientInputState(uint tick)
        {
            Vector2[] vector2Inputs = new Vector2[directionalInputs.Length];
            bool[] booleanInputs = new bool[inputFlags.Length];
            
            // Check Vector2's
            int i = 0;
            foreach (string input in directionalInputs)
            {
                Vector2 inputVector = _playerInput.actions[input].ReadValue<Vector2>();
                inputVector = Vector2.ClampMagnitude(inputVector, 1f); // Limit to -1 and 1
                vector2Inputs[i] = inputVector;
            }
            
            // Check boolean's
            i = 0;
            foreach (string input in inputFlags)
                booleanInputs[i] = _playerInput.actions[input].ReadValue<float>() >= 0.4f;
            
            ClientInputState clientInputState = new ClientInputState(
                vector2Inputs,
                booleanInputs,
                tick);
            
            return clientInputState;
        }
    }
}