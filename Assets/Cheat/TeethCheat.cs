using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using static PlayerInventory;

public class TeethCheat : MonoBehaviour
{
    bool toggleCheat = true;
    [System.Serializable]
    public struct Teeth
    {
        public Tooth tooth;
        public int amount;
    }
    private List<Teeth> teeth;



    public void Setup(List<Teeth> itemsToGive)
    {
        this.teeth = itemsToGive;
    }
    void Update()
    {
        if (Keyboard.current != null && Keyboard.current.tKey.wasPressedThisFrame)
        {
            if (Player.Instance.inventory != null)
            {
                if (toggleCheat)
                {
                    if (teeth != null)
                    {
                        foreach (var starter in teeth)
                        {
                            if (starter.tooth != null && starter.amount > 0)
                            {
                                Player.Instance.inventory.AddTooth(starter.tooth, starter.amount);
                            }
                        }
                    }

                    Debug.Log("Teeth cheat activated");
                    Player.Instance.inventory.ClearCurrentRunTeeth();
                    Player.Instance.inventory.ClearCurrentBattleTeeth();
                    toggleCheat = false;
                }
                else
                {
                    Player.Instance.inventory.teethPossessed.Clear();
                    toggleCheat = true;
                    Debug.Log("Teeth cheat disabled");
                }
            }
        }

        
    }
}
