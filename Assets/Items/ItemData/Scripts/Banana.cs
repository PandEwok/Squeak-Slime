using UnityEngine;

[CreateAssetMenu(fileName = "Banana", menuName = "Items/Banana")]
public class Banana : ItemData
{
    public override void UseItem(GameObject user)
    {
        var inventory = user.GetComponent<PlayerInventory>();
        var playerS = user.GetComponent<Player>();
        if (inventory != null)
        {
            inventory.RemoveItem(this, 1);
            playerS.stats.RestoreSP((int)effectValue);
            Debug.Log("Le joueur a utilise une banane");
        }
    }
}
