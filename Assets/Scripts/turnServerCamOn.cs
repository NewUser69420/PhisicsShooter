using FishNet;
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
