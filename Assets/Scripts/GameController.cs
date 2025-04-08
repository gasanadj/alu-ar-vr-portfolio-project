using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.SceneManagement;

namespace ARSlingshotGame
{
    // Main game controller that replaces the original GameManager class
    public class GameController : MonoBehaviour
    {
        [Header("Game Settings")]
        [SerializeField] private GameObject enemyPrefab;
        [SerializeField] private GameObject ammoImagePrefab;
        [SerializeField] private int enemyCount = 5;
        [SerializeField] private int ammoCount = 7;
        [SerializeField] private Material planeOcclusionMaterial;

        [Header("UI Elements")]
        [SerializeField] private GameObject findingUIPanel;
        [SerializeField] private GameObject pickingUIPanel;
        [SerializeField] private GameObject startButton;
        [SerializeField] private GameObject gameUIPanel;
        [SerializeField] private GameObject ammoUIContainer;
        [SerializeField] private GameObject replayButton;
        [SerializeField] private Text scoreText;
        [SerializeField] private GameObject restartButton;

        // Game state
        private int currentScore;
        private Dictionary<int, GameObject> activeEnemies = new Dictionary<int, GameObject>();
        
        // AR Components
        private ARSession arSession;
        private ARPlane selectedPlane;
        private ARRaycastManager raycastManager;
        private ARPlaneManager planeManager;
        private List<ARRaycastHit> raycastHits = new List<ARRaycastHit>();
        
        // Weapon reference
        private ProjectileLauncher launcher;

        private void Awake()
        {
            arSession = FindObjectOfType<ARSession>();
            arSession.Reset();
            
            raycastManager = FindObjectOfType<ARRaycastManager>();
            planeManager = FindObjectOfType<ARPlaneManager>();
            launcher = FindObjectOfType<ProjectileLauncher>();

            // Register for events
            GameEvents.OnTargetDestroyed += HandleTargetDestroyed;
            GameEvents.OnAmmoUpdated += UpdateAmmoUI;
        }

        private void OnDestroy()
        {
            // Unregister from events
            GameEvents.OnTargetDestroyed -= HandleTargetDestroyed;
            GameEvents.OnAmmoUpdated -= UpdateAmmoUI;
        }

        private void Start()
        {
            // Setup AR detection events
            planeManager.planesChanged += OnPlanesChanged;
            GameEvents.OnPlaneSelected += OnPlaneSelectedHandler;
            
            // Initialize UI
            findingUIPanel.SetActive(true);
            pickingUIPanel.SetActive(false);
            gameUIPanel.SetActive(false);
            startButton.SetActive(false);
            replayButton.SetActive(false);
        }

        private void Update()
        {
            // Plane selection logic
            if (Input.touchCount > 0 && selectedPlane == null && planeManager.trackables.count > 0)
            {
                AttemptPlaneSelection();
            }
        }

        private void OnPlanesChanged(ARPlanesChangedEventArgs args)
        {
            if (selectedPlane == null && planeManager.trackables.count > 0)
            {
                findingUIPanel.SetActive(false);
                pickingUIPanel.SetActive(true);
                planeManager.planesChanged -= OnPlanesChanged;
            }
        }

        private void AttemptPlaneSelection()
        {
            Touch touch = Input.GetTouch(0);
            if (touch.phase != TouchPhase.Began) return;

            if (raycastManager.Raycast(touch.position, raycastHits, TrackableType.PlaneWithinPolygon))
            {
                // Select the first hit plane
                ARRaycastHit hit = raycastHits[0];
                selectedPlane = planeManager.GetPlane(hit.trackableId);
                
                // Adjust plane visuals
                selectedPlane.GetComponent<LineRenderer>().positionCount = 0;
                selectedPlane.GetComponent<Renderer>().material = planeOcclusionMaterial;
                
                // Disable other planes
                foreach (ARPlane plane in planeManager.trackables)
                {
                    if (plane != selectedPlane)
                    {
                        plane.gameObject.SetActive(false);
                    }
                }
                
                // Disable plane manager to stop detecting new planes
                planeManager.enabled = false;
                pickingUIPanel.SetActive(false);
                
                // Trigger selection event
                GameEvents.PlaneSelected(selectedPlane);
            }
        }

        private void OnPlaneSelectedHandler(ARPlane plane)
        {
            // Clear any existing enemies
            foreach (var enemy in activeEnemies.Values)
            {
                Destroy(enemy);
            }
            activeEnemies.Clear();
            
            // Spawn new enemies
            SpawnEnemies(plane);
            startButton.SetActive(true);
        }

        private void SpawnEnemies(ARPlane plane)
        {
            for (int i = 1; i <= enemyCount; i++)
            {
                GameObject enemy = Instantiate(enemyPrefab, plane.center, plane.transform.rotation, plane.transform);
                enemy.GetComponent<EnemyMovement>().Initialize(plane);
                
                // Set unique ID on enemy
                var enemyComponent = enemy.GetComponent<Enemy>();
                enemyComponent.ID = i;
                
                activeEnemies.Add(i, enemy);
            }
        }

        private void HandleTargetDestroyed(int id, int points)
        {
            // Remove enemy from tracking and update score
            activeEnemies.Remove(id);
            currentScore += points;
            scoreText.text = currentScore.ToString();
            
            // Check if all enemies are destroyed
            if (activeEnemies.Count == 0)
            {
                ShowGameEnd();
            }
        }

        private void UpdateAmmoUI(int ammoLeft)
        {
            // Remove ammo icon when ammo is used
            if (ammoUIContainer.transform.childCount > 0 && ammoLeft >= 0)
            {
                Destroy(ammoUIContainer.transform.GetChild(0).gameObject);
            }
            
            // If out of ammo, end the game
            if (ammoLeft == 0)
            {
                ShowGameEnd();
            }
        }

        public void StartGame()
        {
            // Initialize ammo and UI
            launcher.InitializeAmmo(ammoCount);
            currentScore = 0;
            scoreText.text = "0";
            
            // Update UI state
            startButton.SetActive(false);
            gameUIPanel.SetActive(true);
            
            // Create ammo UI indicators
            for (int i = 0; i < ammoCount; i++)
            {
                GameObject ammo = Instantiate(ammoImagePrefab);
                ammo.transform.SetParent(ammoUIContainer.transform, false);
            }
        }

        private void ShowGameEnd()
        {
            // Clear ammo UI
            foreach (Transform child in ammoUIContainer.transform)
            {
                Destroy(child.gameObject);
            }
            
            // Reset launcher and show replay button
            launcher.Reset();
            replayButton.SetActive(true);
        }
        
        public void RestartGame()
        {
            ResetGame(); // Call the ResetGame method instead
        }

        public void PlayAgain()
        {
            // Hide end game buttons
            replayButton.SetActive(false);
            if (restartButton != null)
            {
                restartButton.SetActive(false);
            }

            // Clear any remaining enemies
            foreach (var enemy in activeEnemies.Values)
            {
                Destroy(enemy);
            }
            activeEnemies.Clear();

            // Respawn enemies on the same plane
            SpawnEnemies(selectedPlane);

            // Reset score
            currentScore = 0;
            scoreText.text = "0";

            // Reset and refill ammo
            launcher.InitializeAmmo(ammoCount);

            // Recreate ammo UI
            foreach (Transform child in ammoUIContainer.transform)
            {
                Destroy(child.gameObject);
            }

            for (int i = 0; i < ammoCount; i++)
            {
                GameObject ammo = Instantiate(ammoImagePrefab);
                ammo.transform.SetParent(ammoUIContainer.transform, false);
            }

            // Make sure game UI is active
            gameUIPanel.SetActive(true);
        }

        public void ExitGame()
        {
            Application.Quit();
        }
        
        public void ResetGame()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }
}