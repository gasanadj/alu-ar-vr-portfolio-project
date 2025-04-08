using UnityEngine;
using UnityEngine.XR.ARFoundation;

namespace ARSlingshotGame
{
    // Enemy movement behavior that replaces the original RandomMotion class
    public class EnemyMovement : MonoBehaviour
    {
        [SerializeField] private float moveSpeed = 0.005f;
        
        private ARPlane movementPlane;
        private Vector3 planeCenter;
        private float movementRange;
        private float colliderHeight;
        private Vector3 currentDestination;
        private Quaternion targetRotation;
        private bool hasDestination;
        private bool isMoving;
        private float raycastOffset = 0.5f;

        public void Initialize(ARPlane plane)
        {
            // Setup movement parameters
            movementPlane = plane;
            planeCenter = plane.center;
            movementRange = Mathf.Max(plane.size.x, plane.size.y);
            
            // Calculate collider height for proper positioning
            colliderHeight = transform.localScale.y * GetComponent<CapsuleCollider>().height;
            
            // Position enemy on the plane
            transform.position = planeCenter + Vector3.up * (colliderHeight / 2);
            
            // Get initial destination
            hasDestination = GetRandomDestination(out currentDestination);
            if (hasDestination)
            {
                targetRotation = Quaternion.LookRotation(currentDestination - transform.position, Vector3.up);
            }
            
            isMoving = true;
        }

        private void Update()
        {
            if (!isMoving) return;
            
            if (hasDestination)
            {
                // Rotate towards destination
                transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, moveSpeed * 9);
                
                // Move towards destination
                transform.position = Vector3.MoveTowards(transform.position, currentDestination, moveSpeed);
                
                // Check if destination reached
                if (Vector3.Distance(transform.position, currentDestination) < 0.01f)
                {
                    hasDestination = false;
                }
            }
            else
            {
                // Find new destination
                hasDestination = GetRandomDestination(out currentDestination);
                if (hasDestination)
                {
                    targetRotation = Quaternion.LookRotation(currentDestination - transform.position, Vector3.up);
                }
            }
        }

        private bool GetRandomDestination(out Vector3 destination)
        {
            // Generate random point on plane
            Vector3 randomPoint = planeCenter + Random.insideUnitSphere * movementRange;
            
            // Raycast to find point on plane
            RaycastHit hit;
            if (Physics.Raycast(randomPoint + Vector3.up * raycastOffset, Vector3.down, out hit))
            {
                if (hit.collider.gameObject == movementPlane.gameObject)
                {
                    // Position at hit point plus half collider height
                    destination = hit.point + Vector3.up * (colliderHeight / 2);
                    return true;
                }
            }
            
            destination = Vector3.zero;
            return false;
        }
    }
}