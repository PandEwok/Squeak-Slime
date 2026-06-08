using UnityEngine;

[CreateAssetMenu(fileName = "PepperDef", menuName = "Items/PepperDef")]
public class PepperDef : ItemData
{
    public override void UseItem(GameObject user)
    {
        var inventory = user.GetComponent<PlayerInventory>();
        var playerS = user.GetComponent<Player>();
        if (inventory != null)
        {
            inventory.RemoveItem(this, 1);
            playerS.ActionDefenseBuff(effectDuration, effectValue);
            Debug.Log("Le joueur a utilise un piment de defense");
        }
    }
}
