using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class ElevatorGoal : MonoBehaviour
{
    [SerializeField, Tooltip("Where to automatically move the player to before moving")]
    Transform playerTargetXform;

    [SerializeField, Tooltip("How long before triggering reload")]
    float risingAnimationTime = 1f;

    [SerializeField, Tooltip("The animator controller for when the elevator leaves")]
    RuntimeAnimatorController exitAnimatorController;

    Animator animator;


    private void Awake()
    {
        animator = animator != null ? animator : GetComponent<Animator>();
        if (animator == null)
            Debug.LogError($"{name} is missing an animator controller");
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
            StartCoroutine(PlayerEnterRoutine(other.GetComponent<Player>()));
    }

    IEnumerator PlayerEnterRoutine(Player player)
    {
        animator.SetBool("On", true);
        player.ChangeState(PlayerState.LevelCompleted);
        yield return new WaitForEndOfFrame();

        var rb = player.GetComponent<Rigidbody>();

        var targetTime = Time.time + 1f;
        while(Time.time < targetTime)
        {
            rb.MovePosition(Vector3.MoveTowards(player.transform.position, playerTargetXform.position, player.MoveSpeed * Time.deltaTime));
            yield return new WaitForEndOfFrame();
        }

        animator.runtimeAnimatorController = exitAnimatorController;
        yield return new WaitForSeconds(risingAnimationTime);
        LevelController.instance.ReloadScene();
    }
}
