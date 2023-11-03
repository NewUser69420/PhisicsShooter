using FishNet;
using UnityEngine;

public class turnServerCamOn : MonoBehaviour
{
    void Start()
    {
        if(!InstanceFinder.IsServerStarted)
        {
            gameObject.SetActive(false);
        }
    }
}
