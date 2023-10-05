using FishNet.Demo.AdditiveScenes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FPArmAnimator : MonoBehaviour
{
    [SerializeField] private Transform HandPositioner;
    [SerializeField] private Transform FPGun;
    [SerializeField] private Transform Player;
    [SerializeField] private LayerMask whatIsWall;

    private Vector3 velocity;
    private Vector3 velocityy;

    void Update()
    {
        switch (gameObject.name)
        {
            case "FP Right Arm IK":
                RaycastHit hit;
                if (Physics.Raycast(Player.position, Player.right, out hit, 0.7f, whatIsWall))
                {
                    HandPositioner.position = Vector3.SmoothDamp(HandPositioner.position, hit.point, ref velocity, 25 * Time.deltaTime);
                }
                else
                {
                    HandPositioner.position = Vector3.SmoothDamp(HandPositioner.position, FPGun.position, ref velocityy, 15 * Time.deltaTime);
                }
                break;
            case "FP Left Arm IK":
                RaycastHit hitt;
                if (Physics.Raycast(Player.position, -Player.right, out hitt, 0.7f, whatIsWall))
                {
                    HandPositioner.position = Vector3.SmoothDamp(HandPositioner.position, hitt.point, ref velocity, 25 * Time.deltaTime);
                }
                else
                {
                    HandPositioner.position = Vector3.SmoothDamp(HandPositioner.position, FPGun.position, ref velocityy, 15 * Time.deltaTime);
                }
                break;
        }
    }
}
