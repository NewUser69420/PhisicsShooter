using static PlayerState;
using UnityEngine;
using FishNet.Component.Animating;

public class GunPositioner : MonoBehaviour
{
    private NetworkAnimator netAnim;

    void Start()
    {
        netAnim = GetComponent<NetworkAnimator>();
    }

    public void AnimateLand()
    {
        netAnim.SetTrigger("land");
    }
}
