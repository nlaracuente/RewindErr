using UnityEngine;

public class FollowMouse : MonoBehaviour
{
    [SerializeField, Tooltip("LayerMask to detect mouse collisions")]
    LayerMask collisionLayer;

    [SerializeField]
    float mouseRayLength = 1000f;

    // Update is called once per frame
    void Update()
    {
        var ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, mouseRayLength, collisionLayer))
        {
            var hitPoint = hit.point;
            hitPoint.y = 0f;
            transform.position = hitPoint;
        }
    }
}
