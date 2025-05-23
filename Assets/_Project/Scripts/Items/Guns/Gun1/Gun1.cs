using System.Globalization;
using CSP;
using CSP.Object;
using CSP.Simulation;
using UnityEngine;

namespace _Project.Scripts.Items.Guns.Gun1
{
    public class Gun1 : Gun
    {
        [Header("Settings")]
        [SerializeField] private AnimationCurve damageFalloff;
        [SerializeField] private float damage;
        [SerializeField] private float distance;
        [SerializeField] private LayerMask hitMask;

        private Transform GetTotalParent(Transform starter)
        {
            Transform runner = starter;
            while (runner.parent != null)
                runner = runner.parent;

            return runner;
        }
        
        public override void Shoot(uint latestReceivedServerGameStateTick)
        {
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
            
            Debug.Log("Shooting with Gun1");

            if (Physics.Raycast(playerCamera.position, playerCamera.forward, out var hit, distance, hitMask))
            {
                #if Server
                if (shouldDoColliderRollback) 
                    SnapshotManager.ApplyGameState(currentGameState);
                #endif

                // Hit
                Transform hitParent = GetTotalParent(hit.transform);
                if (hitParent.TryGetComponent(out IDamageable damageable))
                {
                    int totalDamage = Mathf.RoundToInt((damage + (damageFalloff.Evaluate(hit.distance))));
                    damageable.TakeDamage(totalDamage);
                }
                else
                {
                    // Todo: Add hit mark
                }
            }
            else
            {
                #if Server
                // Miss
                if (shouldDoColliderRollback) 
                    SnapshotManager.ApplyGameState(currentGameState);
                #endif
            }
        }
    }
}