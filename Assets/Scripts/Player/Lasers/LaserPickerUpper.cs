using FishNet;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static LaserPickerUpper;

public class LaserPickerUpper : MonoBehaviour
{
    public enum LaserType
    {
        DefaultLasers,
        BouncyLasers
    }
    public LaserType laserType;

    private bool isEnabled = true;

    private void OnTriggerEnter(Collider other)
    {
        if (!isEnabled) return;
        Debug.Log($"Picking up {laserType}");

        Transform Parent = other.transform.root;

        var laserTypes = Enum.GetValues(typeof(LaserType));
        foreach(var type in laserTypes)
        {
            Destroy(Parent.GetComponent(type.ToString()));
        }

        Parent.gameObject.AddComponent(System.Type.GetType(laserType.ToString() + ",Assembly-CSharp"));

        isEnabled = false;
        transform.Find("Sphere").gameObject.SetActive(false);
        Invoke(nameof(TurnOn), 2f);
    }

    private void TurnOn()
    {
        isEnabled = true;
        transform.Find("Sphere").gameObject.SetActive(true);
    }
}
