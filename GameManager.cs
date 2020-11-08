using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
public class GameManager : Singleton<GameManager>
{
    IEnumerator sceneLoadingRoutine;

    public int CurrentLevel { get { return SceneManager.GetActiveScene().buildIndex; } }

    public void ReloadScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void RewindGame()
    {
        if (sceneLoadingRoutine != null)
            return;

        sceneLoadingRoutine = LoadFirstScene();
        StartCoroutine(sceneLoadingRoutine);
    }

    public void Quit()
    {
        Application.Quit();
    }

    public void LevelCompleted()
    {
        if (sceneLoadingRoutine != null)
            return;

        Player.instance.ChangeState(PlayerState.LevelCompleted);

        var activeScene = SceneManager.GetActiveScene();
        var nextScene = activeScene.buildIndex + 1;

        if (nextScene >= SceneManager.sceneCountInBuildSettings)
            GameCompleted();
        else
        {
            sceneLoadingRoutine = LoadNextSceneAysncRoutine(nextScene);
            StartCoroutine(sceneLoadingRoutine);
        }
    }

    void GameCompleted()
    {
        MenuController.instance.OpenCreditsMenu();
        Player.instance.ChangeState(PlayerState.GameCompleted);
    }

    IEnumerator LoadFirstScene()
    {
        AudioManager.instance.StopAllSounds();
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(0);

        // Wait until the asynchronous scene fully loads
        while (!asyncLoad.isDone)
            yield return null;

        Player.instance.ChangeState(PlayerState.GameRewinded);
        sceneLoadingRoutine = null;
    }

    IEnumerator LoadNextSceneAysncRoutine(int buildIndex, LoadSceneMode loadSceneMode = LoadSceneMode.Single)
    {
        AudioManager.instance.StopAllSounds();
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(buildIndex, loadSceneMode);

        // Wait until the asynchronous scene fully loads
        while (!asyncLoad.isDone)
            yield return null;

        yield return StartCoroutine(
            Player.instance.LevelIntroRoutine(
                LevelController.instance.PlayerStartingPoint,
                LevelController.instance.PlayerEndingPoint
            )
        );

        FindObjectOfType<NoEntryDoor>().TurnOn();
        sceneLoadingRoutine = null;
    }
}
