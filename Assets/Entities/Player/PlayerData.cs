using System.Collections.Generic;

[System.Serializable]
public class PlayerData
{
    public int HP;
    public int SP;
    public bool hasBite;
    public bool hasFireball;
    public bool hasFracture;
    public bool hasAbsorption;

    public List<ItemSaveData> items = new List<ItemSaveData>();
    public List<ToothSaveData> teeth = new List<ToothSaveData>();
}

[System.Serializable]
public struct ItemSaveData
{
    public string itemId;
    public int amount;
}

[System.Serializable]
public struct ToothSaveData
{
    public string itemId;
    public int amount;
}