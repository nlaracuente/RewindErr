using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class MenuController : Singleton<MenuController>
{
    [SerializeField]
    GameObject mainMenu;

    [SerializeField]
    GameObject pauseMenu;

    [SerializeField]
    GameObject creditsMenu;

    [SerializeField]
    Slider musicVolumeSlider;

    [SerializeField]
    Slider sfxVolumeSlider;

    public bool IsGamePaused { get { return pauseMenu.activeSelf; } }

    void Start()
    {
        OpenMainMenu();
    }

    void Update() 
    {
        if (LevelController.instance.IsCompleted || mainMenu.activeSelf || creditsMenu.activeSelf)
            return;

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (pauseMenu.activeSelf)
                ResumeGame();
            else
                OpenPauseMenu();
        }
    }

    /// <summary>
    ///  We need one frame to happen to destroy the sounds before turning 
    ///  Changing the scales
    /// </summary>
    /// <returns></returns>
    IEnumerator OpenPauseMenuRoutine()
    {
        OpenPauseMenu();
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();
        Time.timeScale = 0f;
    }

    public void CloseAllMenus()
    {
        Time.timeScale = 1f;
        mainMenu.SetActive(false);
        pauseMenu.SetActive(false);
        creditsMenu.SetActive(false);
    }

    public void OpenMainMenu()
    {
        Time.timeScale = 0f;
        mainMenu.SetActive(true);
        pauseMenu.SetActive(false);
        creditsMenu.SetActive(false);
    }

    public void CloseMainMenu()
    {
        Time.timeScale = 1f;
        mainMenu.SetActive(false);
    }

    public void OpenPauseMenu()
    {
        Time.timeScale = 0f;
        musicVolumeSlider.value = AudioManager.instance.MusicVolume;
        sfxVolumeSlider.value = AudioManager.instance.SoundFxVolume;

        mainMenu.SetActive(false);
        pauseMenu.SetActive(true);
        creditsMenu.SetActive(false);

        AudioManager.instance.PauseSounds();
    }

    public void ResumeGame()
    {
        pauseMenu.SetActive(false);
        Time.timeScale = 1f;
        AudioManager.instance.ResumeSounds();
    }

    public void OpenCreditsMenu()
    {
        mainMenu.SetActive(false);
        pauseMenu.SetActive(false);
        creditsMenu.SetActive(true);
        AudioManager.instance.StopAllSounds();
    }

    public void PlayGame()
    {
        Time.timeScale = 1f;
        mainMenu.SetActive(false);
        Player.instance.ChangeState(PlayerState.PoweredOff);
    }

    public void RewindGame()
    {
        Time.timeScale = 1f;
        mainMenu.SetActive(false);
        pauseMenu.SetActive(false);
        creditsMenu.SetActive(false);
        LevelController.instance.RewindGame();
    }


    public void MusicVolume(float value)
    {
        AudioManager.instance.MusicVolume = value;
    }

    public void SoundFXVolume(float value)
    {
        AudioManager.instance.SoundFxVolume = value;
    }

    public void ExitGame()
    {
        LevelController.instance.Quit();
    }

    void ToggleMenu(GameObject menu)
    {
        menu.SetActive(!menu.activeSelf);
    }
}
