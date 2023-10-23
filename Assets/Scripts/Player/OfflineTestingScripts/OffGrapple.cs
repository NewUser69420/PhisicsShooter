using UnityEngine;
using static PlayerState;

public class OffGrappling : MonoBehaviour
{
    private OfflinePlayerMovement moveScript;
    private PlayerState playerState;
    private GameObject LightObject;
    private Vector3 grapplePoint;
    private PlayerControlls playerControlls;

    public float grapplingValueMax;

    [SerializeField] private Transform Cam;
    [SerializeField] private LayerMask WhatIsGrappable;
    [SerializeField] private GameObject LightObjectPrefab;
    [SerializeField] private float maxGrappeDistance;
    [SerializeField] private float grappleDelayTime;
    [SerializeField] private float overshootYAxis;
    [SerializeField] private float grapplingCdMod;
    [SerializeField] private float grapplingCost;

    [System.NonSerialized] public bool grappling;
    [System.NonSerialized] public float grapplingValue;

    public void Awake()
    {
        moveScript = GetComponent<OfflinePlayerMovement>();
        playerState = GetComponent<PlayerState>();

        playerControlls = new PlayerControlls();
        playerControlls.Enable();

        grapplingValue = grapplingValueMax;
    }

    private void Update()
    {
        if (grapplingValue < grapplingValueMax)
        {
            grapplingValue += Time.deltaTime * grapplingCdMod;
        }
        else
        {
            grapplingValue = grapplingValueMax;
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
        if (grapplingValue < grapplingCost) return;

        grappling = true;

        RaycastHit hit;
        if (Physics.Raycast(Cam.position, Cam.forward, out hit, maxGrappeDistance, WhatIsGrappable))
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
        grapplingValue -= grapplingCost;

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

        playerState.aState = ActionState.Passive;

        //FindObjectOfType<AudioManger>().Stop("Jetpack Loop");

        Destroy(LightObject);
    }
}
