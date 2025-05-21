using CSP.Items;
using CSP.Simulation;
using UnityEngine;

namespace _Project.Scripts.Items.Guns
{
    public class Gun : PickUpItem
    {
        [Header("Gun Settings")]
        [SerializeField] private bool hold;
        [SerializeField] private int magazineSize = 7;
        [SerializeField] private int magazineAmount = 3;
        [SerializeField] private float fireRate = 0.1f;
        
        private int _currentBullets;
        private int _magazinesLeft;
        private float _fireRateTimer;

        private bool _wasShooting;
        
        protected override void SetUp()
        {
            _currentBullets = magazineSize;
            _magazinesLeft = magazineAmount;
        }

        protected override void Use(bool isUsing)
        {
            // Todo: Check if we have bullets left 
            // Todo: Add reloading
            
            if (isUsing)
            {
                if ((hold && _fireRateTimer <= 0) || (!hold && !_wasShooting && _fireRateTimer <= 0))
                    InitiateShooting();
            }
            
            _wasShooting = isUsing;
        }

        private void InitiateShooting()
        {
            _fireRateTimer = fireRate;
            _currentBullets--;
            
            Shoot();
        }
        
        public virtual void Shoot()
        {
            Debug.Log("Shooting");
        }

        protected override void OnTick()
        {
            if (_fireRateTimer > 0)
                _fireRateTimer -= SnapshotManager.PhysicsTickSystem.TimeBetweenTicks;
        }

        protected override void OnPickedUp()
        {
            
        }

        protected override void OnDropped()
        {
            
        }

        protected override void Highlight()
        {
            
        }

        protected override void UnHighlight()
        {
            
        }
        
        public override int GetItemType() => (int) ItemType.Gun;
        
        public override IState GetCurrentState()
        {
            GunState state = new GunState
            {
                CurrentBullets = _currentBullets,
                MagazinesLeft = _magazinesLeft,
                FireRateTimer = _fireRateTimer,
                Position = transform.position,
                Rotation = transform.eulerAngles,
                Velocity = rb.linearVelocity,
                AngularVelocity = rb.angularVelocity,
                Equipped = pickedUp
            };
            
            return state;
        }

        public override void ApplyState(uint tick, IState state)
        {
            GunState gunState = (GunState)state;
            _currentBullets = gunState.CurrentBullets;
            _magazinesLeft = gunState.MagazinesLeft;
            _fireRateTimer = gunState.FireRateTimer;
            pickedUp = gunState.Equipped;
            
            if (gunState.Equipped) return;
            
            transform.position = gunState.Position;
            transform.eulerAngles = gunState.Rotation;
            rb.linearVelocity = gunState.Velocity;
            rb.angularVelocity = gunState.AngularVelocity;
        }

        public override bool DoWeNeedToReconcile(IState predictedStateData, IState serverStateData)
        {
            GunState predictedState = (GunState) predictedStateData;
            GunState serverState = (GunState) serverStateData;
            
            if (predictedState.Equipped != serverState.Equipped)
            {
                Debug.LogWarning("Reconciliation Gun: Equipped");
                return true;
            }
            else if (predictedState.CurrentBullets != serverState.CurrentBullets)
            {
                Debug.LogWarning("Reconciliation Gun: CurrentBullets");
                return true;
            }
            else if (predictedState.MagazinesLeft != serverState.MagazinesLeft)
            {
                Debug.LogWarning("Reconciliation Gun: MagazinesLeft");
                return true;
            }
            else if (!Mathf.Approximately(predictedState.FireRateTimer, serverState.FireRateTimer))
            {
                Debug.LogWarning("Reconciliation Gun: FireRateTimer");
                return true;
            }

            if (!serverState.Equipped)
            {
                if (Vector3.Distance(predictedState.Velocity, serverState.Velocity) >= 0.1f)
                {
                    Debug.LogWarning("Reconciliation Gun: Velocity");
                    return true;
                }
                else if (Vector3.Distance(predictedState.AngularVelocity, serverState.AngularVelocity) >= 0.1f)
                {
                    Debug.LogWarning("Reconciliation Gun: Angular Velocity");
                    return true;
                }
                else  if (Vector3.Distance(predictedState.Position, serverState.Position) >= 0.1f)
                {
                    Debug.LogWarning("Reconciliation Gun: Position");
                    return true;
                }
                else if (Quaternion.Angle(Quaternion.Euler(predictedState.Rotation), Quaternion.Euler(serverState.Rotation)) >= 0.1f)
                {
                    Debug.LogWarning("Reconciliation Gun: Rotation");
                    return true;
                }
            }

            return false;
        }
    }
}