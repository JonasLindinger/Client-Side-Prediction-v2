using System.Collections.Generic;
using CSP.Object;
using CSP.Player;
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
        
        [SerializeField] private BoxCollider boxCollider;
        [SerializeField] private float pickUpRange;
        [SerializeField] private float dropForwardForce;
        [SerializeField] private float dropUpwardForce;

        public bool pickedUp;
        public PlayerInputNetworkBehaviour owner;
        
        private Rigidbody _rigidbody;
        private Transform _gunContainer;
        private Camera _playerCamera;
        
        #if Client
        private Transform _player;
        #endif
        private bool _usable;

        public override void OnNetworkSpawn()
        {
            PickUpItems.Add(NetworkObjectId, this);
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
        
        public void PickUp(PlayerInputNetworkBehaviour player, Transform gunContainer, Camera playerCamera)
        {
            owner = player;
            _gunContainer = gunContainer;
            _playerCamera = playerCamera;
            
            pickedUp = true;

            OnPickedUp();
        }

        public void Drop()
        {
            owner = null;
            _gunContainer = null;
            _playerCamera = null;
            
            pickedUp = false;
            
            OnDropped();
        }
    }
}