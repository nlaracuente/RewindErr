using UnityEngine;

public class SwitchAnimationController : MonoBehaviour
{
    [SerializeField]
    Animator animator;
    public bool IsOn { get; set; }

    private void Start()
    {
        animator = animator != null ? animator : GetComponent<Animator>();
        if (animator == null)
            Debug.LogError($"{name} is missing an animator controller");
    }

    private void LateUpdate()
    {
        animator.SetBool("On", IsOn);
    }
}
