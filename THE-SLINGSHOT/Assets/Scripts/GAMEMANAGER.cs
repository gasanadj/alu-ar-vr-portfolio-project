using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.SceneManagement;
using UnityEngine.XR.ARSubsystems;

public class GAMEMANAGER : MonoBehaviour
{
    public GameObject planeSearchingCanvas;
    public GameObject targetPrefab;
    public int targetsNum = 5;
    public GameObject selectPlaneCanvas;
    public GameObject startButton;

    ARPlane selectedPlane = null;
    ARRaycastManager raycastManager;
    ARPlaneManager planeManager;

    List<ARRaycastHit> hits = new List<ARRaycastHit>();
    Dictionary<int, GameObject> targets = new Dictionary<int, GameObject>();

    // Ammo-related variables
    public GameObject ammoPrefab;
    private GameObject currentAmmo;
    private Vector3 ammoStartPos;
    private bool isDragging = false;
    private Vector3 dragStartPos;

    void Start()
    {
        raycastManager = FindObjectOfType<ARRaycastManager>();
        planeManager = FindObjectOfType<ARPlaneManager>();
    }

    public void Update()
    {
        if (Input.touchCount > 0 && selectedPlane == null && planeManager.trackables.count > 0)
        {
            SelectPlane();
        }

        if (currentAmmo != null)
        {
            HandleAmmoTouch();
        }
    }

    void SelectPlane()
    {
        Touch touch = Input.GetTouch(0);

        if (touch.phase == TouchPhase.Began)
        {
            if (raycastManager.Raycast(touch.position, hits, TrackableType.PlaneWithinPolygon))
            {
                ARRaycastHit hit = hits[0];
                selectedPlane = planeManager.GetPlane(hit.trackableId);

                // Hide other planes
                foreach (ARPlane plane in planeManager.trackables)
                {
                    if (plane != selectedPlane)
                    {
                        plane.gameObject.SetActive(false);
                    }
                }

                planeManager.enabled = false;
                selectPlaneCanvas.SetActive(false);

                PlaneSelected(selectedPlane);
            }
        }
    }

    void PlaneSelected(ARPlane plane)
    {
        // Clear any existing targets
        foreach (KeyValuePair<int, GameObject> target in targets)
        {
            Destroy(target.Value);
        }
        targets.Clear();

        startButton.SetActive(true);

        // Spawn targets
        for (int i = 1; i <= targetsNum; i++)
        {
            GameObject target = Instantiate(targetPrefab, plane.center, plane.transform.rotation, plane.transform);
            target.GetComponent<MoveRandomly>().StartMoving(plane);
            targets.Add(i, target);
        }

        // Spawn ammo after selecting the plane
        SpawnAmmo();
    }

    public void SpawnAmmo()
    {
        if (currentAmmo != null)
        {
            Destroy(currentAmmo);
        }

        Vector3 screenCenter = new Vector3(Screen.width / 2, Screen.height / 2, 0);
        if (raycastManager.Raycast(screenCenter, hits, TrackableType.PlaneWithinPolygon))
        {
            ARRaycastHit hit = hits[0];
            ammoStartPos = hit.pose.position;
            currentAmmo = Instantiate(ammoPrefab, ammoStartPos, Quaternion.identity);
        }
    }

    private void HandleAmmoTouch()
    {
        if (Input.touchCount == 0) return;

        Touch touch = Input.GetTouch(0);

        if (touch.phase == TouchPhase.Began)
        {
            isDragging = true;
            dragStartPos = touch.position;
        }
        else if (touch.phase == TouchPhase.Moved && isDragging)
        {

        }
        else if (touch.phase == TouchPhase.Ended && isDragging)
        {
            Vector3 dragEndPos = touch.position;
            Vector3 dragDirection = (dragStartPos - dragEndPos).normalized;
            float dragDistance = Vector3.Distance(dragStartPos, dragEndPos);
            float launchForce = dragDistance * 0.05f;

            Rigidbody rb = currentAmmo.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.isKinematic = false;
                rb.AddForce(dragDirection * launchForce, ForceMode.Impulse);
            }

            isDragging = false;
        }
    }
}
