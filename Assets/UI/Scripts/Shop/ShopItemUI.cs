using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ShopItemUI : MonoBehaviour
{
    [Header("UI Component References")]
    public Image itemImage;
    public TextMeshProUGUI priceText;
    public Image currencyImage;

    // The manager passes calculated runtime values straight to the layout here
    public void SetupShopItem(Sprite itemSprite, Color itemColor, int price, Sprite currencySprite, Color currencyColor)
    {
        itemImage.sprite = itemSprite;
        itemImage.color = itemColor; // Tints the item!

        priceText.text = price.ToString();

        currencyImage.sprite = currencySprite;
        currencyImage.color = currencyColor; // Tints the required currency item!
    }
}