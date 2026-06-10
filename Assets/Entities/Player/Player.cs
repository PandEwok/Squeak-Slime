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
    [SerializeField] private GameObject actionMenu;
    [SerializeField] private GameObject qteWarning;
    [SerializeField] private GameObject gradeDisplay;
    [SerializeField] private GameObject vfxSystem;
    public SpriteRenderer sprite;
    
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
                inventory.hasBite,
                inventory.hasFireball,
                inventory.hasFracture,
                inventory.hasAbsorption,
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
                inventory.hasBite = data.hasBite;
                inventory.hasFireball = data.hasFireball;
                inventory.hasFracture = data.hasFracture;
                inventory.hasAbsorption = data.hasAbsorption;

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
}