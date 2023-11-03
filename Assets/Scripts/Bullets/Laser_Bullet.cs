using System.Collections;
using UnityEngine;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using FishNet.Component.Prediction;
using FishNet.Managing.Timing;
using System.Linq;
using FishNet.Connection;

public class Laser_Bullet : NetworkBehaviour
{
    private ServerHealthManager SHealth;

    [System.NonSerialized] public NetworkConnection PlayerConn;

    [System.NonSerialized] public GameObject lastHitObject;
    public GameObject SoundObjectPrefab;

    public int damage = 50;
    public string team;
    private bool canDoDamage = true;

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

    //Simple delay destroy so object does not exist forever.
    public override void OnStartServer()
    {

        StartCoroutine(__DelayDestroy(3f));
    }

    public override void OnStartNetwork()
    {
        if (this.IsSpawned) StartCoroutine(Wait());

        //Make sound
        GameObject SoundObject = Instantiate(SoundObjectPrefab);
        SoundObject.GetComponent<SoundObject>().TrackedObject = gameObject.transform;


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

    /// <summary>
    /// Destroy object after time.
    /// </summary>
    private IEnumerator __DelayDestroy(float time)
    {
        yield return new WaitForSeconds(time);
        base.Despawn();
    }

    IEnumerator Wait()
    {
        yield return new WaitForSeconds(2f);
        if(this.IsSpawned) DespawnBullet();
    }

    void OnTriggerEnter(Collider collider){
        if(base.IsOwner)
        {
            FindObjectOfType<AudioManger>().Play("HitMarker");
        }
        
        if (!IsServerStarted) return;
        if (collider.gameObject.layer == 12)
        {

            if(!canDoDamage) return;
            canDoDamage = !canDoDamage;

            string name = collider.gameObject.name;
            var id = collider.gameObject.GetComponentInParent<NetworkObject>().ClientManager.Connection.ClientId;
            lastHitObject = collider.gameObject;

            SHealth = collider.GetComponentInParent<ServerHealthManager>();


            //do damage
            switch (name)
            {
                case "Hips":
                    SHealth.OnHealthChange(0, SHealth.health[0] - damage, SHealth.health[0], PlayerConn, team);
                    SHealth.health[0] -= damage;
                    break;
                case "Left Thigh":
                    SHealth.OnHealthChange(1, SHealth.health[1] - damage, SHealth.health[1], PlayerConn, team);
                    SHealth.health[1] -= damage;
                    break;
                case "Left Leg":
                    SHealth.OnHealthChange(2, SHealth.health[2] - damage, SHealth.health[2], PlayerConn, team);
                    SHealth.health[2] -= damage;
                    break;
                case "Left Foot":
                    SHealth.OnHealthChange(3, SHealth.health[3] - damage, SHealth.health[3], PlayerConn, team);
                    SHealth.health[3] -= damage;
                    break;
                case "Right Thigh":
                    SHealth.OnHealthChange(4, SHealth.health[4] - damage, SHealth.health[4], PlayerConn, team);
                    SHealth.health[4] -= damage;
                    break;
                case "Right Leg":
                    SHealth.OnHealthChange(5, SHealth.health[5] - damage, SHealth.health[5], PlayerConn, team);
                    SHealth.health[5] -= damage;
                    break;
                case "Right Foot":
                    SHealth.OnHealthChange(6, SHealth.health[6] - damage, SHealth.health[6], PlayerConn, team);
                    SHealth.health[6] -= damage;
                    break;
                case "Spine 1":
                    SHealth.OnHealthChange(7, SHealth.health[7] - damage, SHealth.health[7], PlayerConn, team);
                    SHealth.health[7] -= damage;
                    break;
                case "Spine 2":
                    SHealth.OnHealthChange(8, SHealth.health[8] - damage, SHealth.health[8], PlayerConn, team);
                    SHealth.health[8] -= damage;
                    break;
                case "Spine 3":
                    SHealth.OnHealthChange(9, SHealth.health[9] - damage, SHealth.health[9], PlayerConn, team);
                    SHealth.health[9] -= damage;
                    break;
                case "Left Shoulder":
                    SHealth.OnHealthChange(10, SHealth.health[10] - damage, SHealth.health[10], PlayerConn, team);
                    SHealth.health[10] -= damage;
                    break;
                case "Left Arm":
                    SHealth.OnHealthChange(11, SHealth.health[11] - damage, SHealth.health[11], PlayerConn, team);
                    SHealth.health[11] -= damage;
                    break;
                case "Left Forearm":
                    SHealth.OnHealthChange(12, SHealth.health[12] - damage, SHealth.health[12], PlayerConn, team);
                    SHealth.health[12] -= damage;
                    break;
                case "Left Hand":
                    SHealth.OnHealthChange(13, SHealth.health[13] - damage, SHealth.health[13], PlayerConn, team);
                    SHealth.health[13] -= damage;
                    break;
                case "Right Shoulder":
                    SHealth.OnHealthChange(14, SHealth.health[14] - damage, SHealth.health[14], PlayerConn, team);
                    SHealth.health[14] -= damage;
                    break;
                case "Right Arm":
                    SHealth.OnHealthChange(15, SHealth.health[15] - damage, SHealth.health[15], PlayerConn, team);
                    SHealth.health[15] -= damage;
                    break;
                case "Right Forearm":
                    SHealth.OnHealthChange(16, SHealth.health[16] - damage, SHealth.health[16], PlayerConn, team);
                    SHealth.health[16] -= damage;
                    break;
                case "Right Hand":
                    SHealth.OnHealthChange(17, SHealth.health[17] - damage, SHealth.health[17], PlayerConn, team);
                    SHealth.health[17] -= damage;
                    break;
                case "Neck":
                    SHealth.OnHealthChange(18, SHealth.health[18] - damage, SHealth.health[18], PlayerConn, team);
                    SHealth.health[18] -= damage;
                    break;
                case "Head":
                    SHealth.OnHealthChange(19, SHealth.health[19] - damage, SHealth.health[19], PlayerConn, team);
                    SHealth.health[19] -= damage;
                    break;
            }

            if (this.IsSpawned) DespawnBullet();
        }
        
        
    }

    private void OnCollisionEnter(Collision collision)
    {
        if(base.IsServerStarted) if (this.IsSpawned) { Invoke(nameof(DespawnBullet), 0.01f); }
        if(base.IsClientInitialized) if (this.IsSpawned) { Invoke(nameof(TurnOffBullet), 0.01f); }
    }

    private void DespawnBullet(){
        if(!base.IsServerStarted) return;
        this.GetComponent<NetworkObject>().Despawn();
        Destroy(this);
    }

    private void TurnOffBullet()
    {
        transform.Find("CapsuleUnused").gameObject.SetActive(false);
    }
}
