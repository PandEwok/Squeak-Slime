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
    // NEW: TRACKING ASSETS FOR CHEATS
    // ==========================================
    [Header("Cheat Setup")]
    [Tooltip("Drag your Ordinary Tooth, Golem Tooth, etc. here so the cheat loop can find them!")]
    public List<CurrencyData> allCurrencies;

    private Dictionary<CurrencyData, int> wallet = new Dictionary<CurrencyData, int>();

    void Awake()
    {
        if (_instance == null)
            _instance = this;
        else if (_instance != this)
            Destroy(gameObject);
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
    public void AddCurrency(CurrencyData currencyType, int amount)
    {
        if (wallet.ContainsKey(currencyType))
            wallet[currencyType] += amount;
        else
            wallet.Add(currencyType, amount);

        Debug.Log($"[Wallet Update] +{amount} {currencyType.currencyName}. Total Balance: {wallet[currencyType]}");
    }

    public bool TrySpendCurrency(CurrencyData currencyType, int cost)
    {
        if (wallet.ContainsKey(currencyType) && wallet[currencyType] >= cost)
        {
            wallet[currencyType] -= cost;
            Debug.Log($"[Wallet Update] Spent {cost} {currencyType.currencyName}. Remaining: {wallet[currencyType]}");
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
        // Keyboard trigger
        if (Keyboard.current != null && Keyboard.current.mKey.wasPressedThisFrame)
        {
            GiveAllCurrenciesCheat();
        }
    }

    // This makes a clickable action menu appear when you right-click the script component in the Inspector!
    [ContextMenu("Cheat Menu/Give 100 of All Teeth")]
    public void GiveAllCurrenciesCheat()
    {
        if (allCurrencies == null || allCurrencies.Count == 0)
        {
            Debug.LogWarning("[Cheat Failed] You need to drag your tooth assets into the 'All Currencies' list on the GameManager component first!");
            return;
        }

        Debug.Log("<color=gold><b>[CHEAT INJECTED] Adding 100 units to all available teeth!</b></color>");

        foreach (CurrencyData currency in allCurrencies)
        {
            AddCurrency(currency, 100);
        }
    }
}