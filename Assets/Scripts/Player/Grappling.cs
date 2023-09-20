using Cinemachine.Editor;
using FishNet.Object;
using System.Collections;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using static PlayerState;

public class Grappling : NetworkBehaviour
{
    private PredictedPlayerController moveScript;
    private PlayerState playerState;
    private GameObject LightObject;
    private Vector3 grapplePoint;
    private float grapplingCdTimer;
    private PlayerControlls playerControlls;
    
    [SerializeField] private Transform Cam;
    [SerializeField] private LayerMask WhatIsGrappable;
    [SerializeField] private GameObject LightObjectPrefab;
    [SerializeField] private float maxGrappeDistance;
    [SerializeField] private float grappleDelayTime;
    [SerializeField] private float overshootYAxis;
    [SerializeField] private float grapplingCd;
    [System.NonSerialized] public bool grappling;
    
    public void Awake()
    {
        moveScript = GetComponent<PredictedPlayerController>();
        playerState = GetComponent<PlayerState>();

        playerControlls = new PlayerControlls();
        playerControlls.Enable();
    }

    private void Update()
    {
        if (IsServer)
        {
            if (grapplingCdTimer > 0)
            {
                grapplingCdTimer -= Time.time;
            }
        }
        
        if(grapplingCdTimer > 0)
        {
            grapplingCdTimer -= Time.time;
        }

        if (playerControlls.OnFoot.Grapple.WasPressedThisFrame())
        {
            if (!grappling)
            {
                StartGrapple();
            }
            else if (grappling)
            {
                StopGrapple();
            }
        }
    }

    public void StartGrapple()
    {
        if (grapplingCdTimer > 0) return;
        
        grappling = true;

        RaycastHit hit;
        if(Physics.Raycast(Cam.position, Cam.forward, out hit, maxGrappeDistance, WhatIsGrappable)) 
        {
            grapplePoint = hit.point + (transform.position - hit.point).normalized;
            Invoke(nameof(ExecuteGrapple), grappleDelayTime);
        }
        else
        {
            grapplePoint = Cam.position + Cam.forward * maxGrappeDistance;
            Invoke(nameof(StopGrapple), grappleDelayTime);
        }

        LightObject = Instantiate(LightObjectPrefab, grapplePoint, Quaternion.identity);
    }

    private void ExecuteGrapple()
    {
        Vector3 lowestPoint = new Vector3(transform.position.x, transform.position.y - 1f, transform.position.z);
        float grapplePointRelativeYPos = grapplePoint.y - lowestPoint.y;
        float highestPointOnArc = grapplePointRelativeYPos + overshootYAxis;

        if (grapplePointRelativeYPos < 0)
        {
            highestPointOnArc = overshootYAxis;
        }

        moveScript.jumpToPosition(grapplePoint, highestPointOnArc);

        //FindObjectOfType<AudioManger>().Play("Jetpack Loop");

        Invoke(nameof(StopGrapple), 1f);
    }

    void StopGrapple()
    {
        grappling = false;

        grapplingCdTimer = grapplingCd;

        playerState.aState = ActionState.Passive;

        //FindObjectOfType<AudioManger>().Stop("Jetpack Loop");

        Destroy(LightObject);
    }
}
