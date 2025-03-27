using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

public class Target : MonoBehaviour
{
    public int ID;

    public void ReceiveDamage(int damage, Vector3 shootOrigin)
    {
        Destroy(this.gameObject); // Target is destroyed after damage
    }
}
