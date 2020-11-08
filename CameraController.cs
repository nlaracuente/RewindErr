using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : Singleton<CameraController>
{
    [SerializeField]
    Camera main;
    public Camera MainCamera { get { return main; } }

    [SerializeField]
    Camera mouseCamera;
    public Camera MouseCamera { get { return mouseCamera; } }
}
