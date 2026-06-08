using UnityEngine;
using TMPro;
using UnityEngine.EventSystems; // Required to detect mouse hovers!

public class SkillNode : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("References")]
    public SkillTreeManager treeManager;
    public TextMeshProUGUI levelText;

    [Header("Skill Information")]
    public string skillName = "Heavy Slam";
    [TextArea(3, 5)] // Gives you a nice big text box in the Inspector
    public string skillDescription = "Increases golem strike damage by 15%.";

    [Header("Upgrades Tracker")]
    public int currentLevel = 0;
    public int maxLevel = 3;

    private void Start()
    {
        UpdateNodeText();
    }

    // This automatically runs when you click the button (Hook up via OnClick)
    public void PurchaseUpgrade()
    {
        // If the skill is already maxed, do nothing
        if (currentLevel >= maxLevel) return;

        // If we have currency, buy it!
        if (treeManager.playerTeethCount > 0)
        {
            treeManager.playerTeethCount--;
            currentLevel++;
            treeManager.totalPassivesBought++;

            UpdateNodeText();
            treeManager.UpdateUI();

            // Refresh tooltip text live
            treeManager.ShowTooltip(skillName, GetDynamicDescription());
        }
        else
        {
            // NEW: We are broke! Trigger the visual error feedback instead.
            treeManager.TriggerBrokeError(skillName, GetDynamicDescription());
        }
    }

    private void UpdateNodeText()
    {
        if (levelText != null)
        {
            levelText.text = currentLevel + " / " + maxLevel;
        }
    }

    private string GetDynamicDescription()
    {
        return skillDescription + "\n\n(Current Level: " + currentLevel + ")";
    }

    // Detect mouse hover entering the button
    public void OnPointerEnter(PointerEventData eventData)
    {
        treeManager.ShowTooltip(skillName, GetDynamicDescription());
    }

    // Detect mouse hover leaving the button
    public void OnPointerExit(PointerEventData eventData)
    {
        treeManager.HideTooltip();
    }
}