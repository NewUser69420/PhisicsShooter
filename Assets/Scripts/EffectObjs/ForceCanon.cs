using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ForceCanon : MonoBehaviour
{
    [SerializeField] private float strenght;

    private void OnTriggerStay(Collider collider)
    {
        if (collider.GetComponent<Rigidbody>() != null) collider.GetComponent<Rigidbody>().AddForce(transform.up * strenght * 300);
    }
}
