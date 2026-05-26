using UnityEngine;
using System.Collections.Generic;

public class PlayerInventory : MonoBehaviour
{
    [Header("Modular Inventory System Backend")]
    [Tooltip("Drop your created ItemData ScriptableObjects here so the inventory knows they exist at startup.")]
    public List<ItemData> masterItemCatalog = new List<ItemData>();

    // The master dictionary tracking item quantities dynamically by ID string
    private Dictionary<string, int> itemDatabase = new Dictionary<string, int>();

    public enum TeethType { Normal, Magic, Golem, Mutant }
    [Header("Currency / Teeth Settings")]
    private Dictionary<TeethType, int> teethDatabase = new Dictionary<TeethType, int>();

    private void Awake()
    {
        InitializeInventoryBackend();
    }

    private void InitializeInventoryBackend()
    {
        // Register all items from our catalog into our runtime data dictionary
        foreach (ItemData item in masterItemCatalog)
        {
            if (item != null && !itemDatabase.ContainsKey(item.itemId))
            {
                itemDatabase.Add(item.itemId, 0);
            }
        }

        // Pre-populate our currency teeth counts
        foreach (TeethType type in System.Enum.GetValues(typeof(TeethType)))
        {
            teethDatabase[type] = 0;
        }
    }

    // =========================================================================
    // NEW UNIVERSAL MODULAR METHODS (Use these going forward!)
    // =========================================================================

    public void AddItem(string itemId, int amount)
    {
        if (!itemDatabase.ContainsKey(itemId))
        {
            // Safety fallback if an asset wasn't manually added to the catalog inspector list
            itemDatabase[itemId] = 0;
        }

        int maxLimit = 99;
        ItemData data = masterItemCatalog.Find(x => x.itemId == itemId);
        if (data != null) maxLimit = data.maxStackCapacity;

        itemDatabase[itemId] = Mathf.Clamp(itemDatabase[itemId] + amount, 0, maxLimit);
    }

    public void RemoveItem(string itemId, int amount)
    {
        if (itemDatabase.ContainsKey(itemId))
        {
            itemDatabase[itemId] = Mathf.Max(0, itemDatabase[itemId] - amount);
        }
    }

    public int GetItemQuantity(string itemId)
    {
        return itemDatabase.ContainsKey(itemId) ? itemDatabase[itemId] : 0;
    }

    public void AddTeeth(TeethType type, int amount)
    {
        teethDatabase[type] = Mathf.Clamp(teethDatabase[type] + amount, 0, 99);
    }

    public void RemoveTeeth(TeethType type, int amount)
    {
        teethDatabase[type] = Mathf.Max(0, teethDatabase[type] - amount);
    }


    // =========================================================================
    // BACKWARD COMPATIBILITY BRIDGE (Updated with Setters!)
    // =========================================================================

    public int cheeseInv
    {
        get => GetItemQuantity("Cheese");
        set => itemDatabase["Cheese"] = Mathf.Clamp(value, 0, 99);
    }
    public int bananaInv
    {
        get => GetItemQuantity("Banana");
        set => itemDatabase["Banana"] = Mathf.Clamp(value, 0, 99);
    }
    public int pepperAttInv
    {
        get => GetItemQuantity("PepperAtt");
        set => itemDatabase["PepperAtt"] = Mathf.Clamp(value, 0, 99);
    }
    public int pepperDefInv
    {
        get => GetItemQuantity("PepperDef");
        set => itemDatabase["PepperDef"] = Mathf.Clamp(value, 0, 99);
    }

    public int qty_teeth
    {
        get => teethDatabase[TeethType.Normal];
        set => teethDatabase[TeethType.Normal] = Mathf.Clamp(value, 0, 99);
    }
    public int qty_magic_teeth
    {
        get => teethDatabase[TeethType.Magic];
        set => teethDatabase[TeethType.Magic] = Mathf.Clamp(value, 0, 99);
    }
    public int qty_golem_teeth
    {
        get => teethDatabase[TeethType.Golem];
        set => teethDatabase[TeethType.Golem] = Mathf.Clamp(value, 0, 99);
    }
    public int qty_mutant_teeth
    {
        get => teethDatabase[TeethType.Mutant];
        set => teethDatabase[TeethType.Mutant] = Mathf.Clamp(value, 0, 99);
    }

    public void addCheese(int amount) => AddItem("Cheese", amount);
    public void removeCheese(int amount) => RemoveItem("Cheese", amount);

    public void addBanana(int amount) => AddItem("Banana", amount);
    public void removeBanana(int amount) => RemoveItem("Banana", amount);

    public void addPepperAtt(int amount) => AddItem("PepperAtt", amount);
    public void removePepperAtt(int amount) => RemoveItem("PepperAtt", amount);

    public void addPepperDef(int amount) => AddItem("PepperDef", amount);
    public void removePepperDef(int amount) => RemoveItem("PepperDef", amount);

    // =========================================================================
    // DEBUG & TESTING CHEATS
    // =========================================================================
    [ContextMenu("Cheat Menu/Give 10 of Every Item")]
    public void GiveAllItemsCheat()
    {
        if (masterItemCatalog == null || masterItemCatalog.Count == 0)
        {
            Debug.LogWarning("Master catalog is empty! Drag your item assets into the PlayerInventory inspector.");
            return;
        }

        Debug.Log("<color=cyan><b>[CHEAT] Injected 10 of every item for combat testing!</b></color>");

        foreach (ItemData item in masterItemCatalog)
        {
            AddItem(item.itemId, 10);
        }
    }
}