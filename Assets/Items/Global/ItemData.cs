using UnityEngine;

[CreateAssetMenu(fileName = "New Action Data", menuName = "Inventory/Action Data")]
public class ItemData : ScriptableObject
{
    public enum ActionType { Item, Skill }

    [Header("--- General Settings ---")]
    public ActionType actionType;
    public string itemId;
    public string itemName;
    [TextArea(2, 4)] public string itemDescription;
    public int effectValue = 50;

    [Header("--- Item Only Configuration ---")]
    [Tooltip("Leave blank or ignore if this is a Skill!")]
    public Sprite itemIcon;
    public Color defaultColor = Color.white;
    public int maxStackCapacity = 99;

    [Header("--- Skill Only Configuration ---")]
    [Tooltip("The amount of SP required to execute this skill.")]
    public int spCost;
    [Tooltip("Drag a unique .cs script file here to handle complex math/animations.")]
    public CustomEffectLogic specialEffectLogic;
}