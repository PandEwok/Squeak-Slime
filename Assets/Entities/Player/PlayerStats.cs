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
    [Header("Passives")]
    [SerializeField] private int meleeAttackBoost = 0;
    [SerializeField] private int rangedAttackBoost = 0;
    [SerializeField] private float debuffChance = 0.30f;
    [SerializeField] private int healEveryTurn = 0;
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
        Player.Instance.uiManager.statsUi.UpdateSP();
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
                // if enemy is boss in phase 2
                GameObject combatLogic = GameObject.FindGameObjectWithTag("CombatLogic");
                GameObject currentEnemy = combatLogic.GetComponent<Combat_Logic>().currentEnemyPlaying;
                if (currentEnemy != null)
                {
                    if (currentEnemy.GetComponent<Alchemist_AI>())
                    {
                        if (currentEnemy.GetComponent<Alchemist_AI>().GetCurrentPhase() == 2)
                        {
                            AudioManager.Instance.PlaySFX("Parade");
                            blocking = false;
                            return 0;
                        }
                    }
                }

                effectiveDamage /= 2;
                AudioManager.Instance.PlaySFX("Parade");
                Player.Instance.uiManager.DisplayGrade(GradeScript.Grade.Blocked, true);
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
            EndgameUIScript.Instance.GameOver();
        }
        blocking = false;
        return effectiveDamage;
    }

    public override void MakeBleeding()
    {
        if (MustReceiveStatus())
        {
            
            if (!isBleeding)
            {
                isBleeding = true;
                bleedingInstance = Instantiate(bloodPF, this.transform.position + new Vector3(-0.75f, 1, 0), Quaternion.identity, this.transform);
            }
            bleedingTimer = bleedingDuration + 1;
        }
    }
    public override void MakeBurned()
    {
        if (MustReceiveStatus())
        {
            if (!isOnFire)
            {
                isOnFire = true;
                fireInstance = Instantiate(firePF, this.transform.position + new Vector3(0, 1, 0), Quaternion.identity, this.transform);
            }
            fireTimer = fireDuration + 1;
        }
    }
    public override void MakeDizzy()
    {
        if (MustReceiveStatus())
        {
            if (!isDizzy)
            {
                isDizzy = true;
                dizzyInstance = Instantiate(dizzyPF, this.transform.position + new Vector3(-0.75f - 0.5f, 1, 0), Quaternion.identity, this.transform);
            }
            dizzyTimer = dizzyDuration;
        }
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
    public bool MustReceiveStatus()
    {
        float roll = Random.Range(0f, 1f);
        if (roll < debuffChance)
        {
            return true;
        }
        return false;
    }

    public void HandleHealingEveryTwoTurn()
    {
        if (healEveryTurn > 0)
        {
            Heal(healEveryTurn);
        }
    }
    public int GetMeleeAttackBoost()
    {
        return meleeAttackBoost;
    }
    public int GetRangedAttackBoost()
    {
        return rangedAttackBoost;
    }
    public float GetDebuffResistance()
    {
        return debuffChance;
    }

    /// <summary>
    /// Augmente l'attaque en cas de Melee Attack (ne concerne pas la morsure).
    /// </summary>
    /// <param name="amount">Le nombre de points de dégâts de base à augmenter (nombre entier).</param>
    public void IncreaseMeleeAttackBoost(int amount)
    {
        meleeAttackBoost += amount;
    }

    /// <summary>
    /// Augmente l'attaque en cas de RangedAttack.
    /// </summary>
    /// <param name="amount">Le nombre de points de dégâts de base à augmenter (nombre entier).</param>
    public void IncreaseRangedAttackBoost(int amount)
    {
        rangedAttackBoost += amount;
    }

    /// <summary>
    /// Diminue la probabilite de se voir inflige un statut de la part des ennemis (par defaut a 0.30f).
    /// </summary>
    /// <param name="amount">Le nombre de points de pourcentage a diminuer. /!\ Doit etre sous la forme 0.PointsADiminuer+f /!\.</param>
    public void DecreaseDebuffChance(float amount)
    {
        debuffChance -= amount;
    }

    /// <summary>
    /// Augmente la quantite de vie que le joueur regagne tous les deux tours (par défaut a 0).
    /// </summary>
    /// <param name="amount">Les points de vie en plus (il s'agit d'une incrementation pas de la valeur brute, comme pour les autres fonctions d'ailleurs.</param>
    public void IncreaseHealBetweenTwoTurns(int amount)
    {
        healEveryTurn += amount;
    }

    /// <summary>
    /// Augmente la vie max du joueur et restaure la vie.
    /// </summary>
    /// <param name="amount">Le nombre de points de points de vie max à augmenter.</param>
    public void IncreaseMaximumHealth(int amount)
    {
        originalHealth += amount;
        health = originalHealth;
    }

    /// <summary>
    /// Augmente les SP max du joueur et restaure les SP.
    /// </summary>
    /// <param name="amount">Le nombre de SP max à augmenter.</param>
    public void IncreaseMaximumSP(int amount)
    {
        originalSP += amount;
        SP = originalSP;
    }

    /// <summary>
    /// Augmente l'attaque de base du joueur (concerne n'importe quelle attaque).
    /// </summary>
    /// <param name="amount">Le nombre de points d'attaque de base à augmenter.</param>
    public void IncreaseBaseDamage(int amount)
    {
        baseDamage += amount;
        damage = baseDamage;
    }
    /// <summary>
    /// Augmente la défense de base du joueur.
    /// </summary>
    /// <param name="amount">Le nombre de points de defense de base à augmenter.</param>
    public void IncreaseBaseDefense(int amount)
    {
        baseDefense += amount;
        defense = baseDefense;
    }

    /// <summary>
    /// Augmente le boost de dégâts lors d'un coup critique.
    /// </summary>
    /// <param name="amount">Le nombre de points de pourcentage a augmenter. /!\ Doit etre sous la forme 0.PointsAAjouter /!\.</param>
    public void IncreaseCriticalHitBoost(float amount)
    {
        criticalHitBoost += amount;
    }

    /// <summary>
    /// Augmente la probabilite de faire un coup critique a chaque attaque.
    /// </summary>
    /// <param name="amount">Le nombre de points de pourcentage a augmenter. /!\ Doit etre sous la forme 0.PointsAAjouter /!\.</param>
    public void IncreaseCriticalHitChance(float amount)
    {
        criticalHitChance += amount;
    }

    public void ResetPlayerStats()
    {
        empowered = false;
        defenseBuffed = false;
        isBleeding = false;
        isOnFire = false;
        isDizzy = false;
        isAbsorbing = false;
        bleedingTimer = 0;
        fireTimer = 0;
        dizzyTimer = 0;
        absorptionTimer = 0;
        if (bleedingInstance != null)
        {
            Destroy(bleedingInstance);
        }
        if (fireInstance != null)
        {
            Destroy(fireInstance);
        }
        if (dizzyInstance != null)
        {
            Destroy(dizzyInstance);
        }
        GetComponentInChildren<SpriteRenderer>().color = originalColor;
    }
}
