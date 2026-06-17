using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public enum ActiveSkillType
{
    Bite,
    Fracture,
    Fireball,
    Absorption
}

public class ActiveSkillButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("References")]
    public SkillTreeManager treeManager;

    [Header("Skill Information")]
    public string skillName = "Bite";
    [TextArea(3, 5)]
    public string skillDescription = "Unlocks a devastating close-range bite attack.";

    [Header("Skill Type Setup")]
    [Tooltip("Select which active skill boolean this button activates in the player inventory.")]
    public ActiveSkillType skillType;

    private bool isUnlocked = false;

    private void Start()
    {
        CheckCurrentStatus();
    }

    private void OnEnable()
    {
        CheckCurrentStatus();
    }

    // Syncs with the persistent player inventory state safely
    public void CheckCurrentStatus()
    {
        if (Player.Instance == null || Player.Instance.inventory == null) return;

        switch (skillType)
        {
            case ActiveSkillType.Bite:
                isUnlocked = Player.Instance.inventory.hasBite;
                break;
            case ActiveSkillType.Fracture:
                isUnlocked = Player.Instance.inventory.hasFracture;
                break;
            case ActiveSkillType.Fireball:
                isUnlocked = Player.Instance.inventory.hasFireball;
                break;
            case ActiveSkillType.Absorption:
                isUnlocked = Player.Instance.inventory.hasAbsorption;
                break;
        }
    }

    // Hook this up to your button's OnClick() event in Unity!
    public void PurchaseActiveSkill()
    {
        if (Player.Instance == null || Player.Instance.inventory == null || isUnlocked) return;

        // Turn on the exact matching boolean written by your friend
        switch (skillType)
        {
            case ActiveSkillType.Bite:
                Player.Instance.inventory.hasBite = true;
                break;
            case ActiveSkillType.Fracture:
                Player.Instance.inventory.hasFracture = true;
                break;
            case ActiveSkillType.Fireball:
                Player.Instance.inventory.hasFireball = true;
                break;
            case ActiveSkillType.Absorption:
                Player.Instance.inventory.hasAbsorption = true;
                break;
        }

        isUnlocked = true;

        // Refresh the tooltip text live to show its completion status
        if (treeManager != null)
        {
            treeManager.ShowTooltip(skillName, GetDynamicDescription());
        }
    }

    private string GetDynamicDescription()
    {
        string statusText = isUnlocked
            ? "<color=green>(Unlocked & Ready!)</color>"
            : "<color=orange>(Locked - Requires 5 Passives)</color>";

        return $"{skillDescription}\n\nStatus: {statusText}";
    }

    // Tooltip Hover Links
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (treeManager != null)
        {
            treeManager.ShowTooltip(skillName, GetDynamicDescription());
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (treeManager != null)
        {
            treeManager.HideTooltip();
        }
    }
}