using UnityEngine;

public class PowerStation : MonoBehaviour
{
    [SerializeField]
    bool isPoweredOn = false;
    public bool IsPoweredOn { get { return isPoweredOn; } }

    [SerializeField]
    Transform discMountingPoint;
    public Transform DiscMountingPoint { get { return discMountingPoint; } }

    private void OnTriggerEnter(Collider other)
    {
        var disc = other.gameObject.GetComponent<Disc>();
        if (disc != null && disc.IsCharged)
        {
            disc.ChangeState(DiscState.Connected);
            isPoweredOn = true;
        }
    }
}
