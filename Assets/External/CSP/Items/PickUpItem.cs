using System.Collections.Generic;
using CSP.Object;
using CSP.Player;
using CSP.Simulation;
using UnityEngine;
using UnityEngine.Serialization;

namespace CSP.Items
{
    [RequireComponent(typeof(Rigidbody))]
    public abstract class PickUpItem : PredictedNetworkedObject
    {
        #if Client
        public static Dictionary<ulong, PickUpItem> PickUpAbleItems = new Dictionary<ulong, PickUpItem>();
        #endif
        public static Dictionary<ulong, PickUpItem> PickUpItems = new Dictionary<ulong, PickUpItem>();

        [SerializeField] private Transform decoration;
        public Collider collider;
        [SerializeField] private float pickUpRange;
        [SerializeField] private float dropForwardForce;
        [SerializeField] private float dropUpwardForce;

        [HideInInspector] public bool pickedUp;
        public PlayerInputNetworkBehaviour owner;
        
        [HideInInspector] public Rigidbody rigidbody;
        private Transform _playerCamera;
        
        #if Client
        private Transform _player;
        #endif
        private bool _usable;

        public override void OnNetworkSpawn()
        {
            rigidbody = GetComponent<Rigidbody>();
            
            pickedUp = owner != null;
            
            PickUpItems.Add(NetworkObjectId, this);

            if (!pickedUp)
            {
                rigidbody.isKinematic = false;
                collider.isTrigger = false;
                OnDropped();
            }
            else
            {
                rigidbody.isKinematic = true;
                collider.isTrigger = true;
                OnPickedUp();
            }
            
            SetUp();
        }

        #if Client
        protected void Update()
        {
            if (pickedUp)
            {
                if (PickUpAbleItems.ContainsKey(NetworkObjectId))
                {
                    PickUpAbleItems.Remove(NetworkObjectId);
                    UnHighlight();
                }
                return;
            }
            
            // Get the local Player and if there is non, we just return early.
            if (_player == null)
                _player = PlayerInputNetworkBehaviour.LocalPlayer.transform;

            if (_player == null)
            {
                if (PickUpAbleItems.ContainsKey(NetworkObjectId))
                {
                    PickUpAbleItems.Remove(NetworkObjectId);
                    UnHighlight();
                }
                return;
            }

            if (IsAbleToPickUp(_player))
            {
                if (PickUpAbleItems.ContainsKey(NetworkObjectId))
                {
                    // We are good to go
                }
                else
                {
                    // Set this weapon to be able to get picked up
                    PickUpAbleItems.Add(NetworkObjectId, this);
                    Highlight();
                }
            }
            else
            {
                if (PickUpAbleItems.ContainsKey(NetworkObjectId))
                {
                    // Set this weapon to be able to get picked up
                    PickUpAbleItems.Remove(NetworkObjectId);
                    UnHighlight();
                }
                else
                {
                    // We are good to go
                }
            }
        }
        #endif
        
        public void Trigger()
        {
            if (!_usable) return;
            Use();
        }

        protected abstract void SetUp();
        protected abstract void Use();
        protected abstract void OnPickedUp();
        protected abstract void OnDropped();
        protected abstract void Highlight();
        protected abstract void UnHighlight();
        public abstract int GetItemType();

        public bool IsAbleToPickUp(Transform player)
        {
            Vector3 distanceToPlayer = player.position - transform.position;
            return distanceToPlayer.magnitude <= pickUpRange && !pickedUp;
        }
        
        public void PickUp(PlayerInputNetworkBehaviour player, Transform gunContainer, Transform playerCamera)
        {
            owner = player;
            _playerCamera = playerCamera;
            
            pickedUp = true;

            // Make Rigidbody kinematic
            rigidbody.isKinematic = true;
            collider.isTrigger = true;
            
            // Make weapon a child of the gunContainer and move it to default position
            decoration.SetParent(gunContainer);
            decoration.localPosition = Vector3.zero;
            decoration.localRotation = Quaternion.Euler(Vector3.zero);
            
            OnPickedUp();
        }

        public void Drop(uint tick)
        {
            pickedUp = false;
            
            transform.position = decoration.position;
            transform.rotation = decoration.rotation;
            
            decoration.SetParent(transform);
            decoration.localPosition = Vector3.zero;
            decoration.localRotation = Quaternion.Euler(Vector3.zero);
            
            rigidbody.isKinematic = false;

            // Gun carry's momentum of the owner. So ideally the weaponVelocity is the rigidbody velocity of the player.
            rigidbody.linearVelocity = owner.GetLinearVelocity();
            
            // Add Forces
            rigidbody.AddForce(_playerCamera.forward * dropForwardForce, ForceMode.Impulse);
            rigidbody.AddForce(_playerCamera.up * dropUpwardForce, ForceMode.Impulse);
            
            // Add "random" rotation
            float random = tick % 2 == 0 ? 1 : -1;
            rigidbody.AddTorque(new Vector3(random, random, random) * 10);
            
            collider.isTrigger = false;
            
            OnDropped();
            
            owner = null;
            _playerCamera = null;
        }
    }
}