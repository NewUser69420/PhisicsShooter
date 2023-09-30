using FishNet;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class NameRotator : MonoBehaviour
{
    private Camera Cam;

    private void Start()
    {
        if (InstanceFinder.IsServer) return;
        Cam = FindObjectOfType<Camera>();
    }

    private void Update()
    {
        if (InstanceFinder.IsServer) return;
        transform.rotation = Quaternion.LookRotation(transform.position - Cam.transform.position);
    }
}
