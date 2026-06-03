using UnityEngine;
using TMPro;
using System.Collections; // NEW: Required for Coroutines

public class SkillTreeManager : MonoBehaviour
{
    [Header("Global Currency")]
    public int playerTeethCount = 5;
    public TextMeshProUGUI teethCounterText;

    [Header("Tree Gate Settings")]
    public int totalPassivesBought = 0;
    public TextMeshProUGUI treeProgressText;
    public GameObject activeSkillButton;

    [Header("Global Tooltip UI")]
    public GameObject tooltipPanel;
    public TextMeshProUGUI tooltipTitleText;
    public TextMeshProUGUI tooltipDescText;

    // NEW: Variables to track the error flash state
    private Color originalTeethColor;
    private Coroutine errorFlashCoroutine;

    private void Start()
    {
        // NEW: Remember what color the teeth text was originally so we can revert back to it
        if (teethCounterText != null)
        {
            originalTeethColor = teethCounterText.color;
        }

        UpdateUI();
        HideTooltip();

        if (activeSkillButton != null)
        {
            activeSkillButton.GetComponent<UnityEngine.UI.Button>().interactable = false;
        }
    }

    public void UpdateUI()
    {
        if (teethCounterText != null) teethCounterText.text = "Teeth: " + playerTeethCount;
        if (treeProgressText != null) treeProgressText.text = totalPassivesBought + " / 5";

        if (totalPassivesBought >= 5 && activeSkillButton != null)
        {
            activeSkillButton.GetComponent<UnityEngine.UI.Button>().interactable = true;
        }
    }

    public void ShowTooltip(string title, string description)
    {
        tooltipPanel.SetActive(true);
        tooltipTitleText.text = title;
        tooltipDescText.text = description;
    }

    public void HideTooltip()
    {
        tooltipPanel.SetActive(false);
    }

    // ==========================================
    // NEW: ERROR FEEDBACK SYSTEM
    // ==========================================

    public void TriggerBrokeError(string fallbackTitle, string fallbackDesc)
    {
        // If they spam click, stop the previous timer and start fresh
        if (errorFlashCoroutine != null) StopCoroutine(errorFlashCoroutine);
        errorFlashCoroutine = StartCoroutine(ErrorFlashRoutine(fallbackTitle, fallbackDesc));
    }

    private IEnumerator ErrorFlashRoutine(string fallbackTitle, string fallbackDesc)
    {
        // 1. Show the error on the tooltip and turn the teeth text RED
        ShowTooltip("Cannot Purchase", "Not enough teeth!");
        if (teethCounterText != null) teethCounterText.color = Color.red;

        // 2. Wait for 1.5 seconds
        yield return new WaitForSeconds(1.5f);

        // 3. Revert the teeth text color
        if (teethCounterText != null) teethCounterText.color = originalTeethColor;

        // 4. If the player is STILL hovering over the button, revert the tooltip back to normal text
        if (tooltipPanel.activeSelf)
        {
            ShowTooltip(fallbackTitle, fallbackDesc);
        }
    }
}