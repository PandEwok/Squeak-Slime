using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class TreasureEvent : MonoBehaviour
{
    [Header("Audio Settings")]
    [Tooltip("The exact name of your main menu track in the AudioManager database.")]
    public string musicTrackName = "EventScene";

    [Header("UI & Animation References")]
    public TextMeshProUGUI dialogueText;
    public Animator exitButtonAnimator;
    public string animationTriggerName = "ShowButton";

    [Header("Dynamic Loot UI Setup")]
    public Transform lootDisplayContainer;
    public GameObject lootSlotPrefab;

    [Header("Event Settings")]
    public float typeSpeed = 0.04f;

    [Header("Modular Biome Loot Tables")]
    [Tooltip("Create a layout setting for each Biome here!")]
    public List<BiomeLootTable> biomeLootTables = new List<BiomeLootTable>();

    [Tooltip("Fallback settings if the player's current biome isn't explicitly configured above.")]
    public BiomeLootTable defaultFallbackLoot;

    [Header("Dialogue Templates")]
    [TextArea(2, 4)] public string introText = "You enter a room with a treasure chest, lucky! You soon open it...";
    [TextArea(2, 4)] public string trapTextTemplate = "The treasure was a TRAP! You lost {0} HP.";
    [TextArea(2, 4)] public string lootHeader = "The treasure was a bunch of loot! You got:";

    // State Tracking
    private int currentStep = 1;
    private string lineToPrint = "";
    private Coroutine typewriterCoroutine;
    private bool isTyping = false;

    private List<(Sprite icon, int qty, Color color)> rolledLootVisuals = new List<(Sprite, int, Color)>();
    private Vector3 playerDefPos = new Vector3(7777, 0, 0);
    public int nextSceneName = 9;
    private void Start()
    {
        if (Player.Instance != null)
        {
            Player.Instance.transform.position = playerDefPos;
        }
        if (lootDisplayContainer != null) lootDisplayContainer.gameObject.SetActive(false);


        if (AudioManager.Instance != null && !string.IsNullOrEmpty(musicTrackName))
        {
            AudioManager.Instance.PlayMusic(musicTrackName);
        }

        lineToPrint = introText;
        typewriterCoroutine = StartCoroutine(TypeTextRoutine());
    }

    private void Update()
    {
        if (Pointer.current != null && Pointer.current.press.wasPressedThisFrame)
        {
            HandleInteraction();
        }
    }

    private void HandleInteraction()
    {
        if (isTyping)
        {
            if (typewriterCoroutine != null) StopCoroutine(typewriterCoroutine);
            dialogueText.text = lineToPrint;
            isTyping = false;

            ShowLootIcons();

            if (currentStep == 2) EndTreasureEvent();
            return;
        }

        if (currentStep == 1)
        {
            currentStep = 2;
            DetermineOutcome();
            typewriterCoroutine = StartCoroutine(TypeTextRoutine());
        }
    }

    private void DetermineOutcome()
    {
        rolledLootVisuals.Clear();

        // 1. Figure out which biome the player is currently in safely
        int currentBiome = 1;
        if (Player.Instance != null)
        {
            currentBiome = ((int)Player.Instance.currentBiome);
        }

        // 2. Find the correct configuration table for this specific biome
        BiomeLootTable activeConfig = GetConfigForBiome(currentBiome);

        if (activeConfig == null)
        {
            Debug.LogError($"[TreasureEvent] No loot configurations found for Biome {currentBiome} or Fallback!");
            lineToPrint = "The chest was empty... (Check your Inspector setups!)";
            return;
        }

        // 3. Roll against this biome's custom success rate
        float roll = Random.Range(0f, 100f);

        if (roll <= activeConfig.successChance && activeConfig.possibleTreasureDrops.Count > 0)
        {
            lineToPrint = lootHeader;

            // Roll drop counts using this biome's min/max rules
            int itemsToDropCount = Random.Range(activeConfig.minSlotsDropped, activeConfig.maxSlotsDropped + 1);
            Dictionary<TreasureDrop, int> rolledLootAmounts = new Dictionary<TreasureDrop, int>();

            for (int i = 0; i < itemsToDropCount; i++)
            {
                TreasureDrop rolledDrop = activeConfig.possibleTreasureDrops[Random.Range(0, activeConfig.possibleTreasureDrops.Count)];

                if (!rolledDrop.IsValid) continue;

                int quantity = Random.Range(1, activeConfig.maxDropQuantity + 1);

                if (rolledLootAmounts.ContainsKey(rolledDrop))
                {
                    rolledLootAmounts[rolledDrop] += quantity;
                }
                else
                {
                    rolledLootAmounts.Add(rolledDrop, quantity);
                }
            }

            // Grant items & cache visuals
            foreach (KeyValuePair<TreasureDrop, int> lootEntry in rolledLootAmounts)
            {
                TreasureDrop drop = lootEntry.Key;
                int finalQuantity = lootEntry.Value;

                if (Player.Instance != null && Player.Instance.inventory != null)
                {
                    if (drop.type == TreasureDrop.DropType.Item)
                    {
                        Player.Instance.inventory.AddItem(drop.itemData, finalQuantity);
                    }
                    else if (drop.type == TreasureDrop.DropType.Tooth)
                    {
                        // FIXED: Connected to active functional tooth deposit method
                        Player.Instance.inventory.AddTooth(drop.toothData, finalQuantity);
                    }
                }

                if (drop.Icon != null)
                {
                    rolledLootVisuals.Add((drop.Icon, finalQuantity, drop.Color));
                }
            }
        }
        else
        {
            // Calculate trap damage using this biome's risk profile
            int damageTaken = Random.Range(activeConfig.minTrapDamage, activeConfig.maxTrapDamage + 1);

            if (Player.Instance != null && Player.Instance.stats != null)
            {
                Player.Instance.stats.health = Mathf.Max(0, Player.Instance.stats.health - damageTaken);
            }

            lineToPrint = string.Format(trapTextTemplate, damageTaken);
        }
    }

    // Helper method to filter through our lists smoothly
    private BiomeLootTable GetConfigForBiome(int biomeId)
    {
        foreach (BiomeLootTable table in biomeLootTables)
        {
            if (table.targetBiome == biomeId) return table;
        }
        return defaultFallbackLoot;
    }

    private IEnumerator TypeTextRoutine()
    {
        isTyping = true;
        dialogueText.text = "";

        int index = 0;
        while (index < lineToPrint.Length)
        {
            dialogueText.text += lineToPrint[index];
            index++;
            yield return new WaitForSeconds(typeSpeed);
        }

        isTyping = false;
        ShowLootIcons();

        if (currentStep == 2) EndTreasureEvent();
    }

    private void ShowLootIcons()
    {
        if (rolledLootVisuals.Count > 0 && lootDisplayContainer != null && lootSlotPrefab != null)
        {
            foreach (Transform child in lootDisplayContainer)
            {
                Destroy(child.gameObject);
            }

            lootDisplayContainer.gameObject.SetActive(true);

            foreach (var loot in rolledLootVisuals)
            {
                GameObject newSlot = Instantiate(lootSlotPrefab, lootDisplayContainer);
                LootSlotUI slotScript = newSlot.GetComponent<LootSlotUI>();
                if (slotScript != null)
                {
                    slotScript.SetupSlot(loot.icon, loot.qty, loot.color);
                }
            }
        }
    }

    private void EndTreasureEvent()
    {
        if (exitButtonAnimator != null)
        {
            exitButtonAnimator.SetTrigger(animationTriggerName);
        }
    }

    // FIXED: Now hooks perfectly into your unified progression system!
    public void ExitTreasureSceneButton()
    {
        if (Player.Instance != null)
        {
            Player.Instance.pendingEventID = "";
            // Advance floor progression counter
            Player.Instance.floor++;

            // Evaluate if floor exceeds max limits for biome shifting
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

        Player.Instance.SwitchSceneInCaseOfVictory();
    }
}

[System.Serializable]
public class BiomeLootTable
{
    [Tooltip("Which biome number does this profile apply to?")]
    public int targetBiome = 1;

    [Range(0f, 100f)] public float successChance = 50f;

    [Header("Drop Sizing Settings")]
    public int minSlotsDropped = 1;
    public int maxSlotsDropped = 3;
    public int maxDropQuantity = 3;

    [Header("Trap Scaling Settings")]
    public int minTrapDamage = 15;
    public int maxTrapDamage = 30;

    [Header("Loot Pool Selection")]
    public List<TreasureDrop> possibleTreasureDrops = new List<TreasureDrop>();
}

[System.Serializable]
public class TreasureDrop
{
    public enum DropType { Item, Tooth }
    public DropType type;

    public ItemData itemData;
    public Tooth toothData;

    public Sprite Icon => type == DropType.Item ? itemData?.itemIcon : toothData?.itemIcon;
    public string Name => type == DropType.Item ? itemData?.itemName : toothData?.itemName;
    public Color Color => type == DropType.Item ? (itemData != null ? itemData.defaultColor : Color.white) : (toothData != null ? toothData.defaultColor : Color.white);
    public bool IsValid => (type == DropType.Item && itemData != null) || (type == DropType.Tooth && toothData != null);
}