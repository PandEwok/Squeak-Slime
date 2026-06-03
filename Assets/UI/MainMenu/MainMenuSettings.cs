using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;

public class MainMenuSettings : MonoBehaviour
{
    [Header("Audio Setup")]
    public AudioMixer targetMixer;
    public Slider musicSlider;
    private string musicParameterName = "MusicVol";

    private Animator animator;
    private bool isOpen = false;

    private void Awake()
    {
        // Automatically grab the animator component on this panel
        animator = GetComponent<Animator>();
    }

    private void Start()
    {
        // Keep your awesome working slider math setup!
        if (musicSlider != null)
        {
            musicSlider.minValue = 0.0001f;
            musicSlider.maxValue = 1f;

            if (targetMixer.GetFloat(musicParameterName, out float currentVolume))
            {
                musicSlider.value = Mathf.Pow(10f, currentVolume / 20f);
            }

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
        float decibelVolume = Mathf.Log10(sliderValue) * 20f;
        targetMixer.SetFloat(musicParameterName, decibelVolume);
    }
}