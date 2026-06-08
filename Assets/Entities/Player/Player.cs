using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEngine.GraphicsBuffer;

public class Player : MonoBehaviour
{
    public static Player Instance { get; private set; }
    [HideInInspector] public Vector3 originalPosition;
    [Header("Game scripts")]
    public Combat_Logic combatLogic;
    public PlayerInventory inventory;
    public Stats_System stats;
    [Header("Player only Stats")]
    public int originalSP = 100;
    public int SP = 100;
    public float attackBoostStrenght = 0.5f;
    public float defenseBoostStrenght = 0.5f;
    public int baseDamage = 0;
    public int baseDefense = 0;
    protected int empowerDelay = 0;
    protected int defenseBuffDelay = 0;
    protected bool empowered = false;
    protected bool defenseBuffed = false;
    protected float particleSpawnTimer = 0;
    protected float particleSpawnDuration = 0.1f;
    protected int particleQtt = 4;
    protected float particleRandomXRange = 0.6f;
    protected float particleRandomYRange = 0.25f;
    protected float particleVerticalOffset = -0.2f;
    [HideInInspector] public bool hasWon = false;
    [Header("UI Elements")]
    public GameObject gameOverUI;
    public GameObject victoryUI;
    private GameObject actionMenu;
    private GameObject qteWarning;
    private GradeScript gradeScript;
    [Header("VFX Prefabs")]
    [SerializeField] private GameObject attackBoostEffect;
    [SerializeField] private GameObject defenseBoostEffect;
    [HideInInspector] public SpriteRenderer sprite;
    [Header("Skills Booleans")]
    public bool hasBite = false;
    public bool hasFireball = false;
    public bool hasFracture = false;
    public bool hasAbsorption = false;
    [Header("Actions")]
    public MeleeAttack meleeAttack;
    public MeleeAttack biteAttack;
    public RangedAttack rangedAttack;
    public FireballAttack fireballAttack;
    public FractureAttack fractureAttack;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        DontDestroyOnLoad(gameObject);
        Transform gradeTransform = transform.Find("GradeDisplay");
        Transform actionTransform = transform.Find("ActionMenu");
        Transform qteWarningTransform = transform.Find("QTEWarning");
        Transform spriteTransform = transform.Find("slime");
        if (gradeTransform != null)
        {
            gradeScript = gradeTransform.GetComponent<GradeScript>();
        }
        else
        {
            Debug.LogError("GradeDisplay object not found as a child of the player.");
        }
        if (actionTransform != null)
        {
            actionMenu = actionTransform.gameObject;
        }
        else
        {
            Debug.LogError("ActionMenu object not found as a child of the player.");
        }
        if (qteWarningTransform != null)
        {
            qteWarning = qteWarningTransform.gameObject;
        }
        else
        {
            Debug.LogError("QTE Warning object not found as a child of the player.");
        }
        if (spriteTransform != null)
        {
            sprite = spriteTransform.gameObject.GetComponent<SpriteRenderer>();
        }
        else
        {
            Debug.LogError("Sprite object not found as a child of the player.");
        }
    }
    private void Start()
    {
        stats = GetComponent<Stats_System>();
        originalPosition = transform.position;
        baseDamage = stats.damage;
        baseDefense = stats.defense;
        gameOverUI.SetActive(false);
        victoryUI.SetActive(false);
    }

    private void Update()
    {
        if (!hasWon)
        {
            particleSpawnTimer += Time.deltaTime;

            empowered = (empowerDelay > 0);
            if (empowered && particleSpawnTimer > particleSpawnDuration)
            {
                particleSpawnTimer = 0;
                for (int i = 0; i < particleQtt; i++)
                {
                    float randomX = Random.Range(-particleRandomXRange, particleRandomXRange);
                    float randomY = Random.Range(-particleRandomYRange, particleRandomYRange);
                    Instantiate(attackBoostEffect, this.transform.position + new Vector3(randomX, -particleVerticalOffset + randomY, 0), Quaternion.identity, this.transform);
                }
            }
            defenseBuffed = (defenseBuffDelay > 0);
            if (defenseBuffed && particleSpawnTimer > particleSpawnDuration)
            {
                particleSpawnTimer = 0;
                for (int i = 0; i < particleQtt; i++)
                {
                    float randomX = Random.Range(-particleRandomXRange, particleRandomXRange);
                    float randomY = Random.Range(-particleRandomYRange, particleRandomYRange);
                    Instantiate(defenseBoostEffect, this.transform.position + new Vector3(randomX, -particleVerticalOffset + randomY, 0), Quaternion.identity, this.transform);
                }
            }
        }

        //Appuyez sur S pour save
        if(Keyboard.current != null && Keyboard.current.sKey.wasPressedThisFrame)
        {
            FileManager.Instance.SaveGame(
                stats.health,
                SP,
                hasBite,
                hasFireball,
                hasFracture,
                hasAbsorption,
                inventory
            );
            Debug.Log("Sauvegarde effectuée !");
        }

        //Appuyez sur L pour charger les donnees
        if (Keyboard.current != null && Keyboard.current.lKey.wasPressedThisFrame)
        {
            PlayerData data = FileManager.Instance.LoadGame();
            if (data != null)
            {
                stats.health = data.HP;
                SP = data.SP;
                hasBite = data.hasBite;
                hasFireball = data.hasFireball;
                hasFracture = data.hasFracture;
                hasAbsorption = data.hasAbsorption;

                inventory.LoadInventoryData(data);
                Debug.Log("Chargement effectué !");
            }
        }
        if(Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
        {
            FileManager.Instance.DeleteSave();
            Debug.Log("Sauvegarde supprimée !");
        }
}

    

    public void HealPlayer(float healAmount)
    {
        stats.heal((int)healAmount);
    }
    public void AbsorbHealth(int damages)
    {
        if(stats.hasAbsorption)
        {
            int healAmount = Mathf.RoundToInt(damages * 0.5f);
            HealPlayer(healAmount);
        }
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
    public void SwitchingTurn()
    {
       
        defenseBuffed = (defenseBuffDelay > 0);
        if (defenseBuffed)
        {
            stats.defense = baseDefense + (int)(baseDefense * defenseBoostStrenght);
        }
        else
        {
            stats.defense = baseDefense;
        }
        combatLogic.switchTurn();
    }
    public void ApplyAttackBoost()
    {
        empowered = (empowerDelay > 0);
        if (empowered)
        {
            stats.damage = baseDamage + (int)(baseDamage * attackBoostStrenght);
        }
        else
        {
            stats.damage = baseDamage;
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
    
    public IEnumerator TriggerDefenseQTE(float windowDuration)
    {
        stats.blocking = false;
        float elapsed = 0f;

        Debug.Log("Def QTE");
        ShowQTE(true);
        while (elapsed < windowDuration)
        {
            if (Pointer.current.press.wasPressedThisFrame)
            {
                stats.blocking = true;
                Debug.Log("Blocked!");
                break;
            }

            elapsed += Time.deltaTime;
            yield return null;
        }
        ShowQTE(false);
        if (stats.blocking)
        {
            DisplayGrade(GradeScript.Grade.Excellent, true);
        }

    }

    public void GameOver()
    {
        gameOverUI.SetActive(true);
        actionMenu.SetActive(false);
        gameOverUI.GetComponent<UI_GameoverScript>().ToggleGameOverUiVisibility(true);
    }

    public void Victory()
    {
        victoryUI.SetActive(true);
        actionMenu.SetActive(false);
        victoryUI.GetComponent<UI_VictoryScript>().ToggleVictoryUiVisibility(true);
    }

    public void ShowQTE(bool mustDisplay)
    {
        if (mustDisplay)
        {
            qteWarning.SetActive(true);
            AudioManager.Instance.PlaySFX("QTE");
        }
        else
        {
            qteWarning.SetActive(false);
        }
    }
    public void DisplayGrade(GradeScript.Grade grade, bool display)
    {
        if (gradeScript != null)
        {
            gradeScript.StopAllCoroutines();
            gradeScript.gameObject.SetActive(true);
            StartCoroutine(gradeScript.GradeDisplay(grade, display));
        }
    }

    public bool DoesHaveAnySkill()
    {
        return hasBite || hasFireball || hasFracture || hasAbsorption;
    }
}