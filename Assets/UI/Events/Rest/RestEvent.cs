using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem; // <-- 1. ADD THIS FOR THE NEW INPUT SYSTEM

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

    private int hpRestored;
    private int spRestored;
    private string finalMessage;

    private Coroutine typewriterCoroutine;
    private bool isTyping = false;
    private bool eventCompleted = false;

    private void Start()
    {
        CalculateRestoration();

        finalMessage = string.Format(restTextTemplate, hpRestored, spRestored);
        typewriterCoroutine = StartCoroutine(TypeTextRoutine());
    }

    private void Update()
    {
        // 2. UPDATED: Uses the New Input System to detect a left-click/tap safely
        if (Pointer.current != null && Pointer.current.press.wasPressedThisFrame)
        {
            HandleInteraction();
        }
    }

    private void CalculateRestoration()
    {
        float totalPercent = 0.40f;

        float[] extraModifiers = { 0.05f, 0.10f, 0.15f };
        totalPercent += extraModifiers[Random.Range(0, extraModifiers.Length)];

        int playerMaxHP = 100;
        int playerMaxSP = 100;

        hpRestored = Mathf.RoundToInt(playerMaxHP * totalPercent);
        spRestored = Mathf.RoundToInt(playerMaxSP * totalPercent);
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