using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct CurrencyPriceConfig
{
    public CurrencyData currency;
    public int minPrice;
    public int maxPrice;
}

[System.Serializable]
public class FloorProgressionConfig
{
    public string floorName;
    public List<CurrencyPriceConfig> availableCurrencies;

    [Header("Shop Layout Luck Settings")]
    [Tooltip("The absolute minimum slots this shop will ever roll on this floor.")]
    public int absoluteMinSlots = 1;

    [Tooltip("The standard maximum slots on Stage 1.")]
    public int stage1MaxSlots = 3;

    [Tooltip("How many extra potential slots can be unlocked by Stage 6 luck?")]
    public int extraMaxSlotsAtEnd = 2; // e.g., if stage1 max is 3, Stage 6 max could reach 5 (3 + 2)
}