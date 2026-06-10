using UnityEngine;
using System.Collections.Generic;
using UnityEngine.InputSystem;

public class GameManager : MonoBehaviour
{
    private static GameManager _instance;
    public static GameManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<GameManager>();
                if (_instance == null)
                {
                    GameObject tempGO = new GameObject("Runtime_GameManager (Editor Test)");
                    _instance = tempGO.AddComponent<GameManager>();
                    DontDestroyOnLoad(tempGO);
                    Debug.LogWarning("GameManager was missing! Created a temporary test manager.");
                }
            }
            return _instance;
        }
    }

    [Header("Global Progression State")]
    public int currentFloor = 1;
    public int currentStage = 1;
    private const int maxStages = 6;
    private const int maxFloors = 4;

    [Header("Player Inventory")]
    public List<ItemData> collectedItems = new List<ItemData>();

    // ==========================================
    // UPDATED: TRACKING TOOTH ASSETS FOR CHEATS
    // ==========================================
    [Header("Cheat Setup")]
    [Tooltip("Drag your Ordinary Tooth, Golem Tooth, etc. here so the cheat loop can find them!")]
    public List<Tooth> allTeeth; // Changed from CurrencyData to Tooth

    // The wallet now tracks Teeth scriptable objects directly!
    private Dictionary<Tooth, int> wallet = new Dictionary<Tooth, int>();

    void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject); // Added this so it safely persists between levels
        }
        else if (_instance != this)
        {
            Destroy(gameObject);
        }
    }

    // ==========================================
    // PROGRESSION SYSTEM
    // ==========================================
    public void AdvanceStage()
    {
        currentStage++;
        if (currentStage > maxStages) AdvanceFloor();
        Debug.Log($"Advanced! Now on Floor {currentFloor}, Stage {currentStage}");
    }

    private void AdvanceFloor()
    {
        currentStage = 1;
        currentFloor++;
        if (currentFloor > maxFloors) Debug.Log("Game Cleared!");
    }

    // ==========================================
    // WALLET & INVENTORY SYSTEM
    // ==========================================

    // Updated parameter type to 'Tooth'
    public void AddCurrency(Tooth toothType, int amount)
    {
        if (wallet.ContainsKey(toothType))
            wallet[toothType] += amount;
        else
            wallet.Add(toothType, amount);

        // Reads 'itemName' from your friend's Tooth script
        Debug.Log($"[Wallet Update] +{amount} {toothType.itemName}. Total Balance: {wallet[toothType]}");
    }

    // FIXED: Corrected variables from the old currencyType/cost copy-paste remnants
    public bool TrySpendCurrency(Tooth toothType, int amount)
    {
        if (wallet.ContainsKey(toothType) && wallet[toothType] >= amount)
        {
            wallet[toothType] -= amount;
            Debug.Log($"[Wallet Update] Spent {amount} {toothType.itemName}. Remaining: {wallet[toothType]}");
            return true;
        }
        return false;
    }

    public void AddItemToInventory(ItemData newItem)
    {
        collectedItems.Add(newItem);
        Debug.Log($"*** ADDED TO INVENTORY: {newItem.itemName} ***");
    }

    // ==========================================
    // THE CHEAT SYSTEMS
    // ==========================================
    void Update()
    {
        // Keyboard trigger (M key)
        if (Keyboard.current != null && Keyboard.current.mKey.wasPressedThisFrame)
        {
            GiveAllTeethCheat();
        }
    }

    // Right-click the GameManager component header in the inspector to run this manually!
    [ContextMenu("Cheat Menu/Give 100 of All Teeth")]
    public void GiveAllTeethCheat()
    {
        if (allTeeth == null || allTeeth.Count == 0)
        {
            Debug.LogWarning("[Cheat Failed] You need to drag your Tooth assets into the 'All Teeth' list on the GameManager component first!");
            return;
        }

        Debug.Log("<color=gold><b>[CHEAT INJECTED] Adding 100 units to all available teeth!</b></color>");

        foreach (Tooth tooth in allTeeth)
        {
            AddCurrency(tooth, 100);
        }
    }
}