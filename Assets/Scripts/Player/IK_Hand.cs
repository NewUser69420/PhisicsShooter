using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IK_Hand : MonoBehaviour
{
    [SerializeField] private Transform GrabPoint;
    [SerializeField] private Transform HandPositioner;

    void Update()
    {
        HandPositioner.position = GrabPoint.position;
    }
}
