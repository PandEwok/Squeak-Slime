using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    public static Player Instance { get; private set; }
    [HideInInspector] public Vector3 originalPosition;
    [Header("Game scripts")]
    public Combat_Logic combatLogic;
    public PlayerInventory inventory;
    [HideInInspector] public PlayerStats stats;
    [HideInInspector] public GradeScript gradeScript;
    [HideInInspector] public bool hasWon = false;
    [Header("UI Elements")]
    public GameObject gameOverUI;
    public GameObject victoryUI;
    [SerializeField] private GameObject actionMenu;
    [SerializeField] private GameObject qteWarning;
    [SerializeField] private GameObject gradeDisplay;
    [SerializeField] private GameObject vfxSystem;
    public SpriteRenderer sprite;
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
    public DefenseAction defenseAction;
    public AbsorptionAction absorptionAction;
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        DontDestroyOnLoad(gameObject);
    }
    private void Start()
    {
        stats = GetComponent<PlayerStats>();
        gradeScript = gradeDisplay.GetComponent<GradeScript>();
        originalPosition = transform.position;
        gameOverUI.SetActive(false);
        victoryUI.SetActive(false);
    }

    private void Update()
    {
        if (!hasWon)
        {
            if (vfxSystem != null)
            {
                vfxSystem.GetComponent<PlayerVfx>().HandleParticles(this);
            }
        }

        //Appuyez sur S pour save
        if(Keyboard.current != null && Keyboard.current.sKey.wasPressedThisFrame)
        {
            FileManager.Instance.SaveGame(
                stats.health,
                stats.SP,
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
                stats.SP = data.SP;
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
    public void SwitchingTurn()
    {
        stats.ApplyDefenseBoost();
        combatLogic.switchTurn();
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