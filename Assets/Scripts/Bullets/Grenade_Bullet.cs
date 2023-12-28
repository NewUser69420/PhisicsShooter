using FishNet.Managing.Timing;
using FishNet.Object.Synchronizing;
using FishNet.Object;
using UnityEngine;
using System.Collections;

public class Grenade_Bullet : NetworkBehaviour
{
    private bool fuze = true;

    //SyncVar to set spawn force. This is set by predicted spawner and sent to the server.
    private readonly SyncVar<Vector3> _startingForce = new SyncVar<Vector3>();
    //Tick to set rb to kinematic.
    private uint _stopTick = TimeManager.UNSET_TICK;

    private void Awake()
    {
        _startingForce.OnChange += _startingForce_OnChange;
    }

    public void SetStartingForce(Vector3 value)
    {
        /* If the object is not yet initialized then
         * this is being set prior to network spawning.
         * Such a scenario occurs because the client which is
         * predicted spawning sets the synctype value before network
         * spawning to ensure the server receives the value.
         * Just as when the server sets synctypes, if they are set
         * before the object is spawned it's gauranteed clients will
         * get the value in the spawn packet; same practice is used here. */
        if (!base.IsSpawned)
            SetVelocity(value);

        _startingForce.Value = value;
    }

    public override void OnStartNetwork()
    {
        if (base.IsServerStarted) { StartCoroutine(Wait()); StartCoroutine(Wait2()); }

        uint timeToTicks = base.TimeManager.TimeToTicks(0.65f);
        /* If server or predicted spawner then add the kinematic
         * tick onto local. Predicted spawner and server should behave
         * as though no time has elapsed since this spawned. */
        if (base.IsServerStarted || base.Owner.IsLocalClient)
        {
            _stopTick = base.TimeManager.LocalTick + timeToTicks;
        }
        /* If not server or a client that did not predicted spawn this
         * then calculate time passed and set kinematic tick to the same
         * amount in the future while subtracting already passed ticks. */
        else
        {
            uint passed = (uint)Mathf.Max(1, base.TimeManager.Tick - base.TimeManager.LastPacketTick);
            long stopTick = (base.TimeManager.Tick + timeToTicks - passed - 1);
            if (stopTick > 0)
                _stopTick = (uint)stopTick;
            //Time already passed, set to stop next tick.
            else
                _stopTick = 1;
        }

        base.TimeManager.OnTick += TimeManager_OnTick;
    }

    public override void OnStopNetwork()
    {
        base.TimeManager.OnTick -= TimeManager_OnTick;
    }
    private void TimeManager_OnTick()
    {
        if (_stopTick > 0 && base.TimeManager.LocalTick >= _stopTick)
        {
            Rigidbody rb = GetComponent<Rigidbody>();
            rb.isKinematic = true;
        }
    }

    /// <summary>
    /// When starting force changes set that velocity to the rigidbody.
    /// This is an example as how a predicted spawn can modify sync types for server and other clients.
    /// </summary>
    private void _startingForce_OnChange(Vector3 prev, Vector3 next, bool asServer)
    {
        SetVelocity(next);
    }

    /// <summary>
    /// Sets velocity of the rigidbody.
    /// </summary>
    public void SetVelocity(Vector3 value)
    {
        Rigidbody rb = GetComponent<Rigidbody>();
        rb.velocity = value;
    }

    IEnumerator Wait()
    {
        yield return new WaitForSeconds(5f);
        Boom();
    }

    IEnumerator Wait2()
    {
        yield return new WaitForSeconds(0.5f);
        fuze = false;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (base.IsServerStarted) if (this.IsSpawned && !fuze) { Invoke(nameof(Boom), 0.01f); }
    }

    private void Boom()
    {
        Debug.Log("BOOM");
        transform.Find("ForceField").gameObject.SetActive(true);
        Invoke(nameof(DespawnBullet), 1f);
    }

    private void DespawnBullet()
    {
        if (!base.IsServerStarted) return;
        this.GetComponent<NetworkObject>().Despawn();
        Destroy(this);
    }
}