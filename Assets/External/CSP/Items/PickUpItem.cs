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
        
        public BoxCollider boxCollider;
        private Rigidbody _rb;
        
        [SerializeField] private float pickUpRange;
        [SerializeField] private float dropForwardForce;
        [SerializeField] private float dropUpwardForce;

        public bool equipped;
        public PlayerInputNetworkBehaviour owner;
        
        #if Client
        private Transform _player;
        #endif
        private bool _usable;

        public override void OnNetworkSpawn()
        {
            PickUpItems.Add(NetworkObjectId, this);
        }

        #if Client
        protected void Update()
        {
            if (equipped) return;
            
            // Get the local Player and if there is non, we just return early.
            if (_player == null)
                _player = PlayerInputNetworkBehaviour.LocalPlayer.transform;
            
            if (_player == null) return;

            Vector3 distanceToPlayer = _player.position - transform.position;
            if (distanceToPlayer.magnitude <= pickUpRange)
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
        protected abstract void Highlight();
        protected abstract void UnHighlight();
        public abstract int GetItemType();

        public bool IsAbleToPickUp(Transform player)
        {
            Vector3 distanceToPlayer = player.position - transform.position;
            return distanceToPlayer.magnitude <= pickUpRange;
        }

        public void TransferOwnerShip(PlayerInputNetworkBehaviour player, Transform gunContainer)
        {
            if (equipped)
            {
                player.OnDropItem((long) NetworkObjectId);
                owner = player;
            }
            else
            {
                equipped = true;
                owner = player;
            }
        }
        
        public void PickUp(PlayerInputNetworkBehaviour player, Transform gunContainer, Camera playerCamera)
        {
            equipped = true;
            owner = player;
        }

        public void Drop(Transform gunContainer, Camera playerCamera)
        {
            equipped = true;
            owner = null;
        }

        public void DropFromOwner()
        {
            if (!equipped) return;
            if (owner == null) return;
            
            owner.OnDropItem((long) NetworkObjectId);
        }
    }
}