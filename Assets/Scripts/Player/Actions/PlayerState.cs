using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerState : MonoBehaviour
{
    public GroundedState gState;
    public ActionState aState;

    public enum GroundedState
    {
        Grounded,
        InAir
    }

    public enum ActionState
    {
        Passive,
        Jumping,
        Dashing,
        Grappling,
        WallRunning
    }
}
