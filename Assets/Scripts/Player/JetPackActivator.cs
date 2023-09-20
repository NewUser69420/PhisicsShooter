using static PlayerState;
using UnityEngine;
using FishNet.Object;
using System;
using System.Collections;
using UnityEditor.Rendering;

public class JetPackActivator : NetworkBehaviour
{
    private PlayerState playerState;
    [SerializeField] private ParticleSystem ps;
    private float buffer;

    public override void OnStartNetwork()
    {
        playerState = GetComponentInParent<PlayerState>();
        gameObject.layer = 11;
    }

    private void Update()
    {
        switch(playerState.aState) 
        {
            case ActionState.Passive:
                //nothing?
                break;
            case ActionState.Jumping:
                buffer += 0.5f;
                break;
            case ActionState.Dashing:
                buffer += 0.7f;
                break;
            case ActionState.Grappling:
                buffer += 0.1f;
                break;
            case ActionState.WallRunning:
                //nothing?
                break;
        }

        if (buffer >= 1) buffer = 1f;

        var em = ps.emission;

        if (buffer > 0)
        {
            buffer -= Time.deltaTime;
            em.enabled = true;
        }
        else
        {
            em.enabled = false;
        }
    }
}
