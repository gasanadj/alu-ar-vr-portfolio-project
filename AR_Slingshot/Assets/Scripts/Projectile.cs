using System;
using UnityEngine;

namespace ARSlingshotGame
{
    public class Projectile : MonoBehaviour
    {
        [SerializeField] private float lifetimeSeconds = 3f;
        [SerializeField] private LineRenderer trajectoryLine; // Reference to the LineRenderer component

        public Vector3 launchPosition { get; private set; }
        public bool isLaunched { get; private set; }
        public event Action OnHit;

        private float remainingLifetime;

        private void Awake()
        {
            // If LineRenderer doesn't exist, add it
            if (trajectoryLine == null)
            {
                trajectoryLine = gameObject.AddComponent<LineRenderer>();
                SetupLineRenderer();
            }

            // Initially hide the trajectory line
            trajectoryLine.enabled = false;
        }

        private void SetupLineRenderer()
        {
            // Configure the LineRenderer appearance
            trajectoryLine.startWidth = 0.05f;
            trajectoryLine.endWidth = 0.01f;
            trajectoryLine.positionCount = 0;

            // Set material and color
            trajectoryLine.material = new Material(Shader.Find("Sprites/Default"));
            trajectoryLine.startColor = Color.red;
            trajectoryLine.endColor = new Color(1, 0, 0, 0.5f); // Faded red at the end
        }

        public void Launch(Vector3 startPosition)
        {
            isLaunched = true;
            launchPosition = startPosition;
            remainingLifetime = lifetimeSeconds;

            // Hide trajectory line on launch
            trajectoryLine.enabled = false;
        }

        // Display the trajectory prediction
        public void ShowTrajectory(Vector3 velocity, Vector3 startPos)
        {
            // Show the line renderer
            trajectoryLine.enabled = true;

            // Calculate trajectory points
            Vector3[] points = CalculateTrajectoryPoints(startPos, velocity, 2.0f);

            // Update line renderer
            trajectoryLine.positionCount = points.Length;
            trajectoryLine.SetPositions(points);
        }

        // Hide trajectory line
        public void HideTrajectory()
        {
            trajectoryLine.enabled = false;
        }

        // Calculate arc trajectory points
        private Vector3[] CalculateTrajectoryPoints(Vector3 startPos, Vector3 velocity, float timeStep)
        {
            int maxSteps = 20; // Number of points in the trajectory line
            Vector3[] points = new Vector3[maxSteps];

            for (int i = 0; i < maxSteps; i++)
            {
                float time = i * timeStep / maxSteps;

                // Physics formula for position with time: pos = startPos + velocity*t + 0.5*gravity*t^2
                Vector3 point = startPos + velocity * time + 0.5f * Physics.gravity * time * time;
                points[i] = point;
            }

            return points;
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