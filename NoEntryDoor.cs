using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoEntryDoor : MonoBehaviour
{
    [SerializeField]
    Animator animator;

    [SerializeField]
    AudioClipInfo doorOffClipInfo;

    private void Awake()
    {
        if (animator == null)
            animator = GetComponent<Animator>();
    }

    private void Start()
    {
        animator.SetBool("On", false);
    }

    public void TurnOn()
    {
        AudioManager.instance.Play2DSound(doorOffClipInfo);
        animator.SetBool("On", true);
        animator.SetTrigger("TurnOn");
    }
}