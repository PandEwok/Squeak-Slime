using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

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

    [Header("The Item Catalog")]
    public List<ItemData> itemCatalog;

    [Header("The Global Tooth Catalog (Updated!)")]
    public List<Tooth> globalTeeth;

    [Header("Global Shop Layout Luck Settings")]
    public int absoluteMinSlots = 1;
    public int stage1MaxSlots = 3;
    public int extraMaxSlotsAtEnd = 1;

    private const int totalStagesPerFloor = 6;
    private List<ItemData> spawnedItemsThisShop = new List<ItemData>();
    private List<ShopItemUI> activeUISlots = new List<ShopItemUI>();

    private int totalTimesPetted = 0;
    private bool wasFirstPetGood = false;
    private Vector3 playerDefPos = new Vector3(7777, 0, 0);
    public int nextSceneName = 9;

    void Start()
    {
        Player.Instance.transform.position = playerDefPos;
        GenerateProgressionShop();
    }
    public void ExitShop()
    {
        SceneManager.LoadSceneAsync(nextSceneName);
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

        // Uses the new Tooth filter
        List<Tooth> unlockedTeeth = FilterTeethByFloor(globalFloor);
        if (unlockedTeeth.Count == 0) return;

        int slotsToSpawn = CalculateShopSize(globalStage);

        if (itemCatalog == null || itemCatalog.Count == 0)
        {
            Debug.LogError("[ShopManager] CRITICAL: Your Item Catalog list is empty!");
            return;
        }

        if (slotsToSpawn > itemCatalog.Count)
        {
            slotsToSpawn = itemCatalog.Count;
        }

        for (int i = 0; i < slotsToSpawn; i++)
        {
            CreateAutomatedSlot(unlockedTeeth, globalFloor, globalStage);
        }
    }

    List<Tooth> FilterTeethByFloor(int currentFloor)
    {
        List<Tooth> filtered = new List<Tooth>();
        foreach (Tooth tooth in globalTeeth)
        {
            // Checks your friend's 'rank' field
            if (tooth.rank <= currentFloor) filtered.Add(tooth);
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

    void CreateAutomatedSlot(List<Tooth> availableTeeth, int currentFloor, int currentStage)
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

        Tooth randomTooth = availableTeeth[Random.Range(0, availableTeeth.Count)];

        // Economy calculations using tooth.rank
        int floorDifference = currentFloor - randomTooth.rank;
        int stagePriceCreep = Mathf.FloorToInt(currentStage / 2f);

        int baseMinPrice = 1 + stagePriceCreep;
        int baseMaxPrice = 3 + stagePriceCreep;

        int calculatedMin = Mathf.Max(1, baseMinPrice + floorDifference);
        int calculatedMax = Mathf.Max(calculatedMin, baseMaxPrice + (floorDifference * 2));

        int finalPrice = Random.Range(calculatedMin, calculatedMax + 1);

        GameObject newSlot = Instantiate(shopItemPrefab, shopContainer);
        ShopItemUI uiScript = newSlot.GetComponent<ShopItemUI>();

        // Pass the tooth into the UI setup function
        uiScript.SetupShopItem(randomItem, finalPrice, randomTooth, this);
        activeUISlots.Add(uiScript);
    }

    public void AttemptPurchase(ShopItemUI slotUI, ItemData item, int price, Tooth tooth)
    {
        bool isFreeItem = (price == 0);

        // 1. Grab the inventory directly from your new persistent Player instance
        PlayerInventory playerInv = Player.Instance.inventory;
        if (playerInv == null)
        {
            Debug.LogError("[ShopManager] Player.Instance.inventory is missing! Cannot complete purchase.");
            return;
        }

        // 2. Check if the player possesses this specific tooth type and track their current count
        int currentToothAmount = 0;
        if (playerInv.teethPossessed.ContainsKey(tooth))
        {
            currentToothAmount = playerInv.teethPossessed[tooth];
        }

        // 3. Evaluation Condition: Free item OR player has enough of this specific Tooth asset
        if (isFreeItem || currentToothAmount >= price)
        {
            // Deduct the transaction cost from the wallet (unless it's free)
            if (!isFreeItem)
            {
                playerInv.RemoveTooth(tooth, price);
                Debug.Log($"[Shop] Successfully purchased {item.itemName}! Spent {price} {tooth.itemName}. Remaining: {currentToothAmount - price}");
            }
            else
            {
                Debug.Log($"[Shop] Successfully claimed {item.itemName} for free!");
            }

            // Add the physical item into the player's items dictionary
            playerInv.AddItem(item, 1);

            // Update UI slot feedback
            slotUI.MarkAsSold();

            // Play happy shopkeeper feedback lines
            if (shopkeeperScript != null && thankYouLines.Count > 0)
            {
                shopkeeperScript.SayThankYou(thankYouLines[Random.Range(0, thankYouLines.Count)]);
            }
        }
        else
        {
            // Deny purchase if the player's balance for this specific tooth asset is too low
            Debug.LogWarning($"[Shop] Purchase Denied! You need {price} {tooth.itemName}, but you only have {currentToothAmount}.");

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