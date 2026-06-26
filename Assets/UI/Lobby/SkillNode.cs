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
    RangedDamage,
    HealBetweenTwoTurns // <-- HOOK 1: Added to the inspector dropdown selection!
}

public class SkillNode : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("References")]
    public SkillTreeManager treeManager;
    public TextMeshProUGUI levelText;
    public TextMeshProUGUI costText;

    [Header("Skill Information")]
    public string skillName = "Heavy Slam";
    [TextArea(3, 5)]
    public string skillDescription = "Increases melee damage.";

    [Header("Cost Setup")]
    public int toothCost = 1;

    [Header("Stat Linking")]
    public SkillStatType statToBoost;
    [Tooltip("How much does the stat increase per level? (e.g., 0.10 for 10% crit chance, or 5 for +5 Melee Damage)")]
    public float boostPerLevel = 1f;

    [Header("Upgrades Tracker")]
    public int currentLevel = 0;
    public int maxLevel = 3;

    private void Start()
    {
        switch (statToBoost)
        {
            case SkillStatType.MeleeDamage:
                currentLevel = Player.Instance.stats.meleeDamageUD;
                treeManager.totalPassivesBought = Player.Instance.stats.totalRodentUD;
                break;
            case SkillStatType.CriticalDamage:
                currentLevel = Player.Instance.stats.criticalDamageUD;
                treeManager.totalPassivesBought = Player.Instance.stats.totalRodentUD;
                break;
            case SkillStatType.CriticalChance:
                currentLevel = Player.Instance.stats.criticalChanceUD;
                treeManager.totalPassivesBought = Player.Instance.stats.totalRodentUD;
                break;
            case SkillStatType.MaxHealth:
                currentLevel = Player.Instance.stats.maxHPGolemUD;
                treeManager.totalPassivesBought = Player.Instance.stats.totalGolemUD;
                break;
            case SkillStatType.BaseDamage:
                currentLevel = Player.Instance.stats.baseDamageUD;
                treeManager.totalPassivesBought = Player.Instance.stats.totalMutantUD;
                break;
            case SkillStatType.RangedDamage:
                currentLevel = Player.Instance.stats.rangedDamageUD;
                treeManager.totalPassivesBought = Player.Instance.stats.totalMagicUD;
                break;
            case SkillStatType.BaseDefense:
                if (boostPerLevel == 2)
                {
                    currentLevel = Player.Instance.stats.baseArmorGolemUD;
                    treeManager.totalPassivesBought = Player.Instance.stats.totalGolemUD;
                }
                else
                {
                    currentLevel = Player.Instance.stats.baseArmorMutantUD;
                    treeManager.totalPassivesBought = Player.Instance.stats.totalMutantUD;
                }
                    break;
            case SkillStatType.DebuffResist:
                currentLevel = Player.Instance.stats.debuffResistanceUD;
                treeManager.totalPassivesBought = Player.Instance.stats.totalMagicUD;
                break;
            case SkillStatType.HealBetweenTwoTurns:
                currentLevel = Player.Instance.stats.healEveryTwoTurnUD;
                treeManager.totalPassivesBought = Player.Instance.stats.totalMagicUD;
                break;
            case SkillStatType.MaxSP:
                if (boostPerLevel == 3)
                {
                    currentLevel = Player.Instance.stats.maxSPGolemUD;
                    treeManager.totalPassivesBought = Player.Instance.stats.totalGolemUD;
                }
                else
                {
                    currentLevel = Player.Instance.stats.maxSPMutantUD;
                    treeManager.totalPassivesBought = Player.Instance.stats.totalMutantUD;
                }
                break;
            default:
                break;

        }
        Debug.Log($"Total tree: {treeManager.totalPassivesBought}");
        UpdateNodeText();
        treeManager.UpdateUI();
    }

    public void PurchaseUpgrade()
    {
        if (currentLevel >= maxLevel) return;

        if (treeManager.CanAfford(toothCost))
        {
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
                if(boostPerLevel == 3)
                {
                    Player.Instance.stats.IncreaseMaximumSP((int)boostPerLevel, PlayerStats.GolemOrMutant.GOLEM);
                }
                else
                {
                    Player.Instance.stats.IncreaseMaximumSP((int)boostPerLevel, PlayerStats.GolemOrMutant.MUTANT);
                }
                    break;
            case SkillStatType.BaseDamage:
                Player.Instance.stats.IncreaseBaseDamage((int)boostPerLevel);
                break;
            case SkillStatType.BaseDefense:
                if(boostPerLevel == 2)
                {
                    Player.Instance.stats.IncreaseBaseDefense((int)boostPerLevel, PlayerStats.GolemOrMutant.GOLEM);
                }
                else
                {
                    Player.Instance.stats.IncreaseBaseDefense((int)boostPerLevel, PlayerStats.GolemOrMutant.MUTANT);
                }
                    break;
            case SkillStatType.DebuffResist:
                Player.Instance.stats.DecreaseDebuffChance(boostPerLevel);
                break;
            case SkillStatType.RangedDamage:
                Player.Instance.stats.IncreaseRangedAttackBoost((int)boostPerLevel);
                break;
            case SkillStatType.HealBetweenTwoTurns: // <-- HOOK 2: Tells the player stats to heal more!
                Player.Instance.stats.IncreaseHealBetweenTwoTurns((int)boostPerLevel);
                break;
        }
        Player.Instance.stats.UpdateTotalUpgrade();
    }

    private void UpdateNodeText()
    {
        if (levelText != null)
        {
            levelText.text = currentLevel + " / " + maxLevel;
        }

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