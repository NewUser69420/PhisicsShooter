using System.Collections;
using System.Collections.Generic;
using System.Runtime;
using UnityEngine;

public class playerCamMover : MonoBehaviour
{
    [SerializeField] private Transform Player;
    [SerializeField] private float speed;
    private Vector3 Velocity = Vector3.zero;


    private void Update()
    {
        Vector3 targetPos = new Vector3(Player.position.x, Player.position.y + 1.65f, Player.position.z);
        transform.position = Vector3.SmoothDamp(transform.position, targetPos, ref Velocity, speed * Time.deltaTime);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawSphere(transform.position, 0.2f);
    }
}
