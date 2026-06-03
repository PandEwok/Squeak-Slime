using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;
using UnityEditor.UIElements;
using UnityEditor.Experimental.GraphView;
public class PlayerInventory : MonoBehaviour
{
    public Dictionary<ItemData, int> itemsPossessed = new Dictionary<ItemData, int>();
    [System.Serializable]
    public struct StartingItem
    {
        public ItemData item;
        public int amount;
    }

    [Header("Configuration de l'Inventaire de Départ")]
    [SerializeField] private List<StartingItem> startingInventory;
    public void AddItem(ItemData item, int amount)
    {
        if(itemsPossessed.ContainsKey(item))
            itemsPossessed[item] += amount;
        else
            itemsPossessed.Add(item, amount);
    }
    public void RemoveItem(ItemData item, int amount)
    {
        if (itemsPossessed.ContainsKey(item))
        {
            itemsPossessed [item] -= amount;
            if (itemsPossessed[item] < 0) itemsPossessed.Remove(item);
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
    }
    //public int cheeseInv = 0;
    //public int bananaInv = 0;
    //public int pepperAttInv = 0;
    //public int pepperDefInv = 0;
    //public enum TeethType
    //{
    //    Normal,
    //    Magic,
    //    Golem,
    //    Mutant
    //}
    //public int qty_teeth = 0;
    //public int qty_magic_teeth = 0;
    //public int qty_golem_teeth = 0;
    //public int qty_mutant_teeth = 0;
    //
    //public void addCheese(int amount)
    //{
    //    cheeseInv += amount;
    //    if (cheeseInv > 99)
    //    {
    //        cheeseInv = 99;
    //    }
    //}
    //public void removeCheese(int amount)
    //{
    //    cheeseInv -= amount;
    //    if (cheeseInv < 0)
    //    {
    //        cheeseInv = 0;
    //    }
    //}
    //public void addBanana(int amount)
    //{
    //    bananaInv += amount;
    //    if (bananaInv > 99)
    //    {
    //        bananaInv = 99;
    //    }
    //}
    //public void removeBanana(int amount)
    //{
    //    bananaInv -= amount;
    //    if (bananaInv < 0)
    //    {
    //        bananaInv = 0;
    //    }
    //}
    //public void addPepperAtt(int amount)
    //{
    //    pepperAttInv += amount;
    //    if (pepperAttInv > 99)
    //    {
    //        pepperAttInv = 99;
    //    }
    //}
    //
    //public void removePepperAtt(int amount)
    //{
    //    pepperAttInv -= amount;
    //    if (pepperAttInv < 0)
    //    {
    //        pepperAttInv = 0;
    //    }
    //}
    //public void addPepperDef(int amount)
    //{
    //    pepperDefInv += amount;
    //    if (pepperDefInv > 99)
    //    {
    //        pepperDefInv = 99;
    //    }
    //}
    //
    //public void removePepperDef(int amount)
    //{
    //    pepperDefInv -= amount;
    //    if (pepperDefInv < 0)
    //    {
    //        pepperDefInv = 0;
    //    }
    //}
    //
    //public void AddTeeth(TeethType type, int amount)
    //{
    //    switch (type)
    //    {
    //        case TeethType.Normal:
    //            qty_teeth += amount;
    //            if (qty_teeth > 99)
    //            {
    //                qty_teeth = 99;
    //            }
    //            break;
    //        case TeethType.Magic:
    //            qty_magic_teeth += amount;
    //            if (qty_magic_teeth > 99)
    //            {
    //                qty_magic_teeth = 99;
    //            }
    //            break;
    //        case TeethType.Golem:
    //            qty_golem_teeth += amount;
    //            if (qty_golem_teeth > 99)
    //            {
    //                qty_golem_teeth = 99;
    //            }
    //            break;
    //        case TeethType.Mutant:
    //            qty_mutant_teeth += amount;
    //            if (qty_mutant_teeth > 99)
    //            {
    //                qty_mutant_teeth = 99;
    //            }
    //            break;
    //        default:
    //            break;
    //    }
    //}
    //
    //public void RemoveTeeth(TeethType type, int amount)
    //{
    //    switch (type)
    //    {
    //        case TeethType.Normal:
    //            qty_teeth -= amount;
    //            if (qty_teeth < 0)
    //            {
    //                qty_teeth = 0;
    //            }
    //            break;
    //        case TeethType.Magic:
    //            qty_magic_teeth -= amount;
    //            if (qty_magic_teeth < 0)
    //            {
    //                qty_magic_teeth = 0;
    //            }
    //            break;
    //        case TeethType.Golem:
    //            qty_golem_teeth -= amount;
    //            if (qty_golem_teeth < 0)
    //            {
    //                qty_golem_teeth = 0;
    //            }
    //            break;
    //        case TeethType.Mutant:
    //            qty_mutant_teeth -= amount;
    //            if (qty_mutant_teeth < 0)
    //            {
    //                qty_mutant_teeth = 0;
    //            }
    //            break;
    //        default:
    //            break;
    //    }
    //}
}