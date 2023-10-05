using FishNet;
using FishNet.Object;
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
        foreach (GameObject player in GameObject.FindGameObjectsWithTag("Player"))
        {
            if (player.GetComponent<NetworkObject>().Owner.IsLocalClient) Cam = player.GetComponentInChildren<Camera>();
        }
    }

    private void Update()
    {
        if (InstanceFinder.IsServer) return;
        transform.rotation = Quaternion.LookRotation(transform.position - Cam.transform.position);
    }
}
