using UnityEngine;

public abstract class ItemData : ScriptableObject
{
    public string itemName;
    public string itemId;
    [TextArea(2, 4)] public string itemDescription;
    public Sprite itemIcon;
    public Color defaultColor;
    public float effectValue; //Pour piments, 0,5 = 50% en plus
    public int effectDuration; //Nb tours NOTE: +1 pour attaque
    public int maxStackCapacity = 99;


    public abstract void UseItem(GameObject user);

}
