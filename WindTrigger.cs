using UnityEngine;

public class WindTrigger : MonoBehaviour
{
    [SerializeField]
    LayerMask collisionLayer;

    [SerializeField, Tooltip("The transform to get the forward vector for calculating Force")]
    Transform parentXForm;

    [SerializeField]
    Transform rayStartXForm;

    [SerializeField]
    float pushPower = 2f;
    public Vector3 Force { get { return parentXForm.forward * pushPower; } }

    private void Awake()
    {
        if (parentXForm == null)
            parentXForm = transform.parent != null ? transform.parent : transform;
    }

    private void OnTriggerStay(Collider other)
    {
        //Debug.DrawLine(rayStartXForm.position, other.transform.position, Color.red, 1f);

        // Something is blocking the wind
        if (Physics.Linecast(rayStartXForm.position, other.transform.position, collisionLayer))
            return;

        var torch = other.GetComponent<Torch>();
        if (torch != null)
            torch.IsOn = false;

        var disc = other.GetComponent<Disc>();
        if (disc != null) {
            if(disc.ElementalState == DiscElementalState.Fire)
                disc.ElementalState = DiscElementalState.None;

            disc.WindTrigger = this;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        var disc = other.GetComponent<Disc>();
        if (disc != null)
            disc.WindTrigger = null;
    }
}
