using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.UI; // Required to control the UI Image component!

public class SkillTreeManager : MonoBehaviour
{
    [Header("Currency Setup")]
    [Tooltip("Drag the Tooth ScriptableObject (e.g., Ordinary Tooth) required for this specific panel here.")]
    public Tooth panelTooth;
    public TextMeshProUGUI teethCounterText;

    [Header("UI Icon Setup")]
    [Tooltip("Drag the empty UI Image Game Object next to your text here!")]
    public Image toothIconVisual; // The game object slot for the tooth image

    [Header("Tree Gate Settings")]
    public int totalPassivesBought = 0;
    public TextMeshProUGUI treeProgressText;
    public GameObject activeSkillButton;

    [Header("Global Tooltip UI")]
    public GameObject tooltipPanel;
    public TextMeshProUGUI tooltipTitleText;
    public TextMeshProUGUI tooltipDescText;
    public int maxPurshase = 5;
    private Color originalTeethColor;
    private Coroutine errorFlashCoroutine;

    private void Start()
    {
        if (teethCounterText != null)
        {
            originalTeethColor = teethCounterText.color;
        }

        // Automatically apply the ScriptableObject's graphic data to the UI slot
        InitializeToothIcon();

        UpdateUI();
        HideTooltip();

        if (activeSkillButton != null)
        {
            activeSkillButton.GetComponent<UnityEngine.UI.Button>().interactable = false;
        }
    }

    // Copies the graphic variables directly out of your custom Tooth asset
    private void InitializeToothIcon()
    {
        if (toothIconVisual != null && panelTooth != null)
        {
            // Matched perfectly with your friend's Tooth.cs variables!
            toothIconVisual.sprite = panelTooth.itemIcon;
            toothIconVisual.color = panelTooth.defaultColor;
        }
    }

    public int GetCurrentTeeth()
    {
        if (Player.Instance != null && Player.Instance.inventory != null && panelTooth != null)
        {
            if (Player.Instance.inventory.teethPossessed.ContainsKey(panelTooth))
            {
                return Player.Instance.inventory.teethPossessed[panelTooth];
            }
        }
        return 0;
    }

    public bool CanAfford(int amountRequired)
    {
        return GetCurrentTeeth() >= amountRequired;
    }

    public void SpendTeeth(int amount)
    {
        if (Player.Instance != null && Player.Instance.inventory != null && panelTooth != null)
        {
            Player.Instance.inventory.RemoveTooth(panelTooth, amount);
        }
    }

    public void UpdateUI()
    {
        if (teethCounterText != null) teethCounterText.text = "Teeth: " + GetCurrentTeeth();

        if (treeProgressText != null)
        {
            int displayedProgress = Mathf.Min(totalPassivesBought, maxPurshase);
            treeProgressText.text = displayedProgress + " / " + maxPurshase;
        }

        if (totalPassivesBought >= maxPurshase && activeSkillButton != null)
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
    // ERROR FEEDBACK SYSTEM
    // ==========================================
    public void TriggerBrokeError(string fallbackTitle, string fallbackDesc)
    {
        if (errorFlashCoroutine != null) StopCoroutine(errorFlashCoroutine);
        errorFlashCoroutine = StartCoroutine(ErrorFlashRoutine(fallbackTitle, fallbackDesc));
    }

    private IEnumerator ErrorFlashRoutine(string fallbackTitle, string fallbackDesc)
    {
        ShowTooltip("Cannot Purchase", "Not enough teeth!");
        if (teethCounterText != null) teethCounterText.color = Color.red;

        yield return new WaitForSeconds(1.5f);

        if (teethCounterText != null) teethCounterText.color = originalTeethColor;

        if (tooltipPanel.activeSelf)
        {
            ShowTooltip(fallbackTitle, fallbackDesc);
        }
    }
}