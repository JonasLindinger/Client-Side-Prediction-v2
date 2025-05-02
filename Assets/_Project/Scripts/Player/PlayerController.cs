using System.Linq;
using _Project.Scripts.Items;
using _Project.Scripts.Network;
using CSP.Data;
using CSP.Items;
using CSP.Player;
using CSP.Simulation;
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
        [SerializeField] private Transform orientation;
        [SerializeField] private LayerMask whatIsGround;
        [SerializeField] private float playerHeight;
        [SerializeField] private Camera playerCamera;
        [SerializeField] private Transform gunContainer;

        private bool _grounded;
        private bool _readyToJump = true;
        
        private float _xRotation;
        private float _yRotation;

        private Rigidbody _rb;
        private AudioListener _audioListener;
        
        // "Inventory"
        private PickUpItem _equippedItem;
        
        #if Client
        // "Client's Inventory actions"
        private PickUpItem _itemToPickUp;
        private PickUpItem _itemToDrop;
        #endif
        
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
            PickUp(playerInput);
            DropItem(playerInput);
            Look(playerInput);
        }

        public override void OnTick(uint tick, ClientInputState input, bool isReconciliation)
        {
            CheckInventory(input);
            Move(input);
        }

        #region PickUpStuff
        
        #if Client
        #region Local Actions

        private void PickUp(PlayerInput playerInput)
        {
            if (playerInput.actions["PickUp"].ReadValue<float>() > 0.4f) return; // We don't want to pick up, so we return early.
            if (PickUpItem.PickUpAbleItems.Count == 0) return; // No item to pick Up
            
            _itemToPickUp = PickUpItem.PickUpAbleItems.First().Value;
        }

        private void DropItem(PlayerInput playerInput)
        {
            if (playerInput.actions["Drop"].ReadValue<float>() > 0.4f) return; // We don't want to drop, so we return early.
            if (_equippedItem == null) return; // No item to drop
            
            _itemToDrop = _equippedItem;
        }

        #endregion
        #endif

        #region Actions
        private void DropItemAction(ulong itemIdToDrop)
        {
            if (_equippedItem == null) return; // No item to drop
            if (_equippedItem.NetworkObjectId == itemIdToDrop)
            {
                Debug.LogWarning("Something went wrong. We have a different item, so we can't drop the item that we should drop");
                return; // We have a different item, so we can't drop
            }
            
            _equippedItem.Drop(gunContainer, playerCamera);
            _equippedItem = null;
        }

        private void PickUpItemAction(ulong itemIdToPickUp)
        {
            if (_equippedItem.NetworkObjectId == itemIdToPickUp) return; // We already equipped this item, so we don't do anything.
            
            if (_equippedItem != null)
                DropItemAction(_equippedItem.NetworkObjectId);  // Dropping old item if we need to.
            
            if (!PickUpItem.PickUpItems.ContainsKey(itemIdToPickUp))
            {
                Debug.LogWarning("Item to pick up not found!");
                return; // Item doesn't exist?
            }
            
            _equippedItem = PickUpItem.PickUpItems[itemIdToPickUp];
            _equippedItem.PickUp(gunContainer, playerCamera);
        }
        #endregion

        #region Handle Inventory

        private void SetInventory(PlayerState playerState)
        {
            if (playerState.EquippedItem == -1) // We shouldn't have an item equipped
                if (_equippedItem != null) 
                    DropItemAction(_equippedItem.NetworkObjectId);
                else
                {
                    // Everything is fine.
                }
            else
            {
                PickUpItemAction((ulong) playerState.EquippedItem);
            }
        }

        private void CheckInventory(ClientInputState input)
        {
            if (input.Data.GetDataType() != (int) LocalDataTypes.LocalPlayer) return;
            LocalPlayerData playerData = (LocalPlayerData) input.Data;
            if (playerData == null) return;

            if (playerData.ItemToDrop != -1)
                DropItemAction((ulong) playerData.ItemToDrop);
            else if (playerData.ItemToPickUp != -1) 
                PickUpItemAction((ulong) playerData.ItemToPickUp);
        }

        #endregion
        
        #endregion
        
        #region Look

        #if Client
        private void Look(PlayerInput playerInput)
        {
            // Looking
            float mouseX = playerInput.actions["Look"].ReadValue<Vector2>().x * Time.deltaTime * xSensitivity;
            float mouseY = playerInput.actions["Look"].ReadValue<Vector2>().y * Time.deltaTime * ySensitivity;
            
            _yRotation += mouseX;
            
            _xRotation -= mouseY;
            _xRotation = Mathf.Clamp(_xRotation, -90f, 90f);
            
            playerCamera.transform.rotation = Quaternion.Euler(_xRotation, _yRotation, 0);
            orientation.rotation = Quaternion.Euler(0, _yRotation, 0);
        }
        #endif
        
        #endregion
        
        #region Move

        private void Move(ClientInputState input)
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

        #endregion
        
        #region State Stuff

        public override IData GetPlayerData()
        {
            LocalPlayerData localPlayerData = new LocalPlayerData();
            localPlayerData.PlayerRotation = new Vector2(_xRotation, _yRotation);
            
            // Do inventory stuff and reset the items to drop / pick up
            localPlayerData.ItemToDrop = _itemToDrop == null ? -1 : (long) _itemToDrop.NetworkObjectId;
            localPlayerData.ItemToPickUp = _itemToPickUp == null ? -1 : (long) _itemToPickUp.NetworkObjectId;
            _itemToDrop = null;
            _itemToPickUp = null;
            
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
                EquippedItem = (long) _equippedItem.NetworkObjectId,
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

            SetInventory(playerState);
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