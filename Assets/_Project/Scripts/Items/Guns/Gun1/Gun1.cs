using System.Globalization;
using CSP.Object;
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
        
        public override void Shoot()
        {
            Debug.Log("Shooting with Gun1");

            if (Physics.Raycast(playerCamera.position, playerCamera.forward, out var hit, distance, hitMask))
            {
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
                // Miss
            }
        }
    }
}