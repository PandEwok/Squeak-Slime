using UnityEngine;

[CreateAssetMenu(fileName = "Banana", menuName = "Items/Banana")]
public class Banana : ItemData
{
    public override void UseItem(GameObject user)
    {
        var inventory = user.GetComponent<PlayerInventory>();
        var playerS = user.GetComponent<PlayerScript>();
        if (inventory != null)
        {
            inventory.RemoveItem(this, 1);
            playerS.RestoreSP(effectValue);
            Debug.Log("Le joueur a utilise une banane");
        }
    }
}
