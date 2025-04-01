using System;
using UnityEngine;

namespace ARSlingshotGame
{
    // Projectile behavior that replaces the original Ammo class
    public class Projectile : MonoBehaviour
    {
        [SerializeField] private float lifetimeSeconds = 3f;
        
        public Vector3 launchPosition { get; private set; }
        public bool isLaunched { get; private set; }
        public event Action OnHit;

        private float remainingLifetime;

        public void Launch(Vector3 startPosition)
        {
            isLaunched = true;
            launchPosition = startPosition;
            remainingLifetime = lifetimeSeconds;
        }

        private void Update()
        {
            if (!isLaunched) return;
            
            // Visual rotation effect
            transform.Rotate(360 * Time.deltaTime, 0, 0);
            
            // Handle lifetime
            remainingLifetime -= Time.deltaTime;
            if (remainingLifetime <= 0)
            {
                DestroyProjectile();
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!isLaunched) return;
            
            // Check if hit an enemy
            Enemy enemy = other.GetComponent<Enemy>();
            if (enemy != null)
            {
                enemy.TakeDamage(1, launchPosition);
            }
            
            DestroyProjectile();
        }

        private void DestroyProjectile()
        {
            OnHit?.Invoke();
            OnHit = null;
            Destroy(gameObject);
        }
    }
}