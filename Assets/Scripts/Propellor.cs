using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Propellor : MonoBehaviour
{
    [SerializeField] private int speed;

    private void FixedUpdate()
    {
        transform.Rotate(speed * Time.deltaTime * Vector3.forward);
    }
}
