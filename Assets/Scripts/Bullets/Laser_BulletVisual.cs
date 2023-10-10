using System.Collections;
using UnityEngine;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using FishNet.Component.Prediction;
using FishNet.Managing.Timing;
using System.Linq;
using FishNet.Connection;

public class Laser_BulletVisual : NetworkBehaviour
{
    [SyncVar(OnChange = nameof(_startingForce_OnChange))]

    private Vector3 _startingForce;

    private uint _stopTick = TimeManager.UNSET_TICK;

    public override void OnStartNetwork()
    {
        if(this.IsSpawned) StartCoroutine(Wait());

        uint timeToTicks = base.TimeManager.TimeToTicks(0.65f);

        if (base.IsServer || base.Owner.IsLocalClient)
        {
            _stopTick = base.TimeManager.LocalTick + timeToTicks;
        }

        else
        {
            uint passed = (uint)Mathf.Max(1, base.TimeManager.Tick - base.TimeManager.LastPacketTick);
            long stopTick = (base.TimeManager.Tick + timeToTicks - passed - 1);
            if (stopTick > 0)
                _stopTick = (uint)stopTick;
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

    IEnumerator Wait()
    {
        yield return new WaitForSeconds(2f);
        if (this.IsSpawned) DespawnBullet();
    }

    public void SetStartingForce(Vector3 value)
    {
        if (!base.IsSpawned)
            SetVelocity(value);

        _startingForce = value;
    }

    private void _startingForce_OnChange(Vector3 prev, Vector3 next, bool asServer)
    {
        SetVelocity(next);
    }

    public void SetVelocity(Vector3 value)
    {
        Rigidbody rb = GetComponent<Rigidbody>();
        rb.velocity = value;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (base.IsServer) if (this.IsSpawned) { Invoke(nameof(DespawnBullet), 0.01f); }
        if (base.IsClient) if (this.IsSpawned) { Invoke(nameof(TurnOffBullet), 0.01f); }
    }

    private void DespawnBullet()
    {
        if (!base.IsServer) return;
        this.GetComponent<NetworkObject>().Despawn();
        Destroy(this);
    }

    private void TurnOffBullet()
    {
        transform.Find("Capsule").gameObject.SetActive(false);
    }
}
