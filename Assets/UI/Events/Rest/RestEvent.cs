using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class RestEvent : MonoBehaviour
{
    [Header("Audio Settings")]
    [Tooltip("The exact name of your main menu track in the AudioManager database.")]
    public string musicTrackName = "EventScene";

    [Header("UI & Animation")]
    public TextMeshProUGUI dialogueText;
    public Animator buttonAnimator;
    public string animationTriggerName = "ShowButton";

    [Header("Settings")]
    public float typeSpeed = 0.04f;
    [TextArea(2, 4)]
    public string restTextTemplate = "The skies are clear and the night is peaceful. You rest and recover {0} HP and {1} SP.{2}";


    [Header("Player Cap Fallbacks (Editor Standalone Testing)")]
    [Tooltip("Fallbacks used ONLY if loading this scene directly inside the Unity Editor without a Player instance.")]
    public int fallbackMaxHealth = 100;
    public int fallbackMaxSP = 100;

    private int hpRestored;
    private int spRestored;
    private string finalMessage;

    private Coroutine typewriterCoroutine;
    private bool isTyping = false;
    private bool eventCompleted = false;
    private Vector3 playerDefPos = new Vector3(7777, 0, 0);
    public int nextSceneName = 9;

    private void Start()
    {
        CalculateAndApplyRestoration();

        if (Player.Instance != null)
        {
            Player.Instance.transform.position = playerDefPos;
        }

        if (AudioManager.Instance != null && !string.IsNullOrEmpty(musicTrackName))
        {
            AudioManager.Instance.PlayMusic(musicTrackName);
        }

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
        // 1. Establish current progression values safely
        int currentFloor = (Player.Instance != null) ? Player.Instance.floor : 1;
        int maxHP = (Player.Instance != null && Player.Instance.stats != null) ? Player.Instance.stats.originalHealth : fallbackMaxHealth;
        int maxSP = (Player.Instance != null && Player.Instance.stats != null) ? Player.Instance.stats.originalSP : fallbackMaxSP;

        // DIAGNOSTIC LOG: Let's see what your health actually is BEFORE the rest math happens!
        if (Player.Instance != null && Player.Instance.stats != null)
        {
            Debug.Log($"[REST DIAGNOSTIC] Before Rest Math -> Player Health is: {Player.Instance.stats.health} / {maxHP}");
        }

        // 2. Base mechanics calculations
        int totalPercentBonus = 40;
        bool criticalBonusTriggered = false;

        // 3. Roll for subsequent scaling floor bonus chances
        int luckChanceThreshold = currentFloor * 5;
        int dynamicLuckRoll = Random.Range(1, 101);

        if (dynamicLuckRoll <= luckChanceThreshold)
        {
            criticalBonusTriggered = true;
            int dynamicMaxBonusCap = currentFloor * 5;
            int additionalPercentageBonus = Random.Range(5, dynamicMaxBonusCap + 1);
            totalPercentBonus += additionalPercentageBonus;
        }

        // 4. Convert structural percentage modifiers to final recovery floats
        float finalCalculatedScale = totalPercentBonus / 100f;
        hpRestored = Mathf.RoundToInt(maxHP * finalCalculatedScale);
        spRestored = Mathf.RoundToInt(maxSP * finalCalculatedScale);

        // 5. Apply recovery values to our live Player singleton instances
        if (Player.Instance != null && Player.Instance.stats != null)
        {
            Player.Instance.stats.health = Mathf.Min(maxHP, Player.Instance.stats.health + hpRestored);
            Player.Instance.stats.SP = Mathf.Min(maxSP, Player.Instance.stats.SP + spRestored);

            // DIAGNOSTIC LOG: Let's see what your health became AFTER the rest math
            Debug.Log($"[REST DIAGNOSTIC] After Rest Math -> Player Health is now: {Player.Instance.stats.health}");
        }

        // 6. Build the dynamic string message configuration
        string bonusFlavorText = criticalBonusTriggered ? $" <color=yellow>(Critical Recovery Bonus! Recov. Total: {totalPercentBonus}%)</color>" : "";
        finalMessage = string.Format(restTextTemplate, hpRestored, spRestored, bonusFlavorText);
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

    // FIXED: Increments tracking floor count and handles structural biome adjustments!
    public void ExitRestSceneButton()
    {
        if (Player.Instance != null)
        {
            // FIX: Clear the data tracking token out so it is fresh for the next encounter
            Player.Instance.pendingEventID = "";
            // Advance progression
            Player.Instance.floor++;

            // Checks against the player's own maxFloor setting!
            if (Player.Instance.floor > Player.Instance.maxFloor)
            {
                // Reset floor back to 1
                Player.Instance.floor = 1;

                // Advance biome enum mapping
                int nextBiomeIndex = (int)Player.Instance.currentBiome + 1;

                if (System.Enum.IsDefined(typeof(Player.BiomeType), nextBiomeIndex))
                {
                    Player.Instance.currentBiome = (Player.BiomeType)nextBiomeIndex;
                    Debug.Log($"[Progression] Biome shifted successfully to: {Player.Instance.currentBiome}");
                }
                else
                {
                    Debug.LogWarning("[Progression] Max Biome exceeded!");
                }
            }
        }

        Player.Instance.SwitchSceneInCaseOfVictory();
    }
}