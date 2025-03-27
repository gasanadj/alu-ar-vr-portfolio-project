using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

public class MoveRandomly : MonoBehaviour
{
    float rayYoffset;
    Vector3 planeCenter;
    float range;
    float speed = 0.01f;
    bool hasDestination;
    Vector3 destination;
    bool startMoving = false;
    ARPlane movePlane;
    float colliderHeight;
    Quaternion destinationRotation;

    void Start()
    {

    }

    void Update()
    {
        if (!startMoving) return;

        if (hasDestination)
        {
            transform.rotation = Quaternion.Lerp(transform.rotation, destinationRotation, speed * 20);
            transform.position = Vector3.MoveTowards(transform.position, destination, speed);
            if (Vector3.Distance(transform.position, destination) < 0.01f)
            {
                hasDestination = false;
            }
        }
        else
        {
            hasDestination = RandomPoint(planeCenter, rayYoffset, range, out destination);
            destinationRotation = Quaternion.LookRotation(destination - transform.position, Vector3.up);
        }
    }

    public void StartMoving(ARPlane plane)
    {
        movePlane = plane;
        planeCenter = plane.center;
        range = Mathf.Max(plane.size.x, plane.size.y);
        rayYoffset = 0.5f;
        colliderHeight = transform.localScale.y * GetComponent<CapsuleCollider>().height;
        transform.position = planeCenter + Vector3.up * colliderHeight / 2;
        hasDestination = RandomPoint(planeCenter, rayYoffset, range, out destination);
        destinationRotation = Quaternion.LookRotation(destination - transform.position, Vector3.up);
        startMoving = true;
    }

    public void StopMoving()
    {
        startMoving = false;
    }

    public bool RandomPoint(Vector3 center, float rayYoffset, float range, out Vector3 result)
    {
        Vector3 next = center + Random.insideUnitSphere * range;
        RaycastHit hit;
        if (Physics.Raycast(next, Vector3.down, out hit, Mathf.Infinity))
        {
            if (hit.collider.gameObject == movePlane.gameObject)
            {
                result = hit.point + Vector3.up * colliderHeight / 2;
                return true;
            }
        }
        result = Vector3.zero;
        return false;
    }
}
