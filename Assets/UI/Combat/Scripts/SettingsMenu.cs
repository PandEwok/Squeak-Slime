using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI; // REQUIRED: For managing the volume slider component

public class SettingsMenu : MonoBehaviour
{
    public GameObject optionsPanelOverlay;

    [Header("Audio UI Connection")]
    public Slider musicSlider; // Replaced AudioSource with the actual UI Slider reference

    void Start()
    {
        // Automatically sync the slider with whatever the current volume is set to
        if (musicSlider != null)
        {
            musicSlider.minValue = 0f;
            musicSlider.maxValue = 1f;

            if (AudioManager.Instance != null)
            {
                // Assuming your AudioManager has a way to read current volume, or we fetch it.
                // If it doesn't, we fall back safely to 0.5f or whatever value matches your default.
                musicSlider.value = 0.5f;
            }
            else
            {
                musicSlider.value = 0.5f;
            }

            // Hook up the code listener to register movements in real-time
            musicSlider.onValueChanged.AddListener(ChangeVolume);
        }
    }

    public void QuitGame()
    {
        Debug.Log("Quit button was clicked! Exiting game...");
        SceneManager.LoadScene("MainMenu");
    }

    public void EndRun()
    {
        Debug.Log("End Run button was clicked! Ending run...");
        EndgameUIScript.Instance.GameOver();
        gameObject.SetActive(false);
    }

    public void OpenOptions()
    {
        
        if (optionsPanelOverlay != null)
        {
            bool isCurrentlyActive = optionsPanelOverlay.activeSelf;
            optionsPanelOverlay.SetActive(!isCurrentlyActive);
            if(isCurrentlyActive)
            {
                if (Player.Instance.uiManager.actionMenu != null)
                {
                    Player.Instance.uiManager.actionMenu.SetActive(true);
                }
            }
            else
            {
                if (Player.Instance.uiManager.actionMenu != null)
                {
                    Player.Instance.uiManager.actionMenu.SetActive(false);
                }
            }
        }
    }

    public void ChangeVolume(float volume)
    {
        // FIX: Route the volume change directly to the new central system!
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetMusicVolumeMaster(volume);
        }
        else
        {
            Debug.LogWarning("[Settings Menu] AudioManager Instance not found in this scene!");
        }
    }
}