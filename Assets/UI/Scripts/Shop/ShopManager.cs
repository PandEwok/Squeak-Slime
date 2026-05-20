using UnityEngine;
using System.Collections.Generic;

public class ShopManager : MonoBehaviour
{
    [Header("UI & Prefab Connections")]
    public Transform shopContainer;
    public GameObject shopItemPrefab;

    [Header("The Item Catalog (Constant)")]
    public List<ShopItemData> itemCatalog;

    [Header("The Global Currency Catalog")]
    [Tooltip("Drag ALL your tooth assets here (Ordinary, Golem, Mutant, etc.)")]
    public List<CurrencyData> globalCurrencies;

    [Header("Global Shop Layout Luck Settings")]
    public int absoluteMinSlots = 1;
    public int stage1MaxSlots = 3;
    public int extraMaxSlotsAtEnd = 1;

    private const int totalStagesPerFloor = 6;
    private List<ShopItemData> spawnedItemsThisShop = new List<ShopItemData>();

    void Start()
    {
        GenerateProgressionShop();
    }

    void GenerateProgressionShop()
    {
        // 1. Read live numbers easily from global tracker
        int globalFloor = GameManager.Instance.currentFloor;
        int globalStage = GameManager.Instance.currentStage;

        // Visual layout resets
        foreach (Transform child in shopContainer) Destroy(child.gameObject);
        spawnedItemsThisShop.Clear();

        // 2. Gather only the currencies that are unlocked up to this floor
        List<CurrencyData> unlockedCurrencies = FilterCurrenciesByFloor(globalFloor);
        if (unlockedCurrencies.Count == 0)
        {
            Debug.LogError("No currencies found matching Floor " + globalFloor);
            return;
        }

        // 3. Calculate slot size using the luck algorithm
        int slotsToSpawn = CalculateShopSize(globalStage);
        if (slotsToSpawn > itemCatalog.Count) slotsToSpawn = itemCatalog.Count;

        // 4. Generate the unique, dynamic slots
        for (int i = 0; i < slotsToSpawn; i++)
        {
            CreateAutomatedSlot(unlockedCurrencies, globalFloor);
        }
    }

    // Filters our global list to only pull teeth that are allowed on this floor
    List<CurrencyData> FilterCurrenciesByFloor(int currentFloor)
    {
        List<CurrencyData> filtered = new List<CurrencyData>();
        foreach (CurrencyData currency in globalCurrencies)
        {
            // If we are on Floor 2, this grabs Rank 1 (Ordinary) and Rank 2 (Golem)
            if (currency.toothRank <= currentFloor)
            {
                filtered.Add(currency);
            }
        }
        return filtered;
    }

    int CalculateShopSize(int stage)
    {
        float progressPercentage = (float)(stage - 1) / (totalStagesPerFloor - 1);
        int currentMaxPossible = stage1MaxSlots;

        if (Random.value < progressPercentage)
        {
            currentMaxPossible += extraMaxSlotsAtEnd;
        }

        return Random.Range(absoluteMinSlots, currentMaxPossible + 1);
    }

    void CreateAutomatedSlot(List<CurrencyData> availableCurrencies, int currentFloor)
    {
        if (itemCatalog.Count == 0) return;

        // 1. Roll a unique item (Prevents duplicates)
        ShopItemData randomItem = null;
        while (randomItem == null)
        {
            ShopItemData rolledItem = itemCatalog[Random.Range(0, itemCatalog.Count)];
            if (!spawnedItemsThisShop.Contains(rolledItem))
            {
                randomItem = rolledItem;
                spawnedItemsThisShop.Add(randomItem);
            }
        }

        // 2. Pick a random unlocked currency
        CurrencyData randomCurrency = availableCurrencies[Random.Range(0, availableCurrencies.Count)];

        // 3. DYNAMIC INFLATION MATH
        // Deduce price based on how many floors have passed since this tooth was introduced!
        int floorDifference = currentFloor - randomCurrency.toothRank; // e.g., On Floor 2, Ordinary (Rank 1) difference is 1.

        // Base price for a brand-new tooth rank is always 1 to 3
        int baseMinPrice = 1;
        int baseMaxPrice = 3;

        // For every floor that passes, shift the price boundaries up!
        // Floor 1 Ordinary: Diff 0 -> Min (1 + 0) to Max (3 + 0) = 1-3
        // Floor 2 Ordinary: Diff 1 -> Min (1 + 1) to Max (3 + 2) = 2-5
        // Floor 3 Ordinary: Diff 2 -> Min (1 + 2) to Max (3 + 4) = 3-7
        int calculatedMin = baseMinPrice + floorDifference;
        int calculatedMax = baseMaxPrice + (floorDifference * 2); // Max grows slightly faster for higher inflation!

        int finalPrice = Random.Range(calculatedMin, calculatedMax + 1);

        // 4. Instantiate UI
        GameObject newSlot = Instantiate(shopItemPrefab, shopContainer);
        ShopItemUI uiScript = newSlot.GetComponent<ShopItemUI>();

        uiScript.SetupShopItem(
            randomItem.itemIcon,
            randomItem.defaultColor,
            finalPrice,
            randomCurrency.currencyIcon,
            randomCurrency.defaultColor
        );
    }
}