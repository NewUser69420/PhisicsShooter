using static PlayerState;
using UnityEngine;
using UnityEngine.Playables;

public class feet : MonoBehaviour
{
    private PlayerState state;
    private float groundedTimer;
    [SerializeField] private float groundedTimerMax;
    [SerializeField] private feet otherFeet;

    private void Start()
    {
        state = GetComponentInParent<PlayerState>();
    }

    private void OnTriggerStay(Collider other)
    {
        if (state == null) return;
        if (other.gameObject.layer == 8 || other.gameObject.layer == 10)
        {
            state.gState = GroundedState.Grounded;
            groundedTimer = groundedTimerMax;
            otherFeet.groundedTimer = groundedTimerMax;
        }
    }

    private void Update()
    {
        if (state == null) return;
        
        if (groundedTimer > 0)
        {
            groundedTimer -= Time.deltaTime;
        }
        else
        {
            state.gState = GroundedState.InAir;
        }
    }
}
