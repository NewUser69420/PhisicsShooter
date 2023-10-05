using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FPGunPositioner : MonoBehaviour
{
    [SerializeField] private Transform Parent;
    
    [System.NonSerialized] public Transform aimer;

    private Vector3 velocity;

    void Update()
    {
        
        Parent.rotation = aimer.rotation;

        if(Vector3.Distance(Parent.position, aimer.position) > 0.2f)
        {
            Parent.position = Vector3.SmoothDamp(Parent.position, aimer.position, ref velocity, 10 * Time.deltaTime);
        }
        else
        {
            Parent.position = Vector3.SmoothDamp(Parent.position, aimer.position, ref velocity, 15 * Time.deltaTime);
        }
    }
}
