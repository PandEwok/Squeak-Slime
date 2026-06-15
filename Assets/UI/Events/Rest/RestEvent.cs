using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class RestEvent : MonoBehaviour
{
    [Header("UI & Animation")]
    public TextMeshProUGUI dialogueText;
    public Animator buttonAnimator;
    public string animationTriggerName = "ShowButton";

    [Header("Settings")]
    public float typeSpeed = 0.04f;
    [TextArea(2, 4)]
    public string restTextTemplate = "The skies are clear and the night is peaceful. You rest and recover {0} HP and {1} SP.";

    [Header("Player Cap Fallbacks")]
    [Tooltip("Set the absolute maximum health and SP your player can have so calculations scale correctly.")]
    public int maxPlayerHealthCap = 100;
    public int maxPlayerSPCap = 100;

    [Header("Modular Biome Rest Settings")]
    [Tooltip("Configure unique percentage healing ranges for each biome here!")]
    public List<BiomeRestSetting> biomeRestSettings = new List<BiomeRestSetting>();

    [Tooltip("Fallback settings if the player's current biome isn't explicitly configured above.")]
    public BiomeRestSetting defaultFallbackRest;

    private int hpRestored;
    private int spRestored;
    private string finalMessage;

    private Coroutine typewriterCoroutine;
    private bool isTyping = false;
    private bool eventCompleted = false;

    private void Start()
    {
        CalculateAndApplyRestoration();

        finalMessage = string.Format(restTextTemplate, hpRestored, spRestored);
        typewriterCoroutine = StartCoroutine(TypeTextRoutine());
    }

    private void Update()
    {
        if (Pointer.current != null && Pointer.current.press.wasPressedThisFrame)
        {
            HandleInteraction();
        }
    }

    private void CalculateAndApplyRestoration()
    {
        // 1. Determine current biome from Player
        int currentBiome = 1;
        if (Player.Instance != null)
        {
            currentBiome = ((int)Player.Instance.currentBiome);
        }

        // 2. Fetch the corresponding configuration
        BiomeRestSetting activeConfig = GetConfigForBiome(currentBiome);

        // 3. Roll randomized percentage scales (e.g. 15 to 25)
        float rolledHPPercent = Random.Range(activeConfig.minHPPercent, activeConfig.maxHPPercent + 1) / 100f;
        float rolledSPPercent = Random.Range(activeConfig.minSPPercent, activeConfig.maxSPPercent + 1) / 100f;

        // 4. Calculate flat values based on the maximum caps
        hpRestored = Mathf.RoundToInt(maxPlayerHealthCap * rolledHPPercent);
        spRestored = Mathf.RoundToInt(maxPlayerSPCap * rolledSPPercent);

        // 5. Apply recovery parameters to the live player instance safely
        if (Player.Instance != null && Player.Instance.stats != null)
        {
            // Recover health and clamp using stats.health
            Player.Instance.stats.health = Mathf.Min(maxPlayerHealthCap, Player.Instance.stats.health + hpRestored);

            // Recover SP and clamp using stats.SP
            Player.Instance.stats.SP = Mathf.Min(maxPlayerSPCap, Player.Instance.stats.SP + spRestored);
        }
    }

    private BiomeRestSetting GetConfigForBiome(int biomeId)
    {
        foreach (BiomeRestSetting setting in biomeRestSettings)
        {
            if (setting.targetBiome == biomeId) return setting;
        }
        return defaultFallbackRest;
    }

    private IEnumerator TypeTextRoutine()
    {
        isTyping = true;
        dialogueText.text = "";

        foreach (char letter in finalMessage.ToCharArray())
        {
            dialogueText.text += letter;
            yield return new WaitForSeconds(typeSpeed);
        }

        EndRestEvent();
    }

    private void HandleInteraction()
    {
        if (isTyping && !eventCompleted)
        {
            if (typewriterCoroutine != null) StopCoroutine(typewriterCoroutine);
            dialogueText.text = finalMessage;
            EndRestEvent();
        }
    }

    private void EndRestEvent()
    {
        isTyping = false;
        eventCompleted = true;

        if (buttonAnimator != null)
        {
            buttonAnimator.SetTrigger(animationTriggerName);
        }
    }

    public void ExitRestSceneButton()
    {
        SceneManager.LoadScene("SampleScene 2");
    }
}

// Custom data container for modular inspector percentage settings
[System.Serializable]
public class BiomeRestSetting
{
    [Tooltip("Which biome number does this profile apply to?")]
    public int targetBiome = 1;

    [Header("HP Recovery Range (Percentages, e.g., 15 = 15%)")]
    [Range(0, 100)] public int minHPPercent = 15;
    [Range(0, 100)] public int maxHPPercent = 25;

    [Header("SP Recovery Range (Percentages, e.g., 10 = 10%)")]
    [Range(0, 100)] public int minSPPercent = 10;
    [Range(0, 100)] public int maxSPPercent = 20;
}