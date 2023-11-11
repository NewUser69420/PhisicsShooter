using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AntiGrav : MonoBehaviour
{
    [SerializeField] private float strenght;
    [SerializeField] private Transform Sorce;

    private void OnTriggerStay(Collider collider)
    {
        if(collider.GetComponent<Rigidbody>() != null) collider.gameObject.GetComponent<Rigidbody>().AddForce(Physics.gravity * -80f * (Vector3.Distance(collider.transform.position, Sorce.position) * strenght));
    }
}
