using FishNet;
using FishNet.Object;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class turnServerCamOn : MonoBehaviour
{
    void Start()
    {
        if(!InstanceFinder.IsServer)
        {
            gameObject.SetActive(false);
        }
    }
}
