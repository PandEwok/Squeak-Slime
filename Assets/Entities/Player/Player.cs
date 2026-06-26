using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

public class Player : MonoBehaviour
{
    public enum BiomeType
    {
        FOREST = 1,
        CASTLE_EXTERIOR = 2,
        RAT_LABORATORY = 3,
        RAT_BATTLE = 4
    }

    public static Player Instance { get; private set; }
    [HideInInspector] public Vector3 originalPosition;
    [Header("Game scripts")]
    public Combat_Logic combatLogic;
    public PlayerInventory inventory;
    public int floor = 1;
    public int maxFloor = 6;
    public BiomeType currentBiome = BiomeType.FOREST;
    [HideInInspector] public UiManager uiManager;
    [HideInInspector] public PlayerStats stats;
    [HideInInspector] public bool IsInBattle = false;
    [HideInInspector] public bool IsDead = false;
    [HideInInspector] public string pendingEventID = "";
    public SpriteRenderer sprite;
    [Header("Scenes")]
    public int biome1scene = 9;
    public int biome2scene = 11;
    public int biome3scene = 12;
    public int bossScene = 13;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (Instance != this)
        {
            Debug.Log($"[Singleton] Doublon de {gameObject.name} détecté et détruit.");
            Destroy(gameObject);
            return;
        }

        stats = GetComponent<PlayerStats>();
        uiManager = GetComponent<UiManager>();
    }
    private void Start()
    {
        
        originalPosition = transform.position;
    }
    private void Update()
    {
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

    public string GetBiomeToString()
    {
        switch (currentBiome)
        {
            case BiomeType.FOREST:
                return "Forest";
            case BiomeType.CASTLE_EXTERIOR:
                return "Exteriors of the castle";
            case BiomeType.RAT_LABORATORY:
                return "Rat laboratory";
            case BiomeType.RAT_BATTLE:
                return "Rat battle";
            default:
                return "Unknown";
        }
    }


    public void LoadPlayer(Vector2 pos)
    {
        transform.position = pos;
        originalPosition = pos;
        IsInBattle = true;
        if (GameObject.FindGameObjectWithTag("CombatLogic").GetComponent<Combat_Logic>() != null)
        {
            combatLogic = GameObject.FindGameObjectWithTag("CombatLogic").GetComponent<Combat_Logic>();

            if(uiManager.actionMenu == null)
            {
                Debug.LogError("Erreur, action Menu est DCD");
            }
            else
            {
                uiManager.actionMenu.SetActive(true);
            }
            if (uiManager.actionMenu.GetComponent<ActionBarScript>().combatLogic == null)
            {
                uiManager.actionMenu.GetComponent<ActionBarScript>().combatLogic = combatLogic;
            }
            if (combatLogic == null)
            {
                Debug.LogError("Combat Logic de player est DCD");
            }
            //GetComponentInChildren<ActionBarScript>().combatLogic = GameObject.FindGameObjectWithTag("CombatLogic").GetComponent<Combat_Logic>();
        }
        uiManager.actionMenu.GetComponent<ActionBarScript>().playerScript = this;

        if(IsDead)
        {
            stats.health = stats.originalHealth;
            stats.SP = stats.originalSP;
        }
        IsDead = false;
        stats.ResetPlayerStats();
        uiManager.statsUi = GameObject.FindGameObjectWithTag("Canvas").GetComponent<StatsUI>();
    }

    public void SwitchSceneInCaseOfVictory()
    {
        switch (currentBiome)
        {
            case BiomeType.FOREST:
                SceneManager.LoadSceneAsync(9);
                Debug.Log("Loading Forest");
                break;
            case BiomeType.CASTLE_EXTERIOR:
                SceneManager.LoadSceneAsync(11);
                Debug.Log("Loading castle exteriors");
                break;
            case BiomeType.RAT_LABORATORY:
                SceneManager.LoadSceneAsync(12);
                Debug.Log("Loading rat lab");
                break;
            case BiomeType.RAT_BATTLE:
                SceneManager.LoadSceneAsync(13);
                Debug.Log("Loading boss battle");
                break;
            default:
                Debug.LogError("Biome invalide");
                break;
        }
    }
}