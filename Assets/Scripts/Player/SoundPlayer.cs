using FishNet.Connection;
using FishNet.Object;
using System;
using UnityEngine;
using static PlayerState;

public class SoundPlayer : NetworkBehaviour
{
    private PlayerState playerState;
    private PlayerAudioManager playerAudioManager;
    private bool isPlayingGrappling;
    private bool isPlayingWallrunning;

    public override void OnStartNetwork()
    {
        playerState = GetComponent<PlayerState>();
        playerAudioManager = GetComponent<PlayerAudioManager>();

    }

    private void Update()
    {
        if(base.Owner.IsLocalClient)
        {
            switch(playerState.aState)
            {
                case ActionState.Passive:
                    //reset grappling + wallrunning sounds
                    playerAudioManager.Stop("Grappling");
                    playerAudioManager.Stop("WallRunning");

                    AskToStopRpc(this.gameObject, "Grappling");
                    AskToStopRpc(this.gameObject, "WallRunning");

                    isPlayingGrappling = false;
                    isPlayingWallrunning = false;
                    break;
                case ActionState.Grappling:
                    //do grappling sounds
                    if(!isPlayingGrappling)
                    {
                        Debug.Log("Grappling Test");
                        playerAudioManager.Play("Grappling");
                        AskToStartRpc(this.gameObject, "Grappling");

                        isPlayingGrappling = true;
                    }
                    break;
                case ActionState.WallRunning:
                    //do wallrunning sounds
                    if (!isPlayingWallrunning)
                    {
                        Debug.Log("Grappling Test");
                        playerAudioManager.Play("WallRunning");
                        AskToStartRpc(this.gameObject, "WallRunning");

                        isPlayingWallrunning = true;
                    }
                    break;
            }
        }
    }

    public void PlaySound(String _sound)
    {
        playerAudioManager.Play(_sound);
    }

    public void StopSound(String _sound)
    {
        playerAudioManager.Stop(_sound);
    }

    [ServerRpc]
    private void AskToStartRpc(GameObject obj, String sound)
    {
        StartSoundServerRpc(obj, sound);
    }

    [ServerRpc]
    private void AskToStopRpc(GameObject obj, String sound)
    {
        StopSoundServerRpc(obj, sound);
    }

    [ObserversRpc]
    private void StartSoundServerRpc(GameObject _obj, String _sound)
    {
        if (!base.Owner.IsLocalClient) _obj.GetComponent<SoundPlayer>().PlaySound(_sound);
    }

    [ObserversRpc]
    private void StopSoundServerRpc(GameObject _obj, String _sound)
    {
        if (!base.Owner.IsLocalClient) _obj.GetComponent<SoundPlayer>().StopSound(_sound);
    }
}
