using UnityEngine;

public class LookAtMouse : MonoBehaviour
{
    public bool DisableRotation { get; set; }
    void Update()
    {
        if (DisableRotation)
            return;

        //var mDir = Input.mousePosition - Camera.main.WorldToScreenPoint(transform.position);
        //var angle = Mathf.Atan2(mDir.x, mDir.y) * Mathf.Rad2Deg;
        //transform.rotation = Quaternion.Euler(0f, angle, 0f);

        var mPos = MouseController.instance.MouseWorldPosition();
        var dir = mPos - transform.position;
        dir.y = 0f;
        Quaternion targetRotation = Quaternion.LookRotation(dir);
        transform.rotation = targetRotation;
    }
}
