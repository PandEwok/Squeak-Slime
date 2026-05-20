using UnityEngine;
using UnityEngine.SceneManagement;

public class SettingsMenu : MonoBehaviour
{
    public GameObject optionsPanelOverlay;
    public AudioSource musicSource;

    void Start()
    {
    }
    public void QuitGame()
    {
        Debug.Log("Quit button was clicked! Exiting game...");
        SceneManager.LoadScene("MainMenu");
    }

    public void EndRun()
    {
        Debug.Log("End Run button was clicked! Ending run...");
    }

    public void OpenOptions()
    {
        if (optionsPanelOverlay != null)
        {
            // ! means "the opposite of". If it's active, make it inactive. If it's inactive, make it active.
            bool isCurrentlyActive = optionsPanelOverlay.activeSelf;
            optionsPanelOverlay.SetActive(!isCurrentlyActive);
        }
    }

    public void ChangeVolume(float volume)
    {
        if (musicSource != null)
        {
            musicSource.volume = volume;
        }
    }
}