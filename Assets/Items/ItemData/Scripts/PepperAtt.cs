using UnityEngine;

[CreateAssetMenu(fileName = "PepperAtt", menuName = "Items/PepperAtt")]
public class PepperAtt : ItemData
{
    public override void UseItem(GameObject user)
    {
        var inventory = user.GetComponent<PlayerInventory>();
        var playerS = user.GetComponent<PlayerScript>();
        if (inventory != null)
        {
            inventory.RemoveItem(this, 1);
            playerS.ActionEmpower(effectDuration, effectValue);
            Debug.Log("Le joueur a utilise un piment d'attaque");
        }
    }
}
