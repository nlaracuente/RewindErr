using UnityEngine;

public class Spinner : MonoBehaviour
{
    [SerializeField, Tooltip("Direction and speed to rotate at")]
    Vector3 axis = Vector3.zero;
    public Vector3 Axis 
    { 
        set { axis = value; }
    }

    Vector3 previousAxis = Vector3.zero;

    private void Awake()
    {
        previousAxis = axis;
    }

    // Update is called once per frame
    void Update()
    {        
        transform.Rotate(axis * Time.deltaTime);
    }

    public void Spin(bool spin)
    {
        if (spin)
            Spin();
        else
            Stop();
    }

    public void Stop()
    {
        previousAxis = axis;
        axis = Vector3.zero;
    }

    public void Spin()
    {
        axis = previousAxis;
    }
}
