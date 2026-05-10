using UnityEngine;

public abstract class BaseItem : ScriptableObject
{
    public string itemName;
    public Sprite icon;
    [TextArea] public virtual string description => "A base item";


    public abstract void Use();
}