using UnityEngine;

public class MouseController : Singleton<MouseController>
{
    /// <summary>
    /// The mouse world position relative to the target
    /// </summary>
    /// <param name="target"></param>
    /// <returns></returns>
    //public Vector3 MouseWorldPosition(Transform target)
    //{
    //    var mPos = Input.mousePosition;
    //    mPos.z = Camera.main.WorldToScreenPoint(target.position).z;
    //    return Camera.main.ScreenToWorldPoint(mPos);
    //}

    /// <summary>
    /// Uses the mouse controller as the position
    /// </summary>
    /// <returns></returns>
    public Vector3 MouseWorldPosition()
    {
        var mPos = Input.mousePosition;
        mPos.z = Camera.main.WorldToScreenPoint(transform.position).z;
        return Camera.main.ScreenToWorldPoint(mPos);
    }
}
