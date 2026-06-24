using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class ShopManager : MonoBehaviour
{

    [Header("Audio Settings")]
    [Tooltip("The exact name of your main menu track in the AudioManager database.")]
    public string musicTrackName = "ShopTheme";

    [System.Serializable]
    public struct ItemRollConfig
    {
        public ItemData item;
        [Range(0, 99)] public int minStackQuantity;
        [Range(0, 99)] public int maxStackQuantity;
    }

    [System.Serializable]
    public struct ToothPriceConfig
    {
        public Tooth toothType;
        public int minPrice;
        public int maxPrice;
    }

    [System.Serializable]
    public class BiomeShopSetup
    {
        public string setupName;
        public Player.BiomeType targetBiome;
        public List<ItemRollConfig> itemPool;
        public List<ToothPriceConfig> allowedTeeth;

        [Header("Layout Limits")]
        public int minSlotsToSpawn = 2;
        public int maxSlotsToSpawn = 4;
    }

    [Header("Shopkeeper Interaction Strings")]
    public ShopkeeperDialogue shopkeeperScript;
    public List<string> thankYouLines = new List<string> { "Thanks!", "Pleasure doing business!" };
    public List<string> brokeLines = new List<string> { "No teeth? No deal." };

    [Header("Modular Petting Probabilities")]
    [Tooltip("60% base chance for a good outcome discount.")]
    [Range(0f, 1f)] public float winChance = 0.60f;
    public int goodOutcomeDiscount = -1;

    [Header("Petting Visuals (Colors)")]
    public Color goodOutcomeColor = Color.green;
    public Color badOutcomeColor = Color.red;

    [Header("Petting Dialogue Tuning Profiles")]
    public List<string> happyPetLines = new List<string> { "*Purr*... Discounts!" };
    public List<string> angryPetLines = new List<string> { "Hands off! Inflation time!" };
    public List<string> followUpGoodLines = new List<string> { "Stop it, you already got your discount!" };
    public List<string> followUpBadLines = new List<string> { "Keep touching me and see what happens." };

    [Header("UI & Prefab Connections")]
    public Transform shopContainer;
    public GameObject shopItemPrefab;

    [Header("Modular Biome Catalogs")]
    public List<BiomeShopSetup> biomeShopProfiles = new List<BiomeShopSetup>();

    private List<ItemData> spawnedItemsThisShop = new List<ItemData>();
    private List<ShopItemUI> activeUISlots = new List<ShopItemUI>();

    private int totalTimesPetted = 0;
    private bool wasFirstPetGood = false;
    private Vector3 playerDefPos = new Vector3(7777, 0, 0);
    public int nextSceneName = 9;

    void Start()
    {
        if (Player.Instance != null)
        {
            Player.Instance.transform.position = playerDefPos;
        }
        GenerateProgressionShop();
        if (AudioManager.Instance != null && !string.IsNullOrEmpty(musicTrackName))
        {
            AudioManager.Instance.PlayMusic(musicTrackName);
        }
    }

    void GenerateProgressionShop()
    {
        // 1. CLEAR THE UI IMMEDIATELY
        // We loop backwards to avoid layout group flickering issues in Unity
        for (int i = shopContainer.childCount - 1; i >= 0; i--)
        {
            Destroy(shopContainer.GetChild(i).gameObject);
        }

        spawnedItemsThisShop.Clear();
        activeUISlots.Clear();
        totalTimesPetted = 0;
        wasFirstPetGood = false;

        Player.BiomeType currentBiome = (Player.Instance != null) ? Player.Instance.currentBiome : Player.BiomeType.FOREST;
        BiomeShopSetup activeConfig = GetConfigForBiome(currentBiome);

        if (activeConfig == null) return;

        // 2. FIRST PASS: Roll quantities for all items to see what actually wants to spawn
        List<KeyValuePair<ItemRollConfig, int>> availableSpawns = new List<KeyValuePair<ItemRollConfig, int>>();

        foreach (ItemRollConfig rollSetup in activeConfig.itemPool)
        {
            int rolledQuantity = Random.Range(rollSetup.minStackQuantity, rollSetup.maxStackQuantity + 1);
            if (rolledQuantity > 0)
            {
                availableSpawns.Add(new KeyValuePair<ItemRollConfig, int>(rollSetup, rolledQuantity));
            }
            else
            {
                Debug.Log($"[Shop Rarity] {rollSetup.item.itemName} rolled 0 and is excluded from this shop visit.");
            }
        }

        // 3. Determine how many total slots we are allowed to display
        int targetSlots = Random.Range(activeConfig.minSlotsToSpawn, activeConfig.maxSlotsToSpawn + 1);

        // Safety clamp: We can't display more slots than we have available items
        int finalSlotsToSpawn = Mathf.Min(targetSlots, availableSpawns.Count);

        Debug.Log($"[Shop Layout] Target Slots: {targetSlots} | Available Items: {availableSpawns.Count} | Final Spawns: {finalSlotsToSpawn}");

        // 4. SECOND PASS: Select random unique items from our available pool up to our layout limit
        for (int i = 0; i < finalSlotsToSpawn; i++)
        {
            int randomIndex = Random.Range(0, availableSpawns.Count);
            var selectedSelection = availableSpawns[randomIndex];

            // Remove it from availableSpawns so it can't be chosen again for a duplicate slot!
            availableSpawns.RemoveAt(randomIndex);

            // Build the physical UI element slot
            BuildPhysicalShopSlot(selectedSelection.Key, selectedSelection.Value, activeConfig);
        }
    }

    // Renamed and streamlined to handle clean instantiation only
    void BuildPhysicalShopSlot(ItemRollConfig rolledItemConfig, int finalQuantity, BiomeShopSetup config)
    {
        ToothPriceConfig toothPriceProfile = config.allowedTeeth[Random.Range(0, config.allowedTeeth.Count)];
        int finalPrice = Random.Range(toothPriceProfile.minPrice, toothPriceProfile.maxPrice + 1);

        GameObject newSlot = Instantiate(shopItemPrefab, shopContainer);
        ShopItemUI uiScript = newSlot.GetComponent<ShopItemUI>();

        uiScript.SetupShopItem(rolledItemConfig.item, finalQuantity, finalPrice, toothPriceProfile.toothType, this);
        activeUISlots.Add(uiScript);
        spawnedItemsThisShop.Add(rolledItemConfig.item);
    }

    BiomeShopSetup GetConfigForBiome(Player.BiomeType biome)
    {
        foreach (BiomeShopSetup profile in biomeShopProfiles)
        {
            if (profile.targetBiome == biome) return profile;
        }
        return biomeShopProfiles.Count > 0 ? biomeShopProfiles[0] : null;
    }

    void CreateAutomatedSlot(BiomeShopSetup config)
    {
        ItemRollConfig rolledItemConfig = default;
        int finalQuantity = 0;
        bool foundValidItem = false;

        // 1. Create a temporary list of pool options, excluding items already on display
        List<ItemRollConfig> availableOptions = new List<ItemRollConfig>(config.itemPool);
        availableOptions.RemoveAll(x => spawnedItemsThisShop.Contains(x.item));

        // 2. Loop through options until we find an item that rolls a quantity greater than 0
        while (availableOptions.Count > 0)
        {
            int randomIndex = Random.Range(0, availableOptions.Count);
            ItemRollConfig potentialRoll = availableOptions[randomIndex];

            // Roll the stack quantity right now
            int rolledQuantity = Random.Range(potentialRoll.minStackQuantity, potentialRoll.maxStackQuantity + 1);

            if (rolledQuantity > 0)
            {
                // Success! We rolled a valid stack size
                rolledItemConfig = potentialRoll;
                finalQuantity = rolledQuantity;

                // Mark it as spawned so it occupies this slot
                spawnedItemsThisShop.Add(rolledItemConfig.item);
                foundValidItem = true;
                break;
            }
            else
            {
                // It rolled 0! The item failed its rarity check for this shop visit.
                // Mark it as "spawned" globally so the shop doesn't try to force it into the next slot...
                spawnedItemsThisShop.Add(potentialRoll.item);

                // ...but remove it from this specific slot's options so the loop can try a different item!
                availableOptions.RemoveAt(randomIndex);
                Debug.Log($"[Shop Rarity] {potentialRoll.item.itemName} rolled 0 units and skipped spawning this time.");
            }
        }

        // 3. If every single item left in the pool rolled 0, stop generating this slot gracefully
        if (!foundValidItem) return;

        // 4. Proceed with currency and pricing calculations for the successful item
        ToothPriceConfig toothPriceProfile = config.allowedTeeth[Random.Range(0, config.allowedTeeth.Count)];
        int finalPrice = Random.Range(toothPriceProfile.minPrice, toothPriceProfile.maxPrice + 1);

        GameObject newSlot = Instantiate(shopItemPrefab, shopContainer);
        ShopItemUI uiScript = newSlot.GetComponent<ShopItemUI>();

        uiScript.SetupShopItem(rolledItemConfig.item, finalQuantity, finalPrice, toothPriceProfile.toothType, this);
        activeUISlots.Add(uiScript);
    }

    public void AttemptPurchase(ShopItemUI slotUI, ItemData item, int quantity, int price, Tooth tooth)
    {
        if (Player.Instance == null || Player.Instance.inventory == null) return;
        PlayerInventory playerInv = Player.Instance.inventory;

        int currentToothAmount = playerInv.teethPossessed.ContainsKey(tooth) ? playerInv.teethPossessed[tooth] : 0;

        if (price == 0 || currentToothAmount >= price)
        {
            if (price > 0)
            {
                playerInv.RemoveTooth(tooth, price);
            }

            // Grants the full bundle item quantity
            playerInv.AddItem(item, quantity);
            slotUI.MarkAsSold();

            // ---> NEW CODE: Refresh the dashboard instantly! <---
            ShopCurrencyDisplay currencyUI = FindObjectOfType<ShopCurrencyDisplay>();
            if (currencyUI != null)
            {
                currencyUI.GenerateDynamicDisplay();
            }
            // ----------------------------------------------------

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
            int priceModifier = 0;
            Color permanentPetColor;

            // Updated 60/40 design rule implementation
            if (Random.value <= winChance)
            {
                wasFirstPetGood = true;
                priceModifier = goodOutcomeDiscount; // -1 Price Drop
                permanentPetColor = goodOutcomeColor;
            }
            else
            {
                wasFirstPetGood = false;
                // 50/50 Chance to penalty add +1 or +2 Price Inflation
                priceModifier = (Random.value < 0.50f) ? 1 : 2;
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

    // Progression Exit Logic synced with RestEvent
    public void ExitShop()
    {
        if (Player.Instance != null)
        {
            Player.Instance.floor++;

            if (Player.Instance.floor > Player.Instance.maxFloor)
            {
                Player.Instance.floor = 1;
                int nextBiomeIndex = (int)Player.Instance.currentBiome + 1;

                if (System.Enum.IsDefined(typeof(Player.BiomeType), nextBiomeIndex))
                {
                    Player.Instance.currentBiome = (Player.BiomeType)nextBiomeIndex;
                    Debug.Log($"[Progression] Biome shifted successfully to: {Player.Instance.currentBiome}");
                }
                else
                {
                    Debug.LogWarning("[Progression] Max Biome exceeded!");
                }
            }
        }

        SceneManager.LoadSceneAsync(nextSceneName);
    }
}