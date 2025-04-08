using System.Collections;
using UnityEngine;

namespace ARSlingshotGame
{
    public class ProjectileLauncher : MonoBehaviour
    {
        [SerializeField] private GameObject projectilePrefab;
        [SerializeField] private int forceMultiplier = 750;
        [SerializeField] private int forceScalar = 3;
        [SerializeField] private float reloadDelay = 1.5f;

        private GameObject loadedProjectile;
        private Projectile projectileComponent;
        private int ammoRemaining;
        private bool isDragging;
        private float dragDepth;
        private Vector3 dragOffset;

        // Initialize launcher with ammo
        public void InitializeAmmo(int amount)
        {
            ammoRemaining = amount;
            LoadProjectile();
        }

        // Reset launcher state
        public void Reset()
        {
            if (loadedProjectile)
            {
                Destroy(loadedProjectile);
                loadedProjectile = null;
                projectileComponent = null;
            }
            ammoRemaining = 0;
            isDragging = false;
        }

        private void LoadProjectile()
        {
            if (loadedProjectile == null && transform.childCount == 0 && ammoRemaining > 0)
            {
                // Create new projectile
                loadedProjectile = Instantiate(projectilePrefab, transform.position, transform.rotation, transform);

                // Setup physics
                var rb = loadedProjectile.GetComponent<Rigidbody>();
                rb.isKinematic = true;

                // Get projectile component reference
                projectileComponent = loadedProjectile.GetComponent<Projectile>();

                // Register for hit events
                projectileComponent.OnHit += HandleProjectileHit;

                // Update ammo count
                ammoRemaining--;
                GameEvents.AmmoUpdated(ammoRemaining);
            }
        }

        private void HandleProjectileHit()
        {
            StartCoroutine(DelayedReload(reloadDelay));
        }

        private IEnumerator DelayedReload(float delay)
        {
            yield return new WaitForSeconds(delay);
            LoadProjectile();
        }

        private void Update()
        {
            if (loadedProjectile == null) return;

            // Handle mouse/touch input
            if (Input.GetMouseButtonDown(0) && !isDragging)
            {
                TryGrabProjectile();
            }

            if (isDragging)
            {
                // Update projectile position based on drag
                loadedProjectile.transform.position = GetDragPoint() + dragOffset;

                // Aim projectile in launch direction
                loadedProjectile.transform.forward = GetLaunchDirection().normalized;

                // Update trajectory visualization
                UpdateTrajectory();
            }

            if (Input.GetMouseButtonUp(0) && isDragging)
            {
                LaunchProjectile();
                isDragging = false;
            }
        }

        private void TryGrabProjectile()
        {
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out hit))
            {
                if (hit.transform.gameObject == loadedProjectile)
                {
                    // Calculate drag parameters
                    dragDepth = Camera.main.WorldToScreenPoint(loadedProjectile.transform.position).z;
                    dragOffset = loadedProjectile.transform.position - GetDragPoint();
                    isDragging = true;
                }
            }
        }

        private Vector3 GetDragPoint()
        {
            // Convert screen position to world position
            Vector3 screenPoint = Input.mousePosition;
            screenPoint.z = dragDepth;
            return Camera.main.ScreenToWorldPoint(screenPoint);
        }

        private Vector3 GetLaunchDirection()
        {
            // Calculate launch force based on pull distance
            Vector3 force = (transform.position - loadedProjectile.transform.position) * forceMultiplier;

            // Add forward component for trajectory
            force = ((transform.forward * force.magnitude * forceScalar) + force);
            return force;
        }

        // Update the trajectory line
        private void UpdateTrajectory()
        {
            if (projectileComponent != null)
            {
                // Calculate launch velocity based on the force
                Vector3 launchVelocity = GetLaunchDirection() / loadedProjectile.GetComponent<Rigidbody>().mass;

                // Update the trajectory visualization
                projectileComponent.ShowTrajectory(launchVelocity, loadedProjectile.transform.position);
            }
        }

        private void LaunchProjectile()
        {
            if (loadedProjectile != null)
            {
                // Detach from parent
                loadedProjectile.transform.parent = null;

                // Enable physics and apply launch force
                Rigidbody rb = loadedProjectile.GetComponent<Rigidbody>();
                rb.isKinematic = false;
                rb.AddForce(GetLaunchDirection());

                // Initialize projectile state
                projectileComponent.Launch(loadedProjectile.transform.position);

                loadedProjectile = null;
                projectileComponent = null;
            }
        }
    }
}