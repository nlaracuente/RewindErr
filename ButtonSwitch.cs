using UnityEngine;
using System.Collections.Generic;

public enum ButtonType
{
    SinglePress,
    Toggle,
}

[RequireComponent(typeof(Animator), typeof(SwitchAnimationController))]
public class ButtonSwitch : MonoBehaviour
{
    [SerializeField, Tooltip("Initial/Current button state")]
    bool isOn = false;

    [SerializeField, Tooltip("All the objects affected by this switch")]
    GameObject[] switchTargets;
    List<IButtonInteractible> targets;

    [SerializeField]
    ButtonType type;

    [SerializeField]
    SwitchAnimationController switchAnimation;

    [SerializeField]
    AudioClipInfo turnOnClipInfo;

    [SerializeField]
    AudioClipInfo turnOffClipInfo;

    bool hasChanged = false;    

    private void Start()
    {
        switchAnimation = switchAnimation != null ? switchAnimation : GetComponent<SwitchAnimationController>();
        if (switchAnimation == null)
            Debug.LogError($"{name} is missing an SwitchAnimationController");

        targets = new List<IButtonInteractible>();
        if(switchTargets != null)
        {
            foreach (var target in switchTargets)
            {
                var i = target.GetComponent<IButtonInteractible>();
                if(i != null)
                    targets.Add(i);
            }   
        }

        // Set default state
        switchAnimation.IsOn = isOn;
        targets.ForEach(t => t.SetState(isOn));
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Disc"))
            ToggleState();
    }

    void ToggleState()
    {
        if (type == ButtonType.SinglePress && hasChanged)
            return;

        hasChanged = true;

        isOn = !isOn;

        if (isOn && turnOnClipInfo != null)
            AudioManager.instance.Play2DSound(turnOnClipInfo);
        else if (!isOn && turnOffClipInfo != null)
            AudioManager.instance.Play2DSound(turnOffClipInfo);

        switchAnimation.IsOn = isOn;
        targets.ForEach(t => t.SetState(isOn));
    }
}
