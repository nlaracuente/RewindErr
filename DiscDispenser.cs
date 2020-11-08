using UnityEngine;

public class DiscDispenser : MonoBehaviour
{
    [SerializeField]
    Transform batterySpawnPoint;
    public Transform DiscMountingPoint { get { return batterySpawnPoint; } }
}
