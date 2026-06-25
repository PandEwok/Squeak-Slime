using UnityEngine;

[CreateAssetMenu(fileName = "Cheese", menuName = "Items/Cheese")]
public class Cheese : ItemData
{
    public override void UseItem(GameObject user)
    {
        var inventory = user.GetComponent<PlayerInventory>();
        var playerS = user.GetComponent<Player>();
        if (inventory != null)
        {
            inventory.RemoveItem(this, 1);
            playerS.stats.Heal((int)effectValue);
            Debug.Log("Le joueur a utilise un fromage");
           
        }
    }
}
