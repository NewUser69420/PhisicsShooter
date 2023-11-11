using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KillBox : MonoBehaviour
{
    [SerializeField] private List<Transform> spawnpoints = new();

    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log("Death");
        collision.transform.position = spawnpoints[Random.Range(0, spawnpoints.Count)].position;
    }
}
