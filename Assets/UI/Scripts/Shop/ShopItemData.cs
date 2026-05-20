using UnityEngine;

[CreateAssetMenu(fileName = "New Shop Item", menuName = "Shop/Base Item")]
public class ShopItemData : ScriptableObject
{
    public string itemName;
    public Sprite itemIcon;
    public Color defaultColor = Color.white; // Provides an inspector color wheel!
}