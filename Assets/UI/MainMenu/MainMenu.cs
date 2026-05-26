using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public GameObject optionsPanel;
    public AudioSource musicSource;

    // We set up the coordinates for where the menu should go
    private Vector3 offScreenPosition = new Vector3(0, 2000, 0); // 2000 pixels UP
    private Vector3 onScreenPosition = new Vector3(0, 0, 0);     // Dead center

    void Start()
    {
        if (optionsPanel != null)
        {
            // Teleport off-screen on startup!
            optionsPanel.transform.localPosition = offScreenPosition;
        }
    }

    public void StartGame()
    {
        SceneManager.LoadScene("HubScene");
    }

    public void QuitGame()
    {
        Debug.Log("Quit button was clicked! Exiting game...");
        Application.Quit();
    }

    public void OpenOptions()
    {
        // Bring it back to the center of the screen
        optionsPanel.transform.localPosition = onScreenPosition;
    }

    public void CloseOptions()
    {
        // Throw it back off-screen
        optionsPanel.transform.localPosition = offScreenPosition;
    }

    public void ChangeVolume(float volume)
    {
        if (musicSource != null)
        {
            musicSource.volume = volume;
        }
    }
}