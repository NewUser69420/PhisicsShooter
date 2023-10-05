using static PlayerState;
using UnityEngine;
using FishNet.Component.Animating;
using FishNet;
using Unity.Properties;
using System;
using UnityEngine.Animations.Rigging;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine.UIElements;

public class GunPositioner : MonoBehaviour
{
    [SerializeField] private Transform Cam;

    private Transform GunLookAssist;
    private Transform GunLandAssist;
    private Transform Gun;

    private Vector3 CurGunPos;

    private bool landing;

    private Vector3 velocity;
    private Vector3 velocityy;

    void Start()
    {
        GunLookAssist = transform.Find("GunLookAssist");
        GunLandAssist = transform.Find("GunLandAssist");
        Gun = transform.Find("Gun");
    }

    public void AnimateLand()
    {
        //doLanding
        landing = true;
        StartCoroutine(Wait());
    }

    private void Update()
    {
        if (!landing)
        {
            //do clamped shoulder movement
            Vector3 LookAsistEulerAngels = Cam.rotation.eulerAngles;
            if (LookAsistEulerAngels.x > 50 && LookAsistEulerAngels.x < 100) LookAsistEulerAngels.x = 50;
            if (LookAsistEulerAngels.x < 310 && LookAsistEulerAngels.x > 60) LookAsistEulerAngels.x = 310;
            GunLookAssist.rotation = Quaternion.Euler(LookAsistEulerAngels);
            CurGunPos = Vector3.SmoothDamp(Gun.position, (Vector3)((GunLookAssist.forward * 0.385f) + GunLookAssist.position), ref velocityy, 20 * Time.deltaTime);
        }
        else
        {
            CurGunPos = Vector3.SmoothDamp(Gun.position, GunLandAssist.position, ref velocity, 15 * Time.deltaTime);
        }

        //do gun pos/rot
        Gun.rotation = Cam.rotation;
        Gun.position = CurGunPos;
    }

    IEnumerator Wait()
    {
        yield return new WaitForSeconds(0.4f);
        landing = false;
    }
}
