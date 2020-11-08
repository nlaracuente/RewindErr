using UnityEngine;

[RequireComponent(typeof(SwitchAnimationController))]
public class ButtonTarget : MonoBehaviour, IButtonInteractible
{
    [SerializeField]
    SwitchAnimationController switchAnimationController;

    private void Awake()
    {
        switchAnimationController = switchAnimationController != null ? switchAnimationController : GetComponent<SwitchAnimationController>();
        if (switchAnimationController == null)
            Debug.LogError($"{name} is missing an SwitchAnimationController");
    }

    public void SetState(bool isOn)
    {
        switchAnimationController.IsOn = isOn;
    }
}
