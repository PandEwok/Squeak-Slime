using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class SearchEvent : MonoBehaviour
{
    [Header("UI & Animation References")]
    public TextMeshProUGUI dialogueText;
    public Animator exitButtonAnimator;
    public string animationTriggerName = "ShowButton";

    [Header("Event Settings")]
    public float typeSpeed = 0.04f;
    [Range(0f, 100f)] public float nothingChance = 35f;
    [Range(0f, 100f)] public float rareTwoItemChance = 15f; // 15% chance to get 2 items instead of 1

    [Header("Modular Loot Pool")]
    public List<ItemData> possibleSearchDrops = new List<ItemData>();
    public int maxDropQuantity = 2; // Searching bushes usually yields smaller quantities 

    [Header("Dialogue Templates")]
    [TextArea(2, 4)] public string introText = "You see a place that could be worth searching.";
    [TextArea(2, 4)] public string failText = "You search around... and you are unable to find anything of value.";
    [TextArea(2, 4)] public string successHeader = "You search around... and you found:";

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
        if (isTyping)
        {
            if (typewriterCoroutine != null) StopCoroutine(typewriterCoroutine);
            dialogueText.text = lineToPrint;
            isTyping = false;

            if (currentStep == 2)
            {
                EndSearchEvent();
            }
            return;
        }

        if (currentStep == 1)
        {
            currentStep = 2;
            DetermineOutcome();
            typewriterCoroutine = StartCoroutine(TypeTextRoutine());
        }
    }

    private void DetermineOutcome()
    {
        // 1. Roll for finding absolutely nothing (35% chance)
        float outcomeRoll = Random.Range(0f, 100f);

        if (outcomeRoll <= nothingChance || possibleSearchDrops.Count == 0)
        {
            lineToPrint = failText;
        }
        else
        {
            // SUCCESS: We found something! Now determine how many items (1 or 2)
            string lootReport = successHeader;

            int itemsToDropCount = 1; // Default to 1 item
            float rarityRoll = Random.Range(0f, 100f);

            // 2. Check if the player hits the "very rare" double drop tier
            if (rarityRoll <= rareTwoItemChance)
            {
                itemsToDropCount = 2;
            }

            // Loop through and select our random items
            for (int i = 0; i < itemsToDropCount; i++)
            {
                ItemData rolledItem = possibleSearchDrops[Random.Range(0, possibleSearchDrops.Count)];
                int quantity = Random.Range(1, maxDropQuantity + 1);

                lootReport += $"\n- {rolledItem.itemName} x{quantity}";

                // HOOK: Pass to inventory system here
                // PlayerInventory.Instance.AddItem(rolledItem, quantity);
            }

            lineToPrint = lootReport;
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

        if (currentStep == 2)
        {
            EndSearchEvent();
        }
    }

    private void EndSearchEvent()
    {
        if (exitButtonAnimator != null)
        {
            exitButtonAnimator.SetTrigger(animationTriggerName);
        }
    }

    public void ExitSearchSceneButton()
    {
        SceneManager.LoadScene("SampleScene 2");
    }
}