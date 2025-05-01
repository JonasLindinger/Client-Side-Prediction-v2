using _Project.Scripts.Network;
using CSP.Data;
using CSP.Player;
using CSP.Simulation;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using IState = CSP.Simulation.IState;

namespace _Project.Scripts.Player
{
    [RequireComponent(typeof(Rigidbody))]
    public class PlayerController : PlayerInputNetworkBehaviour
    {
        [Header("Settings")]
        [SerializeField] private float xSensitivity = 3;
        [SerializeField] private float ySensitivity = 3;
        [Space(10)]
        [Header("Settings")]
        [SerializeField] private float walkSpeed;
        [SerializeField] private float sprintSpeed;
        [SerializeField] private float crouchSpeed;
        [SerializeField] private float groundDrag;
        [Space(2)]
        [SerializeField] private float jumpForce; 
        [SerializeField] private float jumpCooldown;
        [SerializeField] private float airMultipier;
        [Space(10)]
        [Header("References")]
        [SerializeField] private Camera playerCamera;
        [SerializeField] private Transform orientation;
        [SerializeField] private LayerMask whatIsGround;
        [SerializeField] private float playerHeight;

        private bool _grounded;
        private bool _readyToJump = true;
        
        private float xRotation;
        private float yRotation;

        private Rigidbody _rb;
        private AudioListener _audioListener;
        
        public override void OnSpawn()
        {
            _rb = GetComponent<Rigidbody>();
            _rb.freezeRotation = true;
            _rb.interpolation = RigidbodyInterpolation.Interpolate;
            
            playerCamera.enabled = IsOwner;
            
            _audioListener = playerCamera.GetComponent<AudioListener>();
            _audioListener.enabled = IsOwner;

            if (IsOwner)
            {
                Cursor.lockState = CursorLockMode.Locked; 
                Cursor.visible = false;
            }
        }
        
        public override void OnDespawn()
        {
            
        }
        
        public override void InputUpdate(PlayerInput playerInput)
        {
            // Looking
            float mouseX = playerInput.actions["Look"].ReadValue<Vector2>().x * Time.deltaTime * xSensitivity;
            float mouseY = playerInput.actions["Look"].ReadValue<Vector2>().y * Time.deltaTime * ySensitivity;
            
            yRotation += mouseX;
            
            xRotation -= mouseY;
            xRotation = Mathf.Clamp(xRotation, -90f, 90f);
            
            playerCamera.transform.rotation = Quaternion.Euler(xRotation, yRotation, 0);
            orientation.rotation = Quaternion.Euler(0, yRotation, 0);
        }

        public override void OnTick(uint tick, ClientInputState input, bool isReconciliation)
        {
            // Apply rotation
            LocalPlayerData playerData = (LocalPlayerData) input.Data;
            orientation.rotation = Quaternion.Euler(0, playerData.PlayerRotation.y, 0);
            
            // Applying movement
            // Setting the drag
            _grounded = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 0.2f, whatIsGround);

            if (_grounded)
                _rb.linearDamping = groundDrag;
            else
                _rb.linearDamping = 0;

            // Calculating movement
            Vector2 moveInput = input.DirectionalInputs["Move"];

            // _orientation.rotation = Quaternion.Euler(0, input.PlayerRotation, 0);
            Vector3 moveDirection = orientation.forward * moveInput.y + orientation.right * moveInput.x;

            // Applying movement

            float moveSpeed = input.InputFlags["Sprint"] ? sprintSpeed : input.InputFlags["Crouch"] ? crouchSpeed : walkSpeed;

            // Grounded
            if (_grounded)
                _rb.AddForce(moveDirection.normalized * (moveSpeed * 10), ForceMode.Force);

            // In air
            else
                _rb.AddForce(moveDirection.normalized * (moveSpeed * 10 * airMultipier), ForceMode.Force);

            // Speed Control
            Vector3 flatVel = new Vector3(_rb.linearVelocity.x, 0, _rb.linearVelocity.z);
            if (flatVel.magnitude > moveSpeed)
            {
                Vector3 limitedVel = flatVel.normalized * moveSpeed;
                _rb.linearVelocity = new Vector3(limitedVel.x, _rb.linearVelocity.y, limitedVel.z);
            }

            if (input.InputFlags["Jump"] && _grounded && _readyToJump)
            {
                // Resetting Y velocity
                _rb.linearVelocity = new Vector3(_rb.linearVelocity.x, 0, _rb.linearVelocity.z);

                // Applying Force
                _rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);

                // Applying Cooldown
                _readyToJump = false;
                Invoke(nameof(ResetJump), jumpCooldown);
            }
        }
        
        private void ResetJump()
        {
            _readyToJump = true;
        }

        #region State Stuff

        public override IData GetPlayerData()
        {
            LocalPlayerData localPlayerData = new LocalPlayerData();
            localPlayerData.PlayerRotation = new Vector2(xRotation, yRotation);
            return localPlayerData;
        }
        
        public override IState GetCurrentState()
        {
            return new PlayerState()
            {
                Position = transform.position,
                Rotation = transform.eulerAngles,
                Velocity = _rb.linearVelocity,
                AngularVelocity = _rb.angularVelocity,
            };
        }

        public override void ApplyState(IState state)
        {
            // Return early if state is not PlayerState
            if (!(state is PlayerState playerState))
                return;

            transform.position = playerState.Position;
            transform.eulerAngles = playerState.Rotation;
            _rb.linearVelocity = playerState.Velocity;
            _rb.angularVelocity = playerState.AngularVelocity;
        }

        public override bool DoWeNeedToReconcile(IState predictedStateData, IState serverStateData)
        {
            PlayerState predictedState = (PlayerState) predictedStateData;
            PlayerState serverState = (PlayerState) serverStateData;
            
            // If our position is of, we reconcile
            if (Vector3.Distance(predictedState.Position, serverState.Position) >= 0.001f)
                return true;
            // If our rotation is off, we reconcile
            // We don't do that (at least for now)
            else if (Vector3.Distance(predictedState.Rotation, serverState.Rotation) >= 0.001f)
                return true;
            // If our Velocity is of, we reconcile
            else if (Vector3.Distance(predictedState.Velocity, serverState.Velocity) >= 0.01f)
                return true;
            // If our AngularVelocity is of, we reconcile
            else if (Vector3.Distance(predictedState.AngularVelocity, serverState.AngularVelocity) >= 0.01f)
                return true;

            return false;
        }
        
        #endregion
    }
}