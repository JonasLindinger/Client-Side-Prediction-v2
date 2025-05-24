using System.Linq;
using _Project.Scripts.Items;
using _Project.Scripts.Network;
using CSP.Data;
using CSP.Items;
using CSP.Object;
using CSP.Player;
using CSP.Simulation;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;
using IState = CSP.Simulation.IState;

namespace _Project.Scripts.Player
{
    [RequireComponent(typeof(Rigidbody))]
    public class PlayerController : PlayerInputNetworkBehaviour, IDamageable
    {
        [Header("Mouse Settings")]
        [SerializeField] private float xSensitivity = 3;
        [SerializeField] private float ySensitivity = 3;
        [Space(10)]
        [Header("Move Settings")]
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
        [SerializeField] private Transform decoration;
        [SerializeField] private int localPlayerMask;
        [SerializeField] private int otherplayerMask;
        [SerializeField] private LayerMask whatIsGround;
        [SerializeField] private float playerHeight;
        [SerializeField] private Camera playerCamera;
        [SerializeField] private Transform gunContainer;
        
        private int _health = 100;

        private bool _grounded;
        private float _jumpCooldownTimer;
        
        private float _xRotation;
        private float _yRotation;

        private Vector2 _latestOrientation;
        
        private Rigidbody _rb;
        private AudioListener _audioListener;
        
        // "Inventory"
        private PickUpItem _equippedItem;
        
        #if Client
        // "Client's Inventory actions"
        private PickUpItem _itemToPickUp;
        private bool _dropItem;
        #endif
        
        public override void OnSpawn()
        {
            SetGameLayerRecursive(gameObject, IsOwner ? localPlayerMask : otherplayerMask);
            
            _rb = GetComponent<Rigidbody>();
            _rb.freezeRotation = true;
            _rb.interpolation = RigidbodyInterpolation.Interpolate;
            
            playerCamera.enabled = IsOwner;
            
            _audioListener = playerCamera.GetComponent<AudioListener>();
            _audioListener.enabled = IsOwner;

            #if Client
            if (IsOwner)
            {
                Cursor.lockState = CursorLockMode.Locked; 
                Cursor.visible = false;
            }
            #endif
        }
        
        public override void OnDespawn()
        {
            
        }
        
        public override void InputUpdate(PlayerInput playerInput)
        {
            #if Client
            PickUp(playerInput);
            DropItem(playerInput);
            Look(playerInput);
            #endif
        }

        public override void OnTick(uint tick, ClientInputState input, bool isReconciliation)
        {
            CheckInventory(input);
            CheckCurrentItem(input, input.LatestReceivedServerGameStateTick);
            Move(input);
        }

        #region PickUpStuff

        private void CheckCurrentItem(ClientInputState input, uint latestReceivedServerGameStateTick)
        {
            if (_equippedItem == null) return;
            
            _equippedItem.Trigger(input.InputFlags["Use"], latestReceivedServerGameStateTick);
        }
        
        #if Client
        #region Local Actions

        private void PickUp(PlayerInput playerInput)
        {
            if (!(playerInput.actions["Pick Up"].ReadValue<float>() > 0.4f)) return;
            if (_itemToPickUp != null) return;
            if (PickUpItem.PickUpAbleItems.Count == 0) return; // No item to pick Up
            _itemToPickUp = PickUpItem.PickUpAbleItems.First().Value;
        }

        private void DropItem(PlayerInput playerInput)
        {
            if (!(playerInput.actions["Drop"].ReadValue<float>() > 0.4f)) return;
            if (_dropItem) return;

            _dropItem = _equippedItem;
        }

        #endregion
        #endif

        #region Handle Inventory

        private void CheckInventory(ClientInputState input)
        {
            LocalPlayerData data = (LocalPlayerData) input.Data;

            if (data.DropItem)
            {
                if (_equippedItem == null) return;
                _equippedItem.Drop(input.Tick);
                _equippedItem = null;
            }
            else if (data.ItemToPickUp != -1)
            {
                // Check if item exists
                if (!PickUpItem.PickUpItems.ContainsKey((ulong) data.ItemToPickUp)) return;
                
                // Check if we have an item picked up. If we have, drop it.
                if (_equippedItem != null) 
                    _equippedItem.Drop(input.Tick);
                
                PickUpItem item = PickUpItem.PickUpItems[(ulong) data.ItemToPickUp];
                
                // If the item is out of range or the item is already picked up, return
                if (!item.IsAbleToPickUp(transform)) return;
                
                item.PickUp(this, gunContainer, playerCamera.transform);
                _equippedItem = item;
            }
        }
        
        private void SetInventory(uint tick, PlayerState playerState)
        {
            // We shouldn't have a weapon, and we don't have one
            if (playerState.EquippedItem == -1 && _equippedItem == null) return;
            
            // We should have an item, and we have one. (is it the right one?)
            else if (playerState.EquippedItem != -1 && _equippedItem != null)
            {
                // We have the correct item
                if (playerState.EquippedItem == (long) _equippedItem.NetworkObjectId) return;
                
                // We don't have the correct item, so we drop the currect item
                _equippedItem.Drop(tick);
                
                // If our new weapon doesn't exist, return
                if (!PickUpItem.PickUpItems.ContainsKey((ulong) playerState.EquippedItem))
                {
                    Debug.LogWarning("Item not found");
                    return;
                }
                
                PickUpItem item = PickUpItem.PickUpItems[(ulong) playerState.EquippedItem];

                // If our new item is picked up, drop it.
                if (item.pickedUp)
                    item.Drop(tick);
                
                item.PickUp(this, gunContainer, playerCamera.transform);
                _equippedItem = item;
            }
            
            // We have an item, but we shouldn't have one.
            else if (playerState.EquippedItem == -1)
            {
                _equippedItem.Drop(tick);
                _equippedItem = null;
            }
            
            // We have no item, but we should have one.
            else if (_equippedItem == null)
            {
                // If the weapon doesn't exist, return
                if (!PickUpItem.PickUpItems.ContainsKey((ulong) playerState.EquippedItem))
                {
                    Debug.LogWarning("Item not found");
                    return;
                }
                
                PickUpItem item = PickUpItem.PickUpItems[(ulong) playerState.EquippedItem];
                
                // If this item is picked up, drop it.
                if (item.pickedUp)
                    item.Drop(tick);
                
                item.PickUp(this, gunContainer, playerCamera.transform);
                _equippedItem = item;
            }
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
            decoration.rotation = Quaternion.Euler(0, _yRotation, 0);
        }
        #endif
        
        #endregion
        
        #region Move

        private void Move(ClientInputState input)
        {
            if (_jumpCooldownTimer > 0)
                _jumpCooldownTimer -= SnapshotManager.PhysicsTickSystem.TimeBetweenTicks;

            LocalPlayerData playerData = (LocalPlayerData)input.Data;
            _latestOrientation = playerData.PlayerRotation;

            ApplyLatestCameraState();
            
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

            if (input.InputFlags["Jump"] && _grounded && _jumpCooldownTimer <= 0)
            {
                // Resetting Y velocity
                _rb.linearVelocity = new Vector3(_rb.linearVelocity.x, 0, _rb.linearVelocity.z);

                // Applying Force
                _rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);

                // Applying Cooldown
                _jumpCooldownTimer = jumpCooldown;
            }
        }

        #endregion
        
        #region State Stuff
        
        public override IData GetPlayerData()
        {
            LocalPlayerData localPlayerData = new LocalPlayerData();
        
            #if Client
            localPlayerData.PlayerRotation = new Vector2(_xRotation, _yRotation);
            
            // Do inventory stuff and reset the items to drop / pick up
            localPlayerData.ItemToPickUp = (short) (_itemToPickUp == null ? -1 : (long) _itemToPickUp.NetworkObjectId);
            localPlayerData.DropItem = _dropItem;

            _itemToPickUp = null;
            _dropItem = false;
            #endif
            
            return localPlayerData;
        }

        public override IState GetCurrentState()
        {
            short equippedItem = (short) (_equippedItem == null ? -1 : (long) _equippedItem.NetworkObjectId);
            
            return new PlayerState()
            {
                Position = transform.position,
                Rotation = new Vector2(playerCamera.transform.eulerAngles.x, orientation.eulerAngles.y),
                Velocity = _rb.linearVelocity,
                AngularVelocity = _rb.angularVelocity,
                JumpCooldownTimer = _jumpCooldownTimer,
                EquippedItem = equippedItem,
                Health = _health,
            };
        }

        public override void ApplyState(uint tick, IState state)
        {
            // Return early if state is not PlayerState
            if (!(state is PlayerState playerState))
                return;

            _rb.position = playerState.Position;
            Physics.SyncTransforms();
            _latestOrientation = playerState.Rotation;
            _rb.linearVelocity = playerState.Velocity;
            _rb.angularVelocity = playerState.AngularVelocity;
            _jumpCooldownTimer = playerState.JumpCooldownTimer;
            _health = playerState.Health;
            
            SetInventory(tick, playerState);
            ApplyLatestCameraState();
        }

        public override ReconciliationMethod DoWeNeedToReconcile(IState predictedStateData, IState serverStateData)
        {
            PlayerState predictedState = (PlayerState) predictedStateData;
            PlayerState serverState = (PlayerState) serverStateData;
            
            if (Vector3.Distance(predictedState.Position, serverState.Position) >= 0.1f)
            {
                Debug.LogWarning("Reconciliation Player: Position");
                return ReconciliationMethod.World;
            }
            /* DON'T SYNC ROTATION (BUGGY AND USELESS)
            else if (Vector2.Distance(predictedState.Rotation, serverState.Rotation) >= 2f)
            {
                Debug.LogWarning("Reconciliation Player: Rotation");
                return ReconciliationMethod.World;
            }
            */
            else if (Vector3.Distance(predictedState.Velocity, serverState.Velocity) >= 3f)
            {                
                Debug.LogWarning("Reconciliation Player: Velocity");
                return ReconciliationMethod.World;
            }
            else if (Vector3.Distance(predictedState.AngularVelocity, serverState.AngularVelocity) >= 0.1f)
            {                
                Debug.LogWarning("Reconciliation Player: Angular Velocity");
                return ReconciliationMethod.World;
            }
            else if (!Mathf.Approximately(predictedState.JumpCooldownTimer, serverState.JumpCooldownTimer))
            {                
                Debug.LogWarning("Reconciliation Player: JumpCooldownTimer");
                return ReconciliationMethod.World;
            }
            else if (predictedState.EquippedItem != serverState.EquippedItem)
            {                
                Debug.LogWarning("Reconciliation Player: EquippedItem");
                return ReconciliationMethod.World;
            }
            else if (predictedState.Health != serverState.Health)
            {
                Debug.LogWarning("Reconciliation Player: Health");
                return ReconciliationMethod.World;
            }

            return ReconciliationMethod.None;
        }
        
        public override Vector3 GetLinearVelocity()
        {
            return _rb.linearVelocity;
        }

        public override void ApplyLatestCameraState()
        {
            decoration.rotation = Quaternion.Euler(0, _latestOrientation.y, 0);
            orientation.rotation = Quaternion.Euler(0, _latestOrientation.y, 0);
            if (IsOwner) return;
            playerCamera.transform.rotation = Quaternion.Euler(_latestOrientation.x, _latestOrientation.y, 0);
        }

        #endregion

        public void TakeDamage(int damage)
        {
            Debug.Log("-" + damage);
            
            // Subtract damage from health and clamp it to be >= 0
            _health = damage > _health ? 0 : _health - damage;
            
            if (_health == 0)
            {
                // TODO: Handle death
                // Todo: Do animation
                Debug.Log("Player is dead");
            }
            else
            {
                // Do nothing
            }
        }
        
        private static void SetGameLayerRecursive(GameObject go, int layer)
        {
            go.layer = layer;
            foreach (Transform child in go.transform)
            {
                child.gameObject.layer = layer;

                Transform hasChildren = child.GetComponentInChildren<Transform>();
                if (hasChildren != null)
                    SetGameLayerRecursive(child.gameObject, layer);
            }
        }
    }
}