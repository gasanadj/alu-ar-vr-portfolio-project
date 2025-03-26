using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.SceneManagement;
using UnityEngine.XR.ARSubsystems;


public class GAMEMANAGER : MonoBehaviour
{
    // Start is called before the first frame update

    public GameObject planeSearchingCanvas;
    public GameObject targetPrefab;
    public int targetsNum = 5;
    public GameObject selectPlaneCanvas;
    public GameObject startButton;

    ARPlane selectedPlane = null;    
    ARRaycastManager raycastManager;
    ARPlaneManager planeManager;
    ARSession session;

    List<ARRaycastHit> hits = new List<ARRaycastHit>();
    Dictionary<int, GameObject> targets = new Dictionary<int, GameObject>();

    void Start()
    {
       raycastManager = FindObjectOfType<ARRaycastManager>();
       planeManager = FindObjectOfType<ARPlaneManager>();
 
    }

    // Update is called once per frame
    void Update()
    {
       if (Input.touchCount > 0 && selectedPlane == null && planeManager.trackables.count >0)
       {
        SelectPlane();
       } 
    }
    private void SelectPlane()
{
    Touch touch = Input.GetTouch(0);
    

    if (touch.phase == TouchPhase.Began)
    {
        if (raycastManager.Raycast(touch.position, hits, TrackableType.PlaneWithinPolygon))
        {
            ARRaycastHit hit = hits[0];
            selectedPlane =  planeManager.GetPlane(hit.trackableId);
            // selectedPlane.GetComponent<Renderer>().material.color = new Color(0, 0, 0, 0);
            selectedPlane.GetComponent<MeshRenderer>().enabled = false;
            selectedPlane.GetComponent<LineRenderer>().positionCount = 0;
            selectedPlane.gameObject.layer = LayerMask.NameToLayer("Default");
            // selectedPlane.GetComponent<Renderer>().sortingOrder = 0;
            foreach(ARPlane plane in planeManager.trackables)
            {
                if (plane != selectedPlane)
                {
                    plane.gameObject.SetActive(false);
                }
            }
            planeManager.enabled = false;
            selectPlaneCanvas.SetActive(false);
            // OnPlaneSelected?.Invoke(selectedPlane);
        }
    }
}
void PlanesFound(ARPlanesChangedEventArgs args)
{
    if (selectedPlane == null && planeManager.trackables.count > 0)
    {
        planeSearchingCanvas.SetActive(false);
        selectPlaneCanvas.SetActive(true);
        planeManager.planesChanged -= PlanesFound;
    }
}

void PlaneSelected(ARPlane plane)
{
    foreach (KeyValuePair<int, GameObject> target in targets)
    {
        Destroy(target.Value);
    }
    targets.Clear();

    startButton.SetActive(true);
    for (int i = 1; i <= targetsNum; i++)
    {
        GameObject target = Instantiate(targetPrefab, plane.center, plane.transform.rotation, plane.transform);
        // target.GetComponent<MoveRandomly>().StartMoving(plane);
        //target.GetComponent<Target>().ID = i;
       // target.GetComponent<Target>().OnTargetDestroy += UpdateGameWhenHitTarget;
        targets.Add(i, target);
    }
}


}
