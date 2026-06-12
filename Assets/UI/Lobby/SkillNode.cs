using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;

public enum SkillStatType
{
    MeleeDamage,
    CriticalDamage,
    CriticalChance,
    MaxHealth,
    MaxSP,
    BaseDamage,
    BaseDefense,
    DebuffResist,
    RangedDamage
}

public class SkillNode : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("References")]
    public SkillTreeManager treeManager;
    public TextMeshProUGUI levelText;
    public TextMeshProUGUI costText; // Drag your custom "Cost: X" Text component here!

    [Header("Skill Information")]
    public string skillName = "Heavy Slam";
    [TextArea(3, 5)]
    public string skillDescription = "Increases melee damage.";

    [Header("Cost Setup")]
    public int toothCost = 1; // Set custom teeth costs per node in the Inspector!

    [Header("Stat Linking")]
    public SkillStatType statToBoost;
    [Tooltip("How much does the stat increase per level? (e.g., 0.10 for 10% crit chance, or 5 for +5 Melee Damage)")]
    public float boostPerLevel = 1f;

    [Header("Upgrades Tracker")]
    public int currentLevel = 0;
    public int maxLevel = 3;

    private void Start()
    {
        UpdateNodeText();
    }

    public void PurchaseUpgrade()
    {
        if (currentLevel >= maxLevel) return;

        // Asks the manager if the player can afford this specific node's tooth cost
        if (treeManager.CanAfford(toothCost))
        {
            // Deducts the unique cost value
            treeManager.SpendTeeth(toothCost);

            currentLevel++;
            treeManager.totalPassivesBought++;

            ApplyStatBoost();

            UpdateNodeText();
            treeManager.UpdateUI();
            treeManager.ShowTooltip(skillName, GetDynamicDescription());
        }
        else
        {
            treeManager.TriggerBrokeError(skillName, GetDynamicDescription());
        }
    }

    private void ApplyStatBoost()
    {
        if (Player.Instance == null || Player.Instance.stats == null) return;

        switch (statToBoost)
        {
            case SkillStatType.MeleeDamage:
                Player.Instance.stats.IncreaseMeleeAttackBoost((int)boostPerLevel);
                break;
            case SkillStatType.CriticalDamage:
                Player.Instance.stats.IncreaseCriticalHitBoost(boostPerLevel);
                break;
            case SkillStatType.CriticalChance:
                Player.Instance.stats.IncreaseCriticalHitChance(boostPerLevel);
                break;
            case SkillStatType.MaxHealth:
                Player.Instance.stats.IncreaseMaximumHealth((int)boostPerLevel);
                break;
            case SkillStatType.MaxSP:
                Player.Instance.stats.IncreaseMaximumSP((int)boostPerLevel);
                break;
            case SkillStatType.BaseDamage:
                Player.Instance.stats.IncreaseBaseDamage((int)boostPerLevel);
                break;
            case SkillStatType.BaseDefense:
                Player.Instance.stats.IncreaseBaseDefense((int)boostPerLevel);
                break;
            case SkillStatType.DebuffResist:
                Player.Instance.stats.DecreaseDebuffChance(boostPerLevel);
                break;
            case SkillStatType.RangedDamage:
                Player.Instance.stats.IncreaseRangedAttackBoost((int)boostPerLevel);
                break;
        }
    }

    private void UpdateNodeText()
    {
        if (levelText != null)
        {
            levelText.text = currentLevel + " / " + maxLevel;
        }

        // Automatically populates the text field on your node with its setup cost
        if (costText != null)
        {
            costText.text = "Cost: " + toothCost;
        }
    }

    private string GetDynamicDescription()
    {
        return skillDescription + "\n\n(Current Level: " + currentLevel + ")\n(Cost: " + toothCost + ")";
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        treeManager.ShowTooltip(skillName, GetDynamicDescription());
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        treeManager.HideTooltip();
    }
}