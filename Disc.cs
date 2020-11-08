using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum DiscState 
{ 
    Dispensed,  // At the station
    Attached,   // With the player
    Caught,     // Entered the player's trigger
    Fired,      // Thrown 
    Flying,     // Flying
    Recalled,   // Moving towards the player
    Breaking,   // Ran out of bounces
    Connected,  // To the power station
    Charging,   // Connected to the charge station
    Charged,    // Charged 100%
}

public enum RecallType
{
    Simple,
    Rewind,
    SimpleRewind,
    Immediate,
}

public enum DiscElementalState
{
    None,
    Fire,
}

public enum DiscStartingPlace
{
    Dispenser,
    ChargingStation,
    PowerStation,
}

[RequireComponent(typeof(Rigidbody), typeof(TrailRenderer))]
public class Disc : Singleton<Disc>
{
    [SerializeField, Tooltip("How the disc returns to the player on recall")]
    RecallType recallType;

    [SerializeField]
    DiscStartingPlace startingPlace;

    [SerializeField, Tooltip("The disc model")]
    new Renderer renderer;

    [SerializeField]
    TrailRenderer trailRenderer;

    [SerializeField]
    Material defaultMaterial;

    [SerializeField]
    Material recallMaterial;

    [SerializeField]
    bool isCharged = false;
    public bool IsCharged 
    { 
        get { return isCharged; }
        set
        {
            isCharged = value;
            if (!isCharged)
                renderer.material.SetColor("_EmissionColor", Color.red);
            else
                renderer.material.SetColor("_EmissionColor", Color.green);
        }
    }

    [SerializeField, Tooltip("How fast the disc travels")]
    float speed = 20f;

    [SerializeField, Tooltip("How fast the disc moves when recalled")]
    float recallSpeed = 75f;

    [SerializeField, Tooltip("How fast the disc comes back to you after last bounce")]
    float returnSpeed = 50f;

    [SerializeField, Tooltip("How many bounces before it breaks")]
    int totalBounces = 5;

    [SerializeField, Tooltip("How much to change the pitch by on each bounce")]
    float bouncePithChange = 0.25f;

    [SerializeField, Range(0.1f, 1f), Tooltip("How transperant to make the disc when returning")]
    float alphaOnReturn = 0.25f;

    [SerializeField, Range(1f, 3f), Tooltip("How long to be fully charged")]
    float chargingTime = 1f;
    float currentCharge = 0f;

    [SerializeField]
    DiscDispenser discDispenser;

    [SerializeField]
    PowerStation powerStation;

    [SerializeField]
    ChargeStation chargeStation;

    public DiscState State { get; private set; }
    public DiscElementalState ElementalState { get; set; }

    public Rigidbody Rigidbody { get; private set; }

    public WindTrigger WindTrigger { get; set; }

    Player player;
    Transform targetAttachedTo;

    int bounces = 0;
    List<Vector3> collisionPoints;

    /// <summary>
    /// Audio Clips
    /// </summary>
    [SerializeField]
    AudioClipInfo[] bounceClipInfos;

    [SerializeField]
    AudioClipInfo breakClipInfo;

    [SerializeField]
    AudioClipInfo chargingClipInfo;

    private void Start()
    {
        Init();
    }

    public void Init()
    {
        renderer.material.EnableKeyword("_Emission");

        Rigidbody = GetComponent<Rigidbody>();
        if (renderer == null)
            renderer = GetComponent<Renderer>();

        if (trailRenderer == null)
            trailRenderer = GetComponent<TrailRenderer>();

        collisionPoints = new List<Vector3>();
        player = FindObjectOfType<Player>();

        IsCharged = isCharged;
        currentCharge = isCharged ? 1f : 0f;

        switch (startingPlace)
        {
            case DiscStartingPlace.Dispenser:
                ChangeState(DiscState.Dispensed);
                break;

            case DiscStartingPlace.ChargingStation:
                chargeStation.IsPoweredOn = isCharged;
                ChangeState(DiscState.Charging);
                break;

            case DiscStartingPlace.PowerStation:
                ChangeState(DiscState.Connected);
                break;
        }
    }

    private void Update()
    {
        switch(State)
        {
            case DiscState.Attached:
            case DiscState.Connected:
            case DiscState.Dispensed:
            case DiscState.Charging:
            case DiscState.Charged:
                bounces = 0;
                if (targetAttachedTo != null)
                {
                    transform.position = targetAttachedTo.position;
                    transform.rotation = targetAttachedTo.rotation;
                }
                break;
        }
    }

    private void FixedUpdate()
    {
        switch (State)
        {
            case DiscState.Fired:
                collisionPoints.Clear();
                collisionPoints.Add(player.DiscMountingPoint.position);
                
                bounces = 0;
                transform.SetParent(null);

                transform.position = player.DiscLaunchPoint.position;
                transform.rotation = Quaternion.identity;
                transform.forward = player.transform.forward;

                Rigidbody.detectCollisions = true;
                Rigidbody.velocity = player.transform.forward * speed;

                trailRenderer.Clear();
                State = DiscState.Flying;
                break;

            case DiscState.Flying:
                var force = Rigidbody.velocity.normalized * speed;

                if (WindTrigger != null)
                    force += WindTrigger.Force;

                Rigidbody.velocity = force;
                break;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        switch (State)
        {
            case DiscState.Flying:
                collisionPoints.Add(transform.position);

                var info = bounces < bounceClipInfos.Length ? bounceClipInfos[bounces] : bounceClipInfos[0];
                if (bounces++ < totalBounces)
                    AudioManager.instance.Play2DSound(info.clip);
                else
                {
                    AudioManager.instance.Play2DSound(breakClipInfo);
                    ChangeState(DiscState.Breaking);
                }                    
                break;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Can only be "caught" when it is flying
        if (State != DiscState.Flying)
            return;

        if (other.CompareTag("PlayerPickupTrigger") && player.IsPoweredOn)
        {
            StopDisc();
            ChangeState(DiscState.Caught);
        }   
    }

    /// <summary>
    /// Changes the current state
    /// Applies any logic for then new state to take effect
    /// </summary>
    /// <param name="state"></param>
    public void ChangeState(DiscState state)
    {
        State = state;
        ElementalState = DiscElementalState.None;

        switch (state)
        {
            case DiscState.Attached:
                AttachToPlayer();
                break;

            case DiscState.Breaking:
                TriggerBreakRoutine();
                break;

            case DiscState.Dispensed:
                AttachToDispenser();
                break;

            case DiscState.Connected:
                AttachToPowerStation();
                break;

            case DiscState.Caught:
                StopDisc();
                player.TriggerCatchingRoutine(true);
                break;

            case DiscState.Charging:
                TriggerChargingRoutine();
                break;

            case DiscState.Fired:
                trailRenderer.enabled = true;
                trailRenderer.Clear();
                break;

            case DiscState.Recalled:
                TriggerRecallRoutine();
                break;            
        }        
    }

    private void TriggerChargingRoutine()
    {
        StartCoroutine(ChargingRoutine());
    }

    void AttachToDispenser()
    {
        AttachToTarget(discDispenser.DiscMountingPoint);
    }

    void AttachToPlayer()
    {
        AttachToTarget(player.DiscMountingPoint);
    }

    void AttachToPowerStation()
    {
        //LevelController.instance.LevelCompleted()
        AttachToTarget(powerStation.DiscMountingPoint);
    }

    void AttachToChargingStation()
    {
        AttachToTarget(chargeStation.DiscMountingPoint);
    }

    void AttachToTarget(Transform target)
    {
        StopDisc();

        targetAttachedTo = target;
        transform.position = target.position;
        transform.rotation = target.rotation;

        trailRenderer.Clear();
        trailRenderer.enabled = false;
        SetRendererAlpha(1f);
    }

    void StopDisc()
    {
        Rigidbody.detectCollisions = false;
        Rigidbody.velocity = Vector3.zero;
    }

    void TriggerRecallRoutine()
    {
        transform.rotation = Quaternion.identity;

        switch(recallType)
        {
            case RecallType.Simple:
                StartCoroutine(SimpleReturnRoutine(player.DiscMountingPoint));
                break;
            case RecallType.Rewind:
                StartCoroutine(RewindRecallRoutine());
                break;
            case RecallType.SimpleRewind:
                StartCoroutine(SimpleRewindRecallRoutine());
                break;
            case RecallType.Immediate:
                ChangeState(DiscState.Attached);
                break;
            default:
                Debug.LogError($"{name} - RecalType: {recallType} does not have a defined routine");
                break;
        }
    }

    void TriggerBreakRoutine()
    {
        StartCoroutine(BreakRoutine());
    }

    IEnumerator BreakRoutine()
    {
        AudioManager.instance.Play2DSound(breakClipInfo);
        SetRendererAlpha(0f);
        IsCharged = LevelController.instance.BatteryChargedOnBreak;
        yield return new WaitForEndOfFrame();

        ChangeState(DiscState.Dispensed);
    }

    /// <summary>
    /// Follows the player until it gets close enough to re-attach to the player
    /// </summary>
    /// <returns></returns>
    IEnumerator SimpleReturnRoutine(Transform target, bool attachToPlayer = true)
    {
        SetRendererAlpha(alphaOnReturn);

        Rigidbody.detectCollisions = false;
        Rigidbody.velocity = Vector3.zero;

        while (Vector3.Distance(target.position, transform.position) > 0.01f)
        {
            // Player no longer has power - cannot attach to it
            if (attachToPlayer && !player.IsPoweredOn)
                break;

            yield return new WaitForEndOfFrame();
            transform.position = Vector3.MoveTowards(transform.position, target.position, returnSpeed * Time.deltaTime);
        }

        // Keeps moving in the direction it was travelling 
        if (attachToPlayer && !player.IsPoweredOn)
        {
            var dir = player.transform.position - transform.position;
            dir.y = 0f;

            Rigidbody.detectCollisions = true;
            Rigidbody.velocity = dir.normalized * speed;
            ChangeState(DiscState.Flying);
        }
        else
        {
            if (attachToPlayer)
                ChangeState(DiscState.Attached);
            else
                ChangeState(DiscState.Dispensed);
        }        
    }

    /// <summary>
    /// Moves backwards in time revisting all the places it went to until reaching the player
    /// </summary>
    /// <returns></returns>
    IEnumerator RewindRecallRoutine()
    {
        SetRendererAlpha(alphaOnReturn);

        player.DisableInputs = true;
        Rigidbody.detectCollisions = false;
        Rigidbody.velocity = Vector3.zero;

        // Ensure to always end up back to where the player is currently at
        if (collisionPoints.FirstOrDefault() != player.DiscMountingPoint.position)
            collisionPoints.Insert(0, player.DiscMountingPoint.position);

        collisionPoints.Reverse();

        while (collisionPoints.Count > 0)
        {
            var dest = collisionPoints.First();
            collisionPoints.Remove(dest);

            while(Vector3.Distance(dest, transform.position) > 0.01f)
            {
                yield return new WaitForEndOfFrame();
                transform.position = Vector3.MoveTowards(transform.position, dest, recallSpeed * Time.deltaTime);
            }            
        }

        ChangeState(DiscState.Attached);
    }

    /// <summary>
    /// Teleport to each previous "collision point" and then snaps back to the player
    /// </summary>
    /// <returns></returns>
    IEnumerator SimpleRewindRecallRoutine()
    {
        SetRendererAlpha(alphaOnReturn);

        Rigidbody.detectCollisions = false;
        Rigidbody.velocity = Vector3.zero;

        collisionPoints.Reverse();
        while (collisionPoints.Count > 0)
        {
            var destination = collisionPoints.First();
            collisionPoints.Remove(destination);

            // Snap to the point
            transform.position = destination;

            // Wait for a bit 
            yield return new WaitForSeconds(0.25f);
        }

        ChangeState(DiscState.Attached);
    }

    IEnumerator ChargingRoutine()
    {
        AttachToChargingStation();
        currentCharge = IsCharged ? 1f : 0f;
        var targetColor = Color.green;

        AudioManager.instance.Play2DSound(chargingClipInfo);
        while (currentCharge < 1f)
        {
            yield return new WaitForEndOfFrame();
            currentCharge += 1.0f / chargingTime * Time.deltaTime;
            var color = Color.Lerp(renderer.material.color, targetColor, currentCharge);
            renderer.material.SetColor("_EmissionColor", color);
        }

        currentCharge = 1f;
        IsCharged = true;
        ChangeState(DiscState.Charged);
    }

    /// <summary>
    /// Originally was only changing alpha but the new disc's material gets funky
    /// when we do that so using two different materials now
    /// </summary>
    /// <param name="alpha"></param>
    void SetRendererAlpha(float alpha)
    {
        //alpha = Mathf.Clamp01(alpha);
        //var color = renderer.material.color;
        //color.a = alpha;
        //renderer.material.color = color;
        //renderer.material = alpha == 1 ? defaultMaterial : recallMaterial;

        // We need to disable the renderer when the alpha is zero
        // because the object still receives/casts shadows
        renderer.enabled = alpha != 0f;
        if (alpha == 0f)
            trailRenderer.Clear();
    }
}
