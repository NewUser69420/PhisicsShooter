using FishNet;
using FishNet.Object;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KillBox : MonoBehaviour
{
    [SerializeField] private List<Transform> spawnpoints = new();
    [SerializeField] private bool isOffline;

    private void OnCollisionEnter(Collision collision)
    {
        if(isOffline) collision.transform.position = spawnpoints[Random.Range(0, spawnpoints.Count)].position;
        else if (InstanceFinder.IsServerStarted)
        {
            collision.gameObject.GetComponent<ServerHealthManager>().DoDeath(collision.gameObject.GetComponent<NetworkObject>().Owner, collision.gameObject, collision.gameObject.GetComponent<NetworkObject>().Owner);
        }
    }
}
