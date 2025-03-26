using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.XR.ARFoundation;

public class dimensions : MonoBehaviour
{
    // Start is called before the first frame update
    private ARPlane plane;
    private TextMeshPro textMesh;

    void Start()
    {
        plane = GetComponent<ARPlane>();
        textMesh = GetComponentInChildren<TextMeshPro>();
    }

    // Update is called once per frame
    void Update()
    {
        if (plane !=null && textMesh !=null)
        {
            float width =plane.size.x;
            float height =plane.size.y;

            textMesh.text = $"Width: {width:F2}m\nHeight:{height:F2}m";

            textMesh.transform.position = new Vector3(plane.transform.position.x, plane.transform.position.y+ 0.1f, plane.transform.position.z);
        }
    }
}
