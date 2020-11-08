using UnityEngine;

public class LevelController : Singleton<LevelController>
{
    public bool IsCompleted { get; private set; }

    [SerializeField]
    bool batteryChargedOnBreak = false;
    public bool BatteryChargedOnBreak { get { return batteryChargedOnBreak; } }

    [SerializeField, Tooltip("Where to spawn the player when the room loads")]
    Transform playerStartXForm;
    public Vector3 PlayerStartingPoint { get { return playerStartXForm.position; } }

    [SerializeField, Tooltip("Up to where to 'walk' the player when room loads")]
    Transform playerEndXForm;
    public Vector3 PlayerEndingPoint { get { return playerEndXForm.position; } }

    public void ReloadScene()
    {
        GameManager.instance.ReloadScene();
    }

    public void LevelCompleted()
    {
        IsCompleted = true;
        GameManager.instance.LevelCompleted();
    }

    public void RewindGame()
    {
        GameManager.instance.RewindGame();
    }

    public void Quit()
    {
        GameManager.instance.Quit();
    }
}
