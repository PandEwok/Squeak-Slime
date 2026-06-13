using UnityEngine;

[CreateAssetMenu(fileName = "Tooth", menuName = "Tooth/Tooth")]
public class Tooth : ScriptableObject
{
    public string itemName;
    public string itemId;
    public int rank; //1, 2 ou 3
    [TextArea(2, 4)] public string itemDescription;
    public Sprite itemIcon;
    public Color defaultColor;
    public int maxStackCapacity = 99;
}
