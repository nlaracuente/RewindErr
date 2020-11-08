using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PlayerState
{
    Disabled,
    PoweredOff,
    PoweringOn,
    PoweredOn,
    Throwing,
    Recalling,
    BatteryCaught,
    LevelCompleted,
    GameCompleted,
    GameRewinded,
}

[RequireComponent(typeof(Rigidbody))]
public class Player : Singleton<Player>
{
    Transform lookAtTarget;
    Transform LookAtTarget
    {
        get {
            if (lookAtTarget == null)
                lookAtTarget = GameObject.FindGameObjectWithTag("LookAtTarget").transform;
            return lookAtTarget;
        }
    }

    [SerializeField]
    Animator animator;

    [SerializeField]
    LayerMask mouseCollisionLayer;

    [SerializeField]
    float mouseRayLength = 1000f;

    [SerializeField]
    Collider discCatchTrigger;

    [SerializeField]
    Transform discHolder;
    public Transform DiscMountingPoint { get { return discHolder; } }

    [SerializeField]
    Transform discLaunchPoint;
    public Transform DiscLaunchPoint { get { return discLaunchPoint; } }

    [SerializeField]
    LineRenderer lineRenderer;
    public Transform LineRendererXform { get { return lineRenderer.transform; } }

    [SerializeField, Tooltip("LayerMask to detect where the disc will collide first")]
    LayerMask collisionLayer;

    [SerializeField]
    RewindTimer rewindTimer;

    [SerializeField]
    Spinner windupKeySpinner;

    [SerializeField]
    float moveSpeed = 5f;
    public float MoveSpeed { get { return moveSpeed; } }

    /// <summary>
    /// Rotation speed
    /// </summary>
    [SerializeField, Tooltip("How fast the robot turns")]
    float rotationSpeed = 15f;
    
    /// <summary>
    /// Keeps track of angle of rotation for a smoother turn
    /// </summary>
    float rotationAngle;

    Rigidbody rigidBody;
    Vector3 movement;

    Disc disc;
    Disc Disc 
    {
        get
        { 
            if(disc == null)
                disc = FindObjectOfType<Disc>();
            return disc;
        }
    }

    public bool DisableInputs { get; set; } = true;

    public PlayerState State { get; private set; } = PlayerState.PoweredOff;

    [SerializeField, Tooltip("Total seconds to windup")]
    float rewindSpeed = 2f;

    [SerializeField, Tooltip("Total seconds to unwindup")]
    float unwindSpeed = 30f;

    [SerializeField]
    Vector3 windingUpAxis = new Vector3(0f, 300f, 0f);

    [SerializeField]
    Vector3 windingDownAxis = new Vector3(0f, -100f, 0f);

    [SerializeField, Tooltip("How long the powering ON animation plays")]
    float poweringOnDelay = 0.20f;    

    [SerializeField, Tooltip("How long before trigger the launch of the disc")]
    float launchDiscDelay = 0.25f;

    [SerializeField, Tooltip("How long to wait for throwing animation to complete before going back to idle")]
    float throwingDiscDelay = 1f;

    [SerializeField, Tooltip("How long to wait for catch animation")]
    float cathingDelay = .25f;

    public float CurrentPower { get; set; } = 0f;
    public float MaxPower { get; private set; } = 1f;
    public bool IsPoweredOn 
    { 
        get {
            return State != PlayerState.PoweredOff
                   && State != PlayerState.PoweringOn
                   && CurrentPower > 0f; 
        }
    }

    /// <summary>
    /// Audio Clips
    /// </summary>
    [SerializeField]
    AudioClipInfo walkingClipInfo;

    [SerializeField]
    AudioClipInfo rewindingClipInfo;

    [SerializeField]
    AudioClipInfo windingDownClipInfo;

    [SerializeField]
    AudioClipInfo powerOnClipInfo;
    
    [SerializeField]
    AudioClipInfo powerOffClipInfo;
    
    [SerializeField]
    AudioClipInfo throwClipInfo;

    [SerializeField]
    AudioClipInfo catchClipInfo;

    [SerializeField]
    AudioClipInfo recallClipInfo;

    Dictionary<string, AudioSource> loopingAudioInfo = new Dictionary<string, AudioSource>();

    bool DisableUpdates
    {
        get
        {
            return (State == PlayerState.Disabled
                || State == PlayerState.GameCompleted
                || State == PlayerState.GameRewinded
                || State == PlayerState.LevelCompleted)
                || MenuController.instance.IsGamePaused;
        }
    }

    private void Start()
    {
        rigidBody = GetComponent<Rigidbody>();
        if (rewindTimer == null)
            rewindTimer = GetComponentInChildren<RewindTimer>();

        if (windupKeySpinner == null)
            windupKeySpinner = GetComponentInChildren<Spinner>();

        discCatchTrigger.isTrigger = true;
        discCatchTrigger.enabled = false;
        ChangeState(PlayerState.Disabled);
    }

    void Update()
    {
        if (DisableUpdates)
            return;

        movement = Vector3.zero;
        windupKeySpinner.Stop();

        switch (State)
        {
            case PlayerState.PoweredOff:
                if (Input.GetKey(KeyCode.Space))
                    Rewind();
                else
                    Unwind();
                break;

            case PlayerState.PoweredOn:
                movement.Set(Input.GetAxisRaw("Horizontal"), 0f, Input.GetAxisRaw("Vertical"));

                if (movement.x == 0f)
                    PauseLoopingClip(walkingClipInfo);
                else
                    PlayLoopingClip(walkingClipInfo);

                // Only when not performing any other actions can we rewind
                if (movement == Vector3.zero    // Not moving
                    && !Input.GetKeyDown(KeyCode.Mouse0)  // No Actions
                    && Input.GetKey(KeyCode.Space)) // Requesting rewind
                    Rewind();
                else
                    Unwind();

                // Ran out of juice
                if (State != PlayerState.PoweredOn)
                    return;

                // Parse other inputs
                switch (Disc.State)
                {
                    case DiscState.Dispensed:
                    case DiscState.Charged:
                    case DiscState.Flying:
                        if (Input.GetKeyDown(KeyCode.Mouse0))
                            ChangeState(PlayerState.Recalling);
                        break;

                    case DiscState.Attached:
                        if (Input.GetKeyDown(KeyCode.Mouse0))
                            ChangeState(PlayerState.Throwing);
                        break;
                }
                break;
            
            // Always be losing power
            default:
                Unwind();
                break;
        }

        animator.SetBool("IsMoving", movement.x != 0f);
    }

    private void FixedUpdate()
    {
        if (DisableUpdates)
            return;

        if (DisableInputs || movement == Vector3.zero)
            return;

        switch (State)
        {
            case PlayerState.PoweredOn:
                // Face direction
                float targetAngle = Mathf.Atan2(movement.x, movement.z) * Mathf.Rad2Deg;
                rotationAngle = Mathf.LerpAngle(rotationAngle, targetAngle, rotationSpeed * Time.deltaTime);
                rigidBody.MoveRotation(Quaternion.Euler(Vector3.up * rotationAngle));

                // Move in direction (only moving in X axis)
                var m = movement;
                m.z = 0f;

                var dir = Camera.main.transform.TransformDirection(m);
                dir.y = 0f;
                rigidBody.MovePosition(rigidBody.position + dir * moveSpeed * Time.deltaTime);
                break;
        }
    }

    private void LateUpdate()
    {
        if (DisableUpdates)
            return;

        lineRenderer.positionCount = 0;
        if (!DisableInputs && State == PlayerState.PoweredOn && Disc.State == DiscState.Attached)
            PreviewThrowPath();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (DisableUpdates)
            return;

        if (other.CompareTag("EndLevelTrigger"))
            LevelController.instance.LevelCompleted();
    }

    private void Rewind()
    {
        // Ensure the winding down sound is now playing when rewinding
        PauseLoopingClip(windingDownClipInfo);

        // Fully powered...we are good
        if (CurrentPower == 1f)
            PauseLoopingClip(rewindingClipInfo);
        else
        {
            // Still have some reminding left
            windupKeySpinner.Axis = windingUpAxis;
            PlayLoopingClip(rewindingClipInfo);
        }

        CurrentPower = Mathf.Min(CurrentPower + 1.0f / rewindSpeed * Time.deltaTime, 1f);
        if (CurrentPower == MaxPower && State == PlayerState.PoweredOff)
            ChangeState(PlayerState.PoweringOn);
    }

    private void Unwind()
    {
        // Ensure the winding up sound is not playing when winding down
        PauseLoopingClip(rewindingClipInfo);

        // No more juice
        if (CurrentPower == 0f)
            PauseLoopingClip(windingDownClipInfo);
        else
        {
            // Still have juice so wind down
            windupKeySpinner.Axis = windingDownAxis;
            PlayLoopingClip(windingDownClipInfo);
        }
        
        CurrentPower = Mathf.Max(CurrentPower - 1.0f / unwindSpeed * Time.deltaTime, 0f);

        if (CurrentPower == 0f && State != PlayerState.PoweredOff)
        {
            // Doing it here to ensure this only happened after an unwind
            // rather than on a change state
            animator.SetTrigger("PowerDown");
            AudioManager.instance.Play2DSound(powerOffClipInfo);
            ChangeState(PlayerState.PoweredOff);
        } 
    }

    void PlayLoopingClip(AudioClipInfo info)
    {
        if (!loopingAudioInfo.ContainsKey(info.clip.name) || loopingAudioInfo[info.clip.name] == null)
        {
            if (!loopingAudioInfo.ContainsKey(info.clip.name))
                loopingAudioInfo.Add(info.clip.name, AudioManager.instance.Play2DSound(info));
            else
                loopingAudioInfo[info.clip.name] = AudioManager.instance.Play2DSound(info);
            
        }

        if(loopingAudioInfo[info.clip.name] != null && !loopingAudioInfo[info.clip.name].isPlaying)
            loopingAudioInfo[info.clip.name].Play();
    }

    void PauseLoopingClip(AudioClipInfo info)
    {
        if (loopingAudioInfo.ContainsKey(info.clip.name) && loopingAudioInfo[info.clip.name] != null)
            loopingAudioInfo[info.clip.name].Pause();
    }

    void PreviewThrowPath()
    {
        var start = new Vector3(transform.position.x, lineRenderer.transform.position.y, transform.position.z);
        List<Vector3> points = new List<Vector3>() { start };

        var direction = LookAtTarget.position - transform.position;
        direction.y = 0f;

        var end = direction * 1000f;
        RaycastHit hit;

        if (Physics.Linecast(start, end, out hit, collisionLayer))
            points.Add(hit.point);

        lineRenderer.positionCount = points.Count;
        lineRenderer.SetPositions(points.ToArray());
    }

    public void ChangeState(PlayerState state)
    {
        State = state;
        PauseLoopingClip(walkingClipInfo);

        switch (state)
        {
            case PlayerState.PoweredOff:
            case PlayerState.GameCompleted:
                DisableInputs = true;
                discCatchTrigger.enabled = false;
                windupKeySpinner.Stop();
                animator.SetBool("HasPower", false);
                animator.SetBool("IsMoving", false);

                if (state == PlayerState.GameCompleted)
                    animator.SetTrigger("PowerDown");
                break;

            case PlayerState.PoweringOn:
                StartCoroutine(PoweringOnRoutine());
                break;

            case PlayerState.PoweredOn:
                DisableInputs = false;
                animator.SetBool("HasPower", true);
                break;

            case PlayerState.Throwing:
                StartCoroutine(ThrowRoutine());
                break;

            case PlayerState.Recalling:
                StartCoroutine(RecallRoutine());
                break;

            case PlayerState.LevelCompleted:
                DisableInputs = true;
                break;

            case PlayerState.GameRewinded:
                CurrentPower = 0f;
                var start = LevelController.instance.PlayerStartingPoint;
                start.y = transform.position.y;

                transform.position = start;
                ChangeState(PlayerState.PoweredOff);
                break;
        }        
    }

    IEnumerator PoweringOnRoutine()
    {
        AudioManager.instance.Play2DSound(powerOnClipInfo);
        animator.SetBool("HasPower", true);
        yield return new WaitForSeconds(poweringOnDelay);
        ChangeState(PlayerState.PoweredOn);
    }

    IEnumerator ThrowRoutine()
    {
        // Look at mouse direction
        var target = LookAtTarget.position;
        target.y = transform.position.y;
        transform.LookAt(target);
        yield return new WaitForEndOfFrame();

        // Play Animation
        AudioManager.instance.Play2DSound(throwClipInfo);
        animator.SetTrigger("Throw");
        yield return new WaitForSeconds(launchDiscDelay);

        // Throw Disc
        if (CurrentPower > 0f)
        {
            Disc.ChangeState(DiscState.Fired);
            yield return new WaitForSeconds(throwingDiscDelay);
        }

        // Enable the trigger to catch it
        if (CurrentPower > 0f)
        {
            discCatchTrigger.enabled = true;
            ChangeState(PlayerState.PoweredOn);
        }   
    }

    IEnumerator RecallRoutine()
    {
        discCatchTrigger.enabled = false;

        // Stops the disc from moving
        Disc.ChangeState(DiscState.Recalled);
        yield return new WaitForEndOfFrame();

        // Looks at the disc
        var target = Disc.transform.position - transform.position;
        target.y = transform.position.y;
        transform.LookAt(target);
        yield return new WaitForEndOfFrame();

        // Play animations
        animator.SetTrigger("Recall");
        yield return new WaitForEndOfFrame();

        if(IsPoweredOn)
            AudioManager.instance.Play2DSound(recallClipInfo);

        // Wait for the disc to arrive
        while (IsPoweredOn && Disc.State != DiscState.Attached)
            yield return new WaitForEndOfFrame();

        if (IsPoweredOn)
            TriggerCatchingRoutine();
    }

    public void TriggerCatchingRoutine(bool lookAtDisc = false)
    {
        StartCoroutine(CatchingRoutine(lookAtDisc));
    }

    IEnumerator CatchingRoutine(bool lookAtDisc)
    {
        discCatchTrigger.enabled = false;

        if (lookAtDisc)
        {
            // Face disc direction
            var target = Disc.transform.position - transform.position;
            target.y = transform.position.y;
            transform.LookAt(target);
            yield return new WaitForEndOfFrame();
        }
       
        // Wait for the hand to be extended before ensuring the disc is attached
        animator.SetTrigger("Catch");        
        yield return new WaitForEndOfFrame();

        // Ensures the disc attached
        if (Disc.State != DiscState.Attached)
        {
            Disc.ChangeState(DiscState.Attached);
            yield return new WaitForEndOfFrame();
        }

        // Still has power so wait for the catch animation to complete
        if (IsPoweredOn)
        {
            AudioManager.instance.Play2DSound(catchClipInfo);
            yield return new WaitForSeconds(cathingDelay);
        }

        if (IsPoweredOn)
            ChangeState(PlayerState.PoweredOn);
    }

    public IEnumerator LevelIntroRoutine(Vector3 start, Vector3 end)
    {
        // Ensure we don't walk into the ground or sky
        start.y = transform.position.y;
        end.y = transform.position.y;

        // Keep the moving animation
        animator.SetBool("HasPower", true);
        animator.SetBool("IsMoving", true);

        // Always have at least 30% of power left
        if (CurrentPower < .30f)
            CurrentPower = .30f;

        // Warps to level start        
        rigidBody.velocity = Vector3.zero;
        rigidBody.detectCollisions = false;
        transform.position = start;
        yield return new WaitForEndOfFrame();

        // Walks to beyond the door
        while (Vector3.Distance(transform.position, end) > 0.1f)
        {
            transform.position = Vector3.MoveTowards(transform.position, end, moveSpeed * Time.deltaTime);
            yield return new WaitForEndOfFrame();
        }

        transform.position = end;
        rigidBody.detectCollisions = true;
        yield return new WaitForEndOfFrame();

        animator.SetBool("IsMoving", false);
        ChangeState(PlayerState.PoweredOn);
    }
}
