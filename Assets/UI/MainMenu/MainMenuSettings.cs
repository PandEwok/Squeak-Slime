using UnityEngine;
using UnityEngine.UI;

public class MainMenuSettings : MonoBehaviour
{
    [Header("Audio Setup")]
    public Slider musicSlider;
    // Notice: We deleted the AudioMixer and parameter strings completely!

    private Animator animator;
    private bool isOpen = false;

    private void Awake()
    {
        // Automatically grab the animator component on this panel
        animator = GetComponent<Animator>();
    }

    private void Start()
    {
        if (musicSlider != null)
        {
            // Clean 0 to 1 scaling for the new AudioManager
            musicSlider.minValue = 0f;
            musicSlider.maxValue = 1f;

            // Start the slider at 50% volume by default
            musicSlider.value = 0.5f;

            musicSlider.onValueChanged.AddListener(SetMusicVolume);
        }
    }

    /// <summary>
    /// Flips the state and tells the animator to move the panel
    /// </summary>
    public void ToggleSettingsMenu()
    {
        isOpen = !isOpen;

        if (animator != null)
        {
            animator.SetBool("IsOpen", isOpen);
        }
    }

    public void SetMusicVolume(float sliderValue)
    {
        // Tell your friend's global AudioManager to change the volume directly!
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetMusicVolumeMaster(sliderValue);
        }
        else
        {
            Debug.LogWarning("AudioManager Instance not found! Is it in the scene?");
        }
    }
}