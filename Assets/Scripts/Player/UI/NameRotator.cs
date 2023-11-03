using FishNet;
using FishNet.Object;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class NameRotator : NetworkBehaviour
{
    private Transform Cam;

    public override void OnStartNetwork()
    {   
        if (base.IsClientInitialized)
        {
            foreach (var obj in LocalConnection.Objects)
            {
                if (obj.CompareTag("Player")) Cam = obj.transform.Find("Cam");
            }
        }
    }

    private void Update()
    {
        if (base.IsClientInitialized && Cam != null) transform.rotation = Quaternion.LookRotation(transform.position - Cam.transform.position);
    }
}
