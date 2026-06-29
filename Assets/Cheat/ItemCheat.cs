using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using static PlayerInventory;

public class ItemCheat : MonoBehaviour
{
    bool toggleCheat = true;
    [System.Serializable]

    public struct Item
    {
        public ItemData item;
        public int amount;
    }
    
    private List<Item> items;


    public void Setup(List<Item> itemsToGive)
    {
        this.items = itemsToGive;
    }

    void Update()
    {
        if (Keyboard.current != null && Keyboard.current.iKey.wasPressedThisFrame)
        {
            if (Player.Instance.inventory != null)
            {
                if (toggleCheat)
                {
                    if (items != null)
                    {
                        foreach (var starter in items)
                        {
                            if (starter.item != null && starter.amount > 0)
                            {
                                Player.Instance.inventory.AddItem(starter.item, starter.amount);
                            }
                        }
                    }

                    Debug.Log("Item cheat activated");
                    toggleCheat = false;
                }
                else
                {
                    Player.Instance.inventory.itemsPossessed.Clear();
                    toggleCheat = true;
                    Debug.Log("Item cheat disabled");
                }
            }
        }
    }
}
