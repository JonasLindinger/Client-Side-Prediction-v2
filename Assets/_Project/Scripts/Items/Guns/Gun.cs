using CSP;
using CSP.Items;
using CSP.Object;
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
        [SerializeField] private int localPlayerMask;
        [SerializeField] private int otherplayerMask;
        
        private int _currentBullets;
        private int _magazinesLeft;
        private float _fireRateTimer;

        private bool _wasShooting;
        
        protected override void SetUp()
        {
            _currentBullets = magazineSize;
            _magazinesLeft = magazineAmount;
        }

        protected override void Use(bool isUsing, uint latestReceivedServerGameStateTick)
        {
            // Todo: Check if we have bullets left 
            // Todo: Add reloading
            
            if (isUsing)
            {
                if ((hold && _fireRateTimer <= 0) || (!hold && !_wasShooting && _fireRateTimer <= 0))
                    InitiateShooting(latestReceivedServerGameStateTick);
            }
            
            _wasShooting = isUsing;
        }

        private void InitiateShooting(uint latestReceivedServerGameStateTick)
        {
            _fireRateTimer = fireRate;
            _currentBullets--;

            SetGameLayerRecursive(owner.gameObject, localPlayerMask);
            
            // Do Reconciliation
            bool shouldDoColliderRollback =
                Mathf.Abs(SnapshotManager.CurrentTick - latestReceivedServerGameStateTick) <=
                NetworkRunner.NetworkSettings.maxColliderRollbackOffset;
            
            if (!shouldDoColliderRollback)
                Debug.LogWarning("No Collider Rollback because of a too big offset!");
            
            #if Server
            GameState currentGameState = SnapshotManager.GetCurrentState(SnapshotManager.CurrentTick);;
            if (shouldDoColliderRollback) 
                SnapshotManager.ApplyGameState(latestReceivedServerGameStateTick);
            #endif
            
            // Initiate the shooting
            var result = Shoot();
            
            // Continue the game
            #if Server
            if (shouldDoColliderRollback) 
                SnapshotManager.ApplyGameState(currentGameState);
            #endif
            
            SetGameLayerRecursive(owner.gameObject, otherplayerMask);
            
            if (result.damageable != null) 
                result.damageable.TakeDamage(result.damage);
        }
        
        public virtual (IDamageable damageable, int damage) Shoot()
        {
            throw new System.NotImplementedException();
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

        public override ReconciliationMethod DoWeNeedToReconcile(IState predictedStateData, IState serverStateData)
        {
            GunState predictedState = (GunState) predictedStateData;
            GunState serverState = (GunState) serverStateData;
            
            if (predictedState.Equipped != serverState.Equipped)
            {
                Debug.LogWarning("Reconciliation Gun: Equipped");
                return ReconciliationMethod.World;
            }
            
            // These values can be reconciled using the single reconciliation method
            else if (predictedState.CurrentBullets != serverState.CurrentBullets)
            {
                Debug.LogWarning("Reconciliation Gun: CurrentBullets");
                return ReconciliationMethod.Single;
            }
            else if (predictedState.MagazinesLeft != serverState.MagazinesLeft)
            {
                Debug.LogWarning("Reconciliation Gun: MagazinesLeft");
                return ReconciliationMethod.Single;
            }
            else if (!Mathf.Approximately(predictedState.FireRateTimer, serverState.FireRateTimer))
            {
                Debug.LogWarning("Reconciliation Gun: FireRateTimer");
                return ReconciliationMethod.Single;
            }

            // If gun is not equipped, it is absolutely safe to use the single reconciliation method
            if (!serverState.Equipped)
            {
                if (Vector3.Distance(predictedState.Velocity, serverState.Velocity) >= 0.1f)
                {
                    Debug.LogWarning("Reconciliation Gun: Velocity");
                    return ReconciliationMethod.Single;
                }
                else if (Vector3.Distance(predictedState.AngularVelocity, serverState.AngularVelocity) >= 0.1f)
                {
                    Debug.LogWarning("Reconciliation Gun: Angular Velocity");
                    return ReconciliationMethod.Single;
                }
                else  if (Vector3.Distance(predictedState.Position, serverState.Position) >= 0.1f)
                {
                    Debug.LogWarning("Reconciliation Gun: Position");
                    return ReconciliationMethod.Single;
                }
                else if (Quaternion.Angle(Quaternion.Euler(predictedState.Rotation), Quaternion.Euler(serverState.Rotation)) >= 0.1f)
                {
                    Debug.LogWarning("Reconciliation Gun: Rotation");
                    return ReconciliationMethod.Single;
                }
            }

            return ReconciliationMethod.None;
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