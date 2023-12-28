using FishNet.Object;
using System.Collections;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using UnityEngine;

public class ForceGrenade_Field : NetworkBehaviour
{
    private void OnTriggerStay(Collider collider)
    {
        if (!base.IsServerStarted) return;
        
        if(collider.gameObject.layer == 7)
        {
            Rigidbody themTitties = collider.gameObject.GetComponent<Rigidbody>();

            themTitties.AddForce(transform.position - collider.transform.position * (Mathf.Max(0f, 100 - 5 * Vector3.Distance(collider.transform.position, transform.position))), ForceMode.Force);
        }
    }
}


