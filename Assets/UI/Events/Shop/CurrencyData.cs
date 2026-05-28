using UnityEngine;

[CreateAssetMenu(fileName = "New Currency", menuName = "Shop/Currency")]
public class CurrencyData : ScriptableObject
{
    public string currencyName;
    public Sprite currencyIcon;
    public Color defaultColor = Color.white;

    [Header("Progression Rank")]
    [Range(1, 4)]
    [Tooltip("Rank 1 = Floor 1 (Ordinary), Rank 2 = Floor 2 (Golem), etc.")]
    public int toothRank = 1;
}