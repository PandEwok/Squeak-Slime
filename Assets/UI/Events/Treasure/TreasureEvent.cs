using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class TreasureEvent : MonoBehaviour
{
    [Header("UI & Animation References")]
    public TextMeshProUGUI dialogueText;
    public Animator exitButtonAnimator;
    public string animationTriggerName = "ShowButton";

    [Header("Event Settings")]
    public float typeSpeed = 0.04f;
    [Range(0f, 100f)] public float successChance = 35f;

    [Header("Modular Loot Pool")]
    [Tooltip("Drop your scriptable object ItemData files into this list in the inspector!")]
    public List<ItemData> possibleTreasureDrops = new List<ItemData>();
    [Tooltip("The maximum amount of an item that can drop (e.g. if 3, it drops 1 to 3 copies)")]
    public int maxDropQuantity = 3;

    [Header("Dialogue Templates")]
    [TextArea(2, 4)] public string introText = "You enter a room with a treasure chest, lucky! You soon open it...";
    [TextArea(2, 4)] public string trapTextTemplate = "The treasure was a TRAP! You lost {0} HP.";

    [Header("Loot Text Config")]
    [TextArea(2, 4)] public string lootHeader = "The treasure was a bunch of loot! You got:";

    // State Tracking
    private int currentStep = 1;
    private string lineToPrint = "";
    private Coroutine typewriterCoroutine;
    private bool isTyping = false;

    private void Start()
    {
        lineToPrint = introText;
        typewriterCoroutine = StartCoroutine(TypeTextRoutine());
    }

    private void Update()
    {
        if (Pointer.current != null && Pointer.current.press.wasPressedThisFrame)
        {
            HandleInteraction();
        }
    }

    private void HandleInteraction()
    {
        // Case A: The text is currently typing out
        if (isTyping)
        {
            if (typewriterCoroutine != null) StopCoroutine(typewriterCoroutine);
            dialogueText.text = lineToPrint;
            isTyping = false;

            // FIX: If we skip the typing on the final step, we must manually trigger the end event!
            if (currentStep == 2)
            {
                EndTreasureEvent();
            }
            return;
        }

        // Case B: The text is idle, and we need to advance to the outcome
        if (currentStep == 1)
        {
            currentStep = 2;
            DetermineOutcome();
            typewriterCoroutine = StartCoroutine(TypeTextRoutine());
        }
    }

    private void DetermineOutcome()
    {
        float roll = Random.Range(0f, 100f);

        if (roll <= successChance && possibleTreasureDrops.Count > 0)
        {
            string lootReport = lootHeader;
            int itemsToDropCount = Random.Range(2, 4);

            for (int i = 0; i < itemsToDropCount; i++)
            {
                ItemData rolledItem = possibleTreasureDrops[Random.Range(0, possibleTreasureDrops.Count)];
                int quantity = Random.Range(1, maxDropQuantity + 1);

                lootReport += $"\n- {rolledItem.itemName} x{quantity}";
            }

            lineToPrint = lootReport;
        }
        else
        {
            int damageTaken = Random.Range(15, 31);
            lineToPrint = string.Format(trapTextTemplate, damageTaken);
        }
    }

    private IEnumerator TypeTextRoutine()
    {
        isTyping = true;
        dialogueText.text = "";

        foreach (char letter in lineToPrint.ToCharArray())
        {
            dialogueText.text += letter;
            yield return new WaitForSeconds(typeSpeed);
        }

        isTyping = false;

        // If the line finishes naturally on step 2, trigger the end event
        if (currentStep == 2)
        {
            EndTreasureEvent();
        }
    }

    private void EndTreasureEvent()
    {
        if (exitButtonAnimator != null)
        {
            exitButtonAnimator.SetTrigger(animationTriggerName);
        }
    }

    public void ExitTreasureSceneButton()
    {
        SceneManager.LoadScene("SampleScene 2");
    }
}