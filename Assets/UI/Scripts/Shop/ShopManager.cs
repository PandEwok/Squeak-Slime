using UnityEngine;
using System.Collections.Generic;

public class ShopManager : MonoBehaviour
{
    [Header("Shopkeeper Interaction")]
    public ShopkeeperDialogue shopkeeperScript;

    [Tooltip("Add a few variations of thank you text!")]
    public List<string> thankYouLines = new List<string> {
        "Thanks for the purchase!",
        "Pleasure doing business with ya!",
        "Use it well!"
    };

    [Tooltip("Add a few variations of lines for when the player is too broke!")]
    public List<string> brokeLines = new List<string> {
        "No teeth? Aw, sadly I don't give credit.",
        "Come back when you're a little bit... richer."
    };

    [Header("Modular Petting Settings")]
    [Range(0f, 1f)]
    [Tooltip("Percentage chance for a good outcome (e.g., 0.75 = 75%)")]
    public float winChance = 0.75f;
    public int goodOutcomeDiscount = -1;
    public int minBadPenalty = 1;
    public int maxBadPenalty = 3;

    [Header("Petting Visuals (Colors)")]
    public Color goodOutcomeColor = Color.green;
    public Color badOutcomeColor = Color.red;

    [Header("Petting Dialogue Customization")]
    public List<string> happyPetLines = new List<string> { "*Purr*... Discounts for you!", "Aha! That's the spot." };
    public List<string> angryPetLines = new List<string> { "Don't touch the merchandise! Prices are up!", "Hey! Hands to yourself!" };

    [Header("Continuous Petting Dialogue (Cosmetic Only!)")]
    public List<string> followUpGoodLines = new List<string> { "Alright, you're pushing it now.", "Hey, stop it! I already gave you a discount!" };
    public List<string> followUpBadLines = new List<string> { "I already told you to back off!", "Seriously, I'm going to bite you if you don't stop." };

    [Header("UI & Prefab Connections")]
    public Transform shopContainer;
    public GameObject shopItemPrefab;

    [Header("The Item Catalog (Constant)")]
    public List<ShopItemData> itemCatalog;

    [Header("The Global Currency Catalog")]
    public List<CurrencyData> globalCurrencies;

    [Header("Global Shop Layout Luck Settings")]
    public int absoluteMinSlots = 1;
    public int stage1MaxSlots = 3;
    public int extraMaxSlotsAtEnd = 1;

    private const int totalStagesPerFloor = 6;
    private List<ShopItemData> spawnedItemsThisShop = new List<ShopItemData>();
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

        int slotsToSpawn = CalculateShopSize(globalStage);
        if (slotsToSpawn > itemCatalog.Count) slotsToSpawn = itemCatalog.Count;

        for (int i = 0; i < slotsToSpawn; i++)
        {
            CreateAutomatedSlot(unlockedCurrencies, globalFloor);
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

    void CreateAutomatedSlot(List<CurrencyData> availableCurrencies, int currentFloor)
    {
        if (itemCatalog.Count == 0) return;

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

        CurrencyData randomCurrency = availableCurrencies[Random.Range(0, availableCurrencies.Count)];
        int floorDifference = currentFloor - randomCurrency.toothRank;

        int baseMinPrice = 1;
        int baseMaxPrice = 3;
        int calculatedMin = baseMinPrice + floorDifference;
        int calculatedMax = baseMaxPrice + (floorDifference * 2);

        int finalPrice = Random.Range(calculatedMin, calculatedMax + 1);

        GameObject newSlot = Instantiate(shopItemPrefab, shopContainer);
        ShopItemUI uiScript = newSlot.GetComponent<ShopItemUI>();

        uiScript.SetupShopItem(randomItem, finalPrice, randomCurrency, this);
        activeUISlots.Add(uiScript);
    }

    public void AttemptPurchase(ShopItemUI slotUI, ShopItemData item, int price, CurrencyData currency)
    {
        Debug.Log($"Attempting to buy {item.itemName} for {price} {currency.currencyName}s...");

        // FIX: If the item is FREE (0), instantly bypass TrySpendCurrency entirely!
        bool isFreeItem = (price == 0);

        if (isFreeItem || GameManager.Instance.TrySpendCurrency(currency, price))
        {
            // SUCCESSFUL TRANSACTION!
            GameManager.Instance.AddItemToInventory(item);
            slotUI.MarkAsSold();

            if (shopkeeperScript != null && thankYouLines.Count > 0)
            {
                string randomLine = thankYouLines[Random.Range(0, thankYouLines.Count)];
                shopkeeperScript.SayThankYou(randomLine);
            }
        }
        else
        {
            // TRANSACTION DECLINED!
            slotUI.PlayBrokeFeedback();

            if (shopkeeperScript != null && brokeLines.Count > 0)
            {
                string randomBrokeLine = brokeLines[Random.Range(0, brokeLines.Count)];
                shopkeeperScript.SayThankYou(randomBrokeLine);
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
                string lineToSay = wasFirstPetGood
                    ? happyPetLines[Random.Range(0, happyPetLines.Count)]
                    : angryPetLines[Random.Range(0, angryPetLines.Count)];

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