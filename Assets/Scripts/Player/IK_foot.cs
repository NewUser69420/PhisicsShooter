using System;
using UnityEngine;
using static PlayerState;

public class IK_foot : MonoBehaviour
{
    [SerializeField] private Rigidbody rb;
    [SerializeField] private Transform leg;
    [SerializeField] private IK_foot otherLeg;
    [SerializeField] private Transform footPotitioner;
    [SerializeField] private Transform basePos;
    [SerializeField] private float lookForGroundDistance;
    [SerializeField] private LayerMask whatIsStepable;
    [SerializeField] private float stepDistanceNormal;
    [SerializeField] private float stepHight;
    [SerializeField] private float stepSpeedNormal;
    [SerializeField] private float stepSpeedMod;

    [System.NonSerialized] public bool canAnimate;

    private PlayerControlls playerControlls;
    private PlayerState playerState;

    private Vector3 oldPosition;
    private Vector3 newPosition;
    private float stepDistance;
    private float stepSpeed;

    private Vector3 newFootPos;
    private Vector3 oldFootPos;
    private Vector3 curFootPos;

    private float lerp = 1f;
    private float lerpp = 0f;

    private bool inAir;

    private void Start()
    {
        stepDistance = stepDistanceNormal;
        oldPosition = leg.position;
        oldFootPos = footPotitioner.position;

        playerControlls = new PlayerControlls();
        playerControlls.Enable();

        playerState = GetComponentInParent<PlayerState>();

        switch(name)
        {
            case "Right Leg IK":
                canAnimate = true;
                break;
            case "Left Leg IK":
                canAnimate = false;
                break;
            default:
                Debug.Log($"cant find IK obj");
                break;
        }
    }

    private void Update()
    {
        newPosition = leg.position;
        Vector3 direction = (newPosition - oldPosition) * stepDistance * 10;
        direction.y = 0f;
        Debug.DrawRay(leg.position, direction, Color.red);
        Vector3 directionPoint = direction + leg.position;

        stepSpeed = stepSpeedNormal + rb.velocity.magnitude * stepSpeedMod;

        RaycastHit hit;
        if (Physics.Raycast(directionPoint, Vector3.down, out hit, lookForGroundDistance, whatIsStepable))
        {
            Debug.DrawRay(directionPoint, Vector3.down, Color.red);
            if (Vector3.Distance(hit.point, curFootPos) > 0.25f && lerp == 1f && canAnimate)
            {
                newFootPos = new Vector3(hit.point.x, hit.point.y + 0.1f, hit.point.z);
                lerp = 0f;
                lerpp = 0f;
            }
        }
        else if(playerState.gState == GroundedState.InAir)
        {
            curFootPos = Vector3.Lerp(newFootPos, basePos.position, lerpp);
            lerpp += Time.deltaTime * 3;
        }

        if(lerp < 1)
        {
            curFootPos = Vector3.Lerp(oldFootPos, newFootPos, lerp);
            curFootPos.y += MathF.Sin(lerp * MathF.PI) * stepHight;

            lerp += Time.deltaTime * stepSpeed;
        }
        else
        {
            oldFootPos = newFootPos;
            lerp = 1f;

            if(canAnimate)
            {
                canAnimate = false;
                otherLeg.canAnimate = true;
            }
        }

        oldPosition = newPosition;

        footPotitioner.position = curFootPos;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawSphere(newFootPos, 0.1f);
    }
}
