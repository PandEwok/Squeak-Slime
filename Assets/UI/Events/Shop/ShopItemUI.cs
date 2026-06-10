using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class ShopItemUI : MonoBehaviour
{
    [HideInInspector] public int currentDynamicPrice;

    [Header("UI Component References")]
    public Image itemImage;
    public TextMeshProUGUI priceText;
    public Image currencyImage;
    public Button purchaseButton;

    [Header("Juice & Animation Settings")]
    public float shakeMagnitude = 12f;
    public float shakeDuration = 0.25f;
    public Color brokenFlashColor = new Color(1f, 0.35f, 0.35f);

    private bool isShaking = false;
    private Color32 permanentPriceColor;

    private float originalFontSize;
    private TextAlignmentOptions originalAlignment;

    private ItemData myItemData;
    private Tooth myToothData; // Updated variable type
    private ShopManager myManager;

    // Accepts a 'Tooth' instead of CurrencyData
    public void SetupShopItem(ItemData item, int price, Tooth tooth, ShopManager manager)
    {
        if (itemImage == null || priceText == null || currencyImage == null || purchaseButton == null)
        {
            Debug.LogError($"[ShopItemUI] CRITICAL: A UI reference is missing on '{gameObject.name}'!", gameObject);
            return;
        }

        myItemData = item;
        currentDynamicPrice = price;
        myToothData = tooth;
        myManager = manager;

        itemImage.sprite = item.itemIcon;
        itemImage.color = item.defaultColor;

        priceText.text = price.ToString();

        // Assigns using your friend's 'itemIcon' and 'defaultColor' fields
        currencyImage.sprite = tooth.itemIcon;
        currencyImage.color = tooth.defaultColor;

        originalFontSize = priceText.fontSize;
        originalAlignment = priceText.alignment;
        permanentPriceColor = priceText.faceColor;

        purchaseButton.interactable = true;
    }

    public void OnPurchaseClicked()
    {
        // Passes the tooth back to the manager
        myManager.AttemptPurchase(this, myItemData, currentDynamicPrice, myToothData);
    }

    public void MarkAsSold()
    {
        purchaseButton.interactable = false;
        priceText.fontSize = 30f;
        priceText.alignment = TextAlignmentOptions.Center;
        priceText.text = "SOLD";

        if (currencyImage != null)
        {
            currencyImage.gameObject.SetActive(false);
        }

        itemImage.color = new Color(0.5f, 0.5f, 0.5f, 0.5f);
    }

    public void PlayBrokeFeedback()
    {
        if (!isShaking)
        {
            StartCoroutine(ShakeAndFlashBrokeRoutine());
        }
    }

    private IEnumerator ShakeAndFlashBrokeRoutine()
    {
        isShaking = true;
        Vector3 originalPosition = transform.localPosition;

        priceText.faceColor = brokenFlashColor;
        priceText.color = brokenFlashColor;

        float elapsed = 0f;
        while (elapsed < shakeDuration)
        {
            float randomX = Random.Range(-1f, 1f) * shakeMagnitude;
            float randomY = Random.Range(-1f, 1f) * (shakeMagnitude * 0.2f);

            transform.localPosition = new Vector3(originalPosition.x + randomX, originalPosition.y + randomY, originalPosition.z);

            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.localPosition = originalPosition;
        priceText.faceColor = permanentPriceColor;
        priceText.color = permanentPriceColor;

        isShaking = false;
    }

    public void ModifyPriceByPetting(int amount, Color permanentColor)
    {
        currentDynamicPrice = Mathf.Max(0, currentDynamicPrice + amount);

        priceText.fontSize = originalFontSize;
        priceText.alignment = originalAlignment;
        priceText.text = currentDynamicPrice.ToString();

        permanentPriceColor = permanentColor;
        priceText.faceColor = permanentColor;
        priceText.color = permanentColor;
    }
}