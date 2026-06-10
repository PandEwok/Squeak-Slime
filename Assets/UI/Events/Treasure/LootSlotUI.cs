using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LootSlotUI : MonoBehaviour
{
    public Image iconImage;
    public TextMeshProUGUI quantityText;

    // Added a color parameter to match your ItemData / Tooth defaults
    public void SetupSlot(Sprite icon, int quantity, Color itemColor)
    {
        iconImage.sprite = icon;

        // Apply the color from your Scriptable Object directly to the UI Image
        iconImage.color = itemColor;

        // Changed this line so it ALWAYS shows the quantity, even if it's 1
        quantityText.text = $"x{quantity}";

        gameObject.SetActive(true);
    }
}