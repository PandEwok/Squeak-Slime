using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class SearchEvent : MonoBehaviour
{
    [Header("UI & Animation References")]
    public TextMeshProUGUI dialogueText;
    public Animator exitButtonAnimator;
    public string animationTriggerName = "ShowButton";

    [Header("Dynamic Loot UI Setup")]
    public Transform lootDisplayContainer;
    public GameObject lootSlotPrefab;

    [Header("Event Settings")]
    public float typeSpeed = 0.04f;

    [Header("Modular Biome Search Configs")]
    [Tooltip("Configure safe searching parameters for each biome layout here!")]
    public List<BiomeSearchTable> biomeSearchTables = new List<BiomeSearchTable>();

    [Tooltip("Fallback settings if the player's current biome isn't explicitly configured above.")]
    public BiomeSearchTable defaultFallbackSearch;

    [Header("Dialogue Templates")]
    [TextArea(2, 4)] public string introText = "You see a place that could be worth searching.";
    [TextArea(2, 4)] public string failText = "You search around... and you are unable to find anything of value.";
    [TextArea(2, 4)] public string successHeader = "You search around... and you found:";

    // State Tracking
    private int currentStep = 1;
    private string lineToPrint = "";
    private Coroutine typewriterCoroutine;
    private bool isTyping = false;
    private Vector3 playerDefPos = new Vector3(7777, 0, 0);
    public int nextSceneName = 9;
    // Track rolled items to draw actual UI Sprites
    private List<(Sprite icon, int qty, Color color)> rolledLootVisuals = new List<(Sprite, int, Color)>();

    private void Start()
    {
        Player.Instance.transform.position = playerDefPos;
        if (lootDisplayContainer != null) lootDisplayContainer.gameObject.SetActive(false);

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

            // Immediately display icons if dialogue skipped
            ShowLootIcons();

            if (currentStep == 2) EndSearchEvent();
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

        // 1. Evaluate current biome
        int currentBiome = 1;
        if (Player.Instance != null)
        {
            currentBiome = ((int)Player.Instance.currentBiome);
        }

        // 2. Fetch specific configuration table
        BiomeSearchTable activeConfig = GetConfigForBiome(currentBiome);

        if (activeConfig == null)
        {
            Debug.LogError($"[SearchEvent] No configurations found for Biome {currentBiome} or Fallback!");
            lineToPrint = failText;
            return;
        }

        // 3. Roll for empty hand failure rate
        float outcomeRoll = Random.Range(0f, 100f);

        if (outcomeRoll <= activeConfig.nothingChance || activeConfig.possibleSearchDrops.Count == 0)
        {
            lineToPrint = failText;
        }
        else
        {
            lineToPrint = successHeader;

            // Determine if player finds 1 or 2 unique slots based on biome configuration
            int itemsToDropCount = 1;
            float rarityRoll = Random.Range(0f, 100f);

            if (rarityRoll <= activeConfig.rareTwoItemChance)
            {
                itemsToDropCount = 2;
            }

            // Dictionary merges duplicate items rolled within the same search action
            Dictionary<SearchDrop, int> rolledLootAmounts = new Dictionary<SearchDrop, int>();

            for (int i = 0; i < itemsToDropCount; i++)
            {
                SearchDrop rolledDrop = activeConfig.possibleSearchDrops[Random.Range(0, activeConfig.possibleSearchDrops.Count)];

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

            // 4. Grant loot resources and serialize visual parameters
            foreach (KeyValuePair<SearchDrop, int> lootEntry in rolledLootAmounts)
            {
                SearchDrop drop = lootEntry.Key;
                int finalQuantity = lootEntry.Value;

                if (drop.type == SearchDrop.DropType.Item)
                {
                    if (Player.Instance != null && Player.Instance.inventory != null)
                    {
                        Player.Instance.inventory.AddItem(drop.itemData, finalQuantity);
                    }
                }
                else if (drop.type == SearchDrop.DropType.Tooth)
                {
                    if (Player.Instance != null && Player.Instance.inventory != null)
                    {
                        // Connection point for PlayerInventory tooth values:
                        // Player.Instance.inventory.AddTeeth(finalQuantity);
                    }
                }

                if (drop.Icon != null)
                {
                    rolledLootVisuals.Add((drop.Icon, finalQuantity, drop.Color));
                }
            }
        }
    }

    private BiomeSearchTable GetConfigForBiome(int biomeId)
    {
        foreach (BiomeSearchTable table in biomeSearchTables)
        {
            if (table.targetBiome == biomeId) return table;
        }
        return defaultFallbackSearch;
    }

    private IEnumerator TypeTextRoutine()
    {
        isTyping = true;
        dialogueText.text = "";

        foreach (char letter in lineToPrint.ToCharArray())
        {
            dialogueText.text += letter;
            yield return new WaitForSeconds(typeSpeed);
        }

        isTyping = false;

        // Populate visual layouts cleanly upon natural text termination
        ShowLootIcons();

        if (currentStep == 2) EndSearchEvent();
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

    private void EndSearchEvent()
    {
        if (exitButtonAnimator != null)
        {
            exitButtonAnimator.SetTrigger(animationTriggerName);
        }
    }

    public void ExitSearchSceneButton()
    {
        SceneManager.LoadSceneAsync(nextSceneName);
    }
}

// Data models unified to support mixed item pools inside the Search space
[System.Serializable]
public class BiomeSearchTable
{
    [Tooltip("Which biome number does this profile apply to?")]
    public int targetBiome = 1;

    [Header("Probability Parameters")]
    [Range(0f, 100f)] public float nothingChance = 35f;
    [Range(0f, 100f)] public float rareTwoItemChance = 15f;

    [Header("Drop Volume rules")]
    [Tooltip("Max items returned inside a single drop block stack.")]
    public int maxDropQuantity = 2;

    [Header("Loot Pool Configurations")]
    public List<SearchDrop> possibleSearchDrops = new List<SearchDrop>();
}

[System.Serializable]
public class SearchDrop
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