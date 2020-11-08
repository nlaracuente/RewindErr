using UnityEngine;

public class ChargeStation : MonoBehaviour, IButtonInteractible
{
    [SerializeField]
    Vector3 spinDirection = new Vector3(0f, 400f, 0f);

    [SerializeField]
    bool isPoweredOn = false;
    public bool IsPoweredOn 
    { 
        get { return isPoweredOn; }
        set
        {
            isPoweredOn = value;
            discTrigger.enabled = isPoweredOn;            
            spinner.Spin(value);
        }
    }

    [SerializeField]
    Spinner spinner;

    [SerializeField]
    Collider discTrigger;

    [SerializeField]
    Transform discMountingPoint;
    public Transform DiscMountingPoint { get { return discMountingPoint; } }

    private void Start()
    {
        discTrigger.isTrigger = true;
        spinner.Axis = spinDirection;
        IsPoweredOn = isPoweredOn;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!IsPoweredOn)
            return;

        var disc = other.gameObject.GetComponent<Disc>();
        if (disc != null)
            disc.ChangeState(DiscState.Charging);
    }

    public void SetState(bool isOn)
    {
        IsPoweredOn = isOn;
    }
}
