using UnityEngine;

namespace ARSlingshotGame
{
    // Enemy behavior that replaces the original Enemy class
    public class Enemy : MonoBehaviour
    {
        [SerializeField] private int basePoints = 10;
        [SerializeField] private int health = 1;
        
        public int ID { get; set; }

        public void TakeDamage(int damage, Vector3 damageSource)
        {
            health -= damage;
            if (health <= 0)
            {
                // Calculate bonus points based on distance
                int totalPoints = CalculatePoints(damageSource);
                
                // Trigger destruction event
                GameEvents.TargetDestroyed(ID, totalPoints);
                
                Destroy(gameObject);
            }
        }

        private int CalculatePoints(Vector3 shotOrigin)
        {
            // More points for longer shots
            return basePoints + (int)Vector3.Distance(transform.position, shotOrigin);
        }
    }
}