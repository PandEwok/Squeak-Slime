using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;
using UnityEditor.UIElements;
using UnityEditor.Experimental.GraphView;
public class PlayerInventory : MonoBehaviour
{
    public Dictionary<ItemData, int> itemsPossessed = new Dictionary<ItemData, int>();
    public Dictionary<Tooth, int> teethPossessed = new Dictionary<Tooth, int>();
    [System.Serializable]
    public struct StartingItem
    {
        public ItemData item;
        public int amount;
    }
    [System.Serializable]
    public struct StartingTooth
    {
        public Tooth tooth;
        public int amount;
    }

    [Header("Configuration de l'Inventaire de Départ")]
    [SerializeField] private List<StartingItem> startingInventory;
    [Header("Configuration des Dents de Départ")]
    [SerializeField] private List<StartingTooth> startingTeeth;
    public void AddItem(ItemData item, int amount)
    {
        if(itemsPossessed.ContainsKey(item))
            itemsPossessed[item] += amount;
        else
            itemsPossessed.Add(item, amount);
    }
    public void AddTooth(Tooth tooth, int amount)
    {
        if (teethPossessed.ContainsKey(tooth))
            teethPossessed[tooth] += amount;
        else
            teethPossessed.Add(tooth, amount);
    }
    public void RemoveItem(ItemData item, int amount)
    {
        if (itemsPossessed.ContainsKey(item))
        {
            itemsPossessed [item] -= amount;
            if (itemsPossessed[item] < 0) itemsPossessed.Remove(item);
        }
    }
    public void RemoveTooth(Tooth tooth, int amount)
    {
        if (teethPossessed.ContainsKey(tooth))
        {
            teethPossessed[tooth] -= amount;
            if (teethPossessed[tooth] < 0) teethPossessed.Remove(tooth);
        }
    }
    public void Awake()
    {
        if (startingInventory != null)
        {
            foreach (var starter in startingInventory)
            {
                if (starter.item != null && starter.amount > 0)
                {
                    AddItem(starter.item, starter.amount);
                }
            }
        }

        Debug.Log($"[Inventory] Initialisé avec {itemsPossessed.Count} types d'objets uniques.");

        if (startingTeeth != null)
        {
            foreach (var starter in startingTeeth)
            {
                if (starter.tooth != null && starter.amount > 0)
                {
                    AddTooth(starter.tooth, starter.amount);
                }
            }
        }

        Debug.Log($"[Inventory] Initialisé avec {teethPossessed.Count} types de dents uniques.");
    }

    public void LoadInventoryData(PlayerData data)
    {
        if (data == null) return;

        itemsPossessed.Clear();
        teethPossessed.Clear();

        ItemData[] allItems = Resources.LoadAll<ItemData>("Items");
        foreach (var itemSave in data.items)
        {
            ItemData match = System.Array.Find(allItems, x => x.itemId == itemSave.itemId);
            if (match != null)
            {
                AddItem(match, itemSave.amount);
            }
            else
            {
                Debug.LogWarning($"[Inventory] Impossible de trouver le ScriptableObject Item avec l'ID: {itemSave.itemId}");
            }
        }

        Tooth[] allTeeth = Resources.LoadAll<Tooth>("Teeth");
        foreach (var toothSave in data.teeth)
        {
            Tooth match = System.Array.Find(allTeeth, x => x.itemId == toothSave.itemId);
            if (match != null)
            {
                AddTooth(match, toothSave.amount);
            }
            else
            {
                Debug.LogWarning($"[Inventory] Impossible de trouver le ScriptableObject Tooth avec l'ID: {toothSave.itemId}");
            }
        }
    }
}