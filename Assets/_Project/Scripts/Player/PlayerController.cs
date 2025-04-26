using _Project.Scripts.Network;
using CSP.Data;
using CSP.Player;
using CSP.Simulation;
using CSP.TextDebug;
using UnityEngine;

namespace _Project.Scripts.Player
{
    public class PlayerController : PlayerInputNetworkBehaviour
    {
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

        //private Vector3 _virualPosition;
        
        //private Rigidbody _rb;
        
        public override void OnSpawn()
        {
            //_rb = GetComponent<Rigidbody>();
            //_rb.freezeRotation = true;
        }
        
        public override void OnDespawn()
        {
            
        }
        
        public override void OnTick(uint tick, ClientInputState input, bool isReconciliation)
        {
            transform.position += new Vector3(input.DirectionalInputs["Move"].x, 0, input.DirectionalInputs["Move"].y) * 0.5f;
            return;
            /*
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
            */
        }
        
        private void ResetJump()
        {
            _readyToJump = true;
        }
        
        public override IState GetCurrentState()
        {
            return new PlayerState()
            {
                Position = transform.position,
                Rotation = transform.eulerAngles,
                //Velocity = _rb.linearVelocity,
                //AngularVelocity = _rb.angularVelocity,
            };
        }

        public override void ApplyState(IState state)
        {
            // Return early if state is not PlayerState
            if (!(state is PlayerState playerState))
                return;

            transform.position = playerState.Position;
            transform.eulerAngles = playerState.Rotation;
            //_rb.linearVelocity = playerState.Velocity;
            //_rb.angularVelocity = playerState.AngularVelocity;
        }

        public override bool DoWeNeedToReconcile(IState predictedStateData, IState serverStateData)
        {
            PlayerState predictedState = (PlayerState) predictedStateData;
            PlayerState serverState = (PlayerState) serverStateData;
            
            // If our position is of, we reconcile
            if (Vector3.Distance(predictedState.Position, serverState.Position) >= 0.001f)
                return true;
            // If our rotation is off, we reconcile
            /* We don't do that (at least for now)
            else if (Vector3.Distance(clientState.Rotation, serverState.Rotation) >= 0.001f)
                return true;
            */
            // If our Velocity is of, we reconcile
            else if (Vector3.Distance(predictedState.Velocity, serverState.Velocity) >= 0.01f)
                return true;
            // If our AngularVelocity is of, we reconcile
            else if (Vector3.Distance(predictedState.AngularVelocity, serverState.AngularVelocity) >= 0.01f)
                return true;

            return false;
        }
    }
}