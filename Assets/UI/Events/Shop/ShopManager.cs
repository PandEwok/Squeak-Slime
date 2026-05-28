using UnityEngine;
using System.Collections.Generic;

public class ShopManager : MonoBehaviour
{
    [Header("Shopkeeper Interaction")]
    public ShopkeeperDialogue shopkeeperScript;

    public List<string> thankYouLines = new List<string> { "Thanks!", "Pleasure doing business!" };
    public List<string> brokeLines = new List<string> { "No teeth? No deal." };

    [Header("Modular Petting Settings")]
    [Range(0f, 1f)] public float winChance = 0.75f;
    public int goodOutcomeDiscount = -1;
    public int minBadPenalty = 1;
    public int maxBadPenalty = 3;

    [Header("Petting Visuals (Colors)")]
    public Color goodOutcomeColor = Color.green;
    public Color badOutcomeColor = Color.red;

    [Header("Petting Dialogue Customization")]
    public List<string> happyPetLines = new List<string> { "*Purr*... Discounts!" };
    public List<string> angryPetLines = new List<string> { "Hands off! Inflation time!" };

    [Header("Continuous Petting Dialogue (Cosmetic Only!)")]
    public List<string> followUpGoodLines = new List<string> { "Stop it, you already got your discount!" };
    public List<string> followUpBadLines = new List<string> { "Keep touching me and see what happens." };

    [Header("UI & Prefab Connections")]
    public Transform shopContainer;
    public GameObject shopItemPrefab;

    [Header("The Item Catalog (Now using the updated Unified Items!)")]
    public List<ItemData> itemCatalog;

    [Header("The Global Currency Catalog")]
    public List<CurrencyData> globalCurrencies;

    [Header("Global Shop Layout Luck Settings")]
    public int absoluteMinSlots = 1;
    public int stage1MaxSlots = 3;
    public int extraMaxSlotsAtEnd = 1;

    private const int totalStagesPerFloor = 6;
    private List<ItemData> spawnedItemsThisShop = new List<ItemData>();
    private List<ShopItemUI> activeUISlots = new List<ShopItemUI>();

    private int totalTimesPetted = 0;
    private bool wasFirstPetGood = false;

    void Start()
    {
        GenerateProgressionShop();
    }

    void GenerateProgressionShop()
    {
        int globalFloor = GameManager.Instance.currentFloor;
        int globalStage = GameManager.Instance.currentStage;

        foreach (Transform child in shopContainer) Destroy(child.gameObject);
        spawnedItemsThisShop.Clear();
        activeUISlots.Clear();

        totalTimesPetted = 0;
        wasFirstPetGood = false;

        List<CurrencyData> unlockedCurrencies = FilterCurrenciesByFloor(globalFloor);
        if (unlockedCurrencies.Count == 0) return;

        // 1. Roll the initial size based on stage progression
        int slotsToSpawn = CalculateShopSize(globalStage);

        // =========================================================================
        // CRITICAL SAFETY CLAMP: Fixes the Infinite Loop Freeze!
        // =========================================================================
        if (itemCatalog == null || itemCatalog.Count == 0)
        {
            Debug.LogError("[ShopManager] CRITICAL: Your Item Catalog list is empty in the inspector! Cannot spawn shop slots.");
            return;
        }

        // If the shop wants 4 slots but you only have 3 items total, force it down to 3!
        if (slotsToSpawn > itemCatalog.Count)
        {
            slotsToSpawn = itemCatalog.Count;
        }
        // =========================================================================

        for (int i = 0; i < slotsToSpawn; i++)
        {
            CreateAutomatedSlot(unlockedCurrencies, globalFloor, globalStage);
        }
    }

    List<CurrencyData> FilterCurrenciesByFloor(int currentFloor)
    {
        List<CurrencyData> filtered = new List<CurrencyData>();
        foreach (CurrencyData currency in globalCurrencies)
        {
            if (currency.toothRank <= currentFloor) filtered.Add(currency);
        }
        return filtered;
    }

    int CalculateShopSize(int stage)
    {
        float progressPercentage = (float)(stage - 1) / (totalStagesPerFloor - 1);
        int currentMaxPossible = stage1MaxSlots;
        if (Random.value < progressPercentage) currentMaxPossible += extraMaxSlotsAtEnd;
        return Random.Range(absoluteMinSlots, currentMaxPossible + 1);
    }

    void CreateAutomatedSlot(List<CurrencyData> availableCurrencies, int currentFloor, int currentStage)
    {
        if (itemCatalog.Count == 0) return;

        ItemData randomItem = null;
        while (randomItem == null)
        {
            ItemData rolledItem = itemCatalog[Random.Range(0, itemCatalog.Count)];
            if (!spawnedItemsThisShop.Contains(rolledItem))
            {
                randomItem = rolledItem;
                spawnedItemsThisShop.Add(randomItem);
            }
        }

        CurrencyData randomCurrency = availableCurrencies[Random.Range(0, availableCurrencies.Count)];

        // =========================================================================
        // UNIFIED ECONOMY PRICING FORMULA
        // =========================================================================
        // 1. Teeth Type Tier vs Current Floor Impact
        int floorDifference = currentFloor - randomCurrency.toothRank;

        // 2. Stage Progression Impact (creep prices up slightly every 2 stages)
        int stagePriceCreep = Mathf.FloorToInt(currentStage / 2f);

        // 3. Assemble dynamic values
        int baseMinPrice = 1 + stagePriceCreep;
        int baseMaxPrice = 3 + stagePriceCreep;

        int calculatedMin = Mathf.Max(1, baseMinPrice + floorDifference);
        int calculatedMax = Mathf.Max(calculatedMin, baseMaxPrice + (floorDifference * 2));

        // 4. Final generation roll before petting modifications occur
        int finalPrice = Random.Range(calculatedMin, calculatedMax + 1);
        // =========================================================================

        GameObject newSlot = Instantiate(shopItemPrefab, shopContainer);
        ShopItemUI uiScript = newSlot.GetComponent<ShopItemUI>();

        uiScript.SetupShopItem(randomItem, finalPrice, randomCurrency, this);
        activeUISlots.Add(uiScript);
    }

    public void AttemptPurchase(ShopItemUI slotUI, ItemData item, int price, CurrencyData currency)
    {
        bool isFreeItem = (price == 0);

        if (isFreeItem || GameManager.Instance.TrySpendCurrency(currency, price))
        {
            // Give the player the item modularly via ID!
            PlayerInventory playerInv = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerInventory>();
            if (playerInv != null)
            {
                //playerInv.AddItem(item.itemId, 1);
            }

            slotUI.MarkAsSold();

            if (shopkeeperScript != null && thankYouLines.Count > 0)
            {
                shopkeeperScript.SayThankYou(thankYouLines[Random.Range(0, thankYouLines.Count)]);
            }
        }
        else
        {
            slotUI.PlayBrokeFeedback();
            if (shopkeeperScript != null && brokeLines.Count > 0)
            {
                shopkeeperScript.SayThankYou(brokeLines[Random.Range(0, brokeLines.Count)]);
            }
        }
    }

    public void PetTheShopkeeper()
    {
        if (activeUISlots.Count == 0) return;

        totalTimesPetted++;

        if (totalTimesPetted == 1)
        {
            float roll = Random.value;
            int priceModifier = 0;
            Color permanentPetColor;

            if (roll <= winChance)
            {
                wasFirstPetGood = true;
                priceModifier = goodOutcomeDiscount;
                permanentPetColor = goodOutcomeColor;
            }
            else
            {
                wasFirstPetGood = false;
                priceModifier = Random.Range(minBadPenalty, maxBadPenalty + 1);
                permanentPetColor = badOutcomeColor;
            }

            foreach (ShopItemUI slot in activeUISlots)
            {
                if (slot.purchaseButton.interactable)
                {
                    slot.ModifyPriceByPetting(priceModifier, permanentPetColor);
                }
            }

            if (shopkeeperScript != null)
            {
                string lineToSay = wasFirstPetGood ? happyPetLines[Random.Range(0, happyPetLines.Count)] : angryPetLines[Random.Range(0, angryPetLines.Count)];
                shopkeeperScript.SayThankYou(lineToSay);
            }
        }
        else
        {
            if (shopkeeperScript != null)
            {
                string lineToSay = wasFirstPetGood ? followUpGoodLines[Random.Range(0, followUpGoodLines.Count)] : followUpBadLines[Random.Range(0, followUpBadLines.Count)];
                shopkeeperScript.SayThankYou(lineToSay);
            }
        }
    }
}