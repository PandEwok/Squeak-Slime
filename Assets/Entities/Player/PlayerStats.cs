using JetBrains.Annotations;
using TMPro;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;

public class PlayerStats : Stats_System
{
    [Header("Player Specific Stats")]
    public int originalSP = 100;
    public int SP = 100;
    public float attackBoostStrenght = 0.5f;
    public float defenseBoostStrenght = 0.5f;
    public float criticalHitBoost = 0.25f;
    public float criticalHitChance = 0.10f;
    [HideInInspector] public int baseDamage = 0;
    [HideInInspector] public int baseDefense = 0;
    [HideInInspector] public int empowerDelay = 0;
    [HideInInspector] public int defenseBuffDelay = 0;
    [HideInInspector] public bool empowered = false;
    [HideInInspector] public bool defenseBuffed = false;
    [HideInInspector] public bool blocking = false;
    [HideInInspector] public bool defending = false;
    [Header("Miscellaneous")]
    [SerializeField] private string slimeDamageSound = "Slime_Damage";

    protected override void Start()
    {
        base.Start();
        baseDamage = damage;
        baseDefense = defense;
    }

    public void RestoreSP(float spAmount)
    {
        SP += (int)spAmount;
        SP = Mathf.Min(SP, originalSP);
        Debug.Log($"{gameObject.name} healed for {spAmount}. Current health: {SP}");
    }

    public void ActionEmpower(int duration, float effectValue)
    {
        empowerDelay = duration;
        attackBoostStrenght = effectValue;
        AudioManager.Instance.PlaySFX("Powerup");
    }
    public void ActionDefenseBuff(int duration, float effectValue)
    {
        defenseBuffDelay = duration;
        defenseBoostStrenght = effectValue;
        AudioManager.Instance.PlaySFX("Powerup");
    }

    public void ApplyAttackBoost()
    {
        empowered = (empowerDelay > 0);
        if (empowered)
        {
            damage = baseDamage + (int)(baseDamage * attackBoostStrenght);
        }
        else
        {
            damage = baseDamage;
        }
    }

    public void ApplyDefenseBoost()
    {
        defenseBuffed = (defenseBuffDelay > 0);
        if (defenseBuffed)
        {
            defense = baseDefense + (int)(baseDefense * defenseBoostStrenght);
        }
        else
        {
            defense = baseDefense;
        }
    }
    public void DecreaseBoosts()
    {
        if (empowerDelay > 0)
        {
            empowerDelay--;
            Debug.Log($"[BOOST ATK] Diminution ! Nouveau délai : {empowerDelay}");
        }
        if (defenseBuffDelay > 0)
        {
            defenseBuffDelay--;
        }

    }

    public void AbsorbHealth(int damages)
    {
        if (isAbsorbing)
        {
            int healAmount = Mathf.RoundToInt(damages * 0.5f);
            Heal(healAmount);
        }
    }


    public override int TakeDamage(int damageAmount, bool isStatusDamage, Enemy_AI.attackDirection hitDirection = Enemy_AI.attackDirection.NONE)
    {
        int effectiveDamage = 0;
        if (isStatusDamage)
        {
            effectiveDamage = damageAmount;
        }
        else
        {
            effectiveDamage = Mathf.Max(damageAmount - defense, 0);
            Enemy_AI enemyAI = this.gameObject.GetComponent<Enemy_AI>();
            if (enemyAI != null)
            {
                foreach (float buff in enemyAI.defBuffs)
                {
                    float buffReduction = damageAmount * buff;
                    effectiveDamage -= Mathf.RoundToInt(buffReduction);
                }
                effectiveDamage = Mathf.Max(effectiveDamage, 0);
            }

            if (blocking)
            {
                effectiveDamage /= 2;
                AudioManager.Instance.PlaySFX("Parade");
            }
            if (defending)
            {
                effectiveDamage /= 2;
            }
        }
        health -= effectiveDamage;
        AudioManager.Instance.PlaySFX(slimeDamageSound);
        GameObject newDmgDisplay;

        Vector3 spawnPos = new Vector3(this.transform.position.x, this.transform.position.y + 2);
        float randomXOffset = UnityEngine.Random.Range(-0.5f, 0.5f);
        spawnPos.x += randomXOffset;
        float randomYOffset = UnityEngine.Random.Range(-0.5f, 0.5f);
        spawnPos.y += randomYOffset;


        StartCoroutine(DmgShake());
        StartCoroutine(DmgShade());

        newDmgDisplay = Instantiate(damagePF, spawnPos, Quaternion.identity, GameObject.FindGameObjectWithTag("Canvas").transform);
        newDmgDisplay.GetComponent<TextMeshProUGUI>().SetText(effectiveDamage.ToString());

        Debug.Log($"{gameObject.name} took {effectiveDamage} damage. Remaining health: {health}");
        if (health <= 0)
        {
            Debug.Log("Player has died. Game Over.");
            EngameUIScript.Instance.GameOver();
        }
        return effectiveDamage;
    }

    public override void MakeBleeding()
    {
        if (!isBleeding)
        {
            isBleeding = true;
            bleedingInstance = Instantiate(bloodPF, this.transform.position + new Vector3(-0.75f, 3, 0), Quaternion.identity, this.transform);
        }
        bleedingTimer = bleedingDuration + 1;
    }
    public override void MakeBurned()
    {
        if (!isOnFire)
        {
            isOnFire = true;
            fireInstance = Instantiate(firePF, this.transform.position + new Vector3(0, 3, 0), Quaternion.identity, this.transform);
        }
        fireTimer = fireDuration + 1;
    }
    public override void MakeDizzy()
    {
        if (!isDizzy)
        {
            isDizzy = true;
            dizzyInstance = Instantiate(dizzyPF, this.transform.position + new Vector3(-0.75f - 0.5f, 3, 0), Quaternion.identity, this.transform);
        }
        dizzyTimer = dizzyDuration;
    }

    public int HasCriticalHit(int value)
    {
        float roll = Random.Range(0f, 1f);

        if (roll < criticalHitChance)
        {
            Player.Instance.uiManager.DisplayGrade(GradeScript.Grade.Critical, true);
            return value + (int)(value * criticalHitBoost);
        }
        return value;
    }
}
