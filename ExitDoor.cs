using UnityEngine;


/// <summary>
/// A bit confusion because of how the animations was setup but in the interest
/// of time it was kept that way. If the charge station is ON then this door is OFF
/// </summary>
[RequireComponent(typeof(Animator))]
public class ExitDoor : MonoBehaviour
{
    [SerializeField]
    Animator animator;

    [SerializeField]
    PowerStation powerStation;

    [SerializeField]
    AudioClipInfo doorOffClipInfo;

    bool isPoweredOn = false;

    private void Awake()
    {
        if (animator == null)
            animator = GetComponent<Animator>();
    }

    private void Start()
    {
        isPoweredOn = powerStation.IsPoweredOn;
        animator.SetBool("On", !isPoweredOn);
    }

    private void Update()
    {
        if(isPoweredOn != powerStation.IsPoweredOn)
        {
            isPoweredOn = powerStation.IsPoweredOn;
            if (!isPoweredOn)
                animator.SetTrigger("TurnOn");
            else
            {
                AudioManager.instance.Play2DSound(doorOffClipInfo);
                animator.SetTrigger("TurnOff");
            }
        }

        animator.SetBool("On", !isPoweredOn);
    }
}
