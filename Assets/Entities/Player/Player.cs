using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    public enum BiomeType
    {
        FOREST = 1,
        CASTLE_EXTERIOR = 2,
        RAT_LABORATORY = 3
    }
    public static Player Instance { get; private set; }
    [HideInInspector] public Vector3 originalPosition;
    [Header("Game scripts")]
    public Combat_Logic combatLogic;
    public PlayerInventory inventory;
    public int floor = 1;
    public BiomeType currentBiome = BiomeType.FOREST;
    [HideInInspector] public UiManager uiManager;
    [HideInInspector] public PlayerStats stats;
    [HideInInspector] public bool hasWon = false;
    public SpriteRenderer sprite;
    
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
            default:
                return "Unknown";
        }
    }
}