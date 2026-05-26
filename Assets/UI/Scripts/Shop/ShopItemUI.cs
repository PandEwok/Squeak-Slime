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
    [Tooltip("How violent the shake is. 10 to 15 is usually a sweet spot for UI!")]
    public float shakeMagnitude = 12f;
    [Tooltip("How long the shake lasts in seconds.")]
    public float shakeDuration = 0.25f;

    [Tooltip("The unique color used ONLY when the player doesn't have enough currency.")]
    public Color brokenFlashColor = new Color(1f, 0.35f, 0.35f);

    private bool isShaking = false;
    private Color32 permanentPriceColor;

    // NEW: Variables to store your exact original inspector font settings automatically
    private float originalFontSize;
    private TextAlignmentOptions originalAlignment;

    // Hidden variables to remember what this slot is selling
    private ItemData myItemData;
    private CurrencyData myCurrencyData;
    private ShopManager myManager;

    public void SetupShopItem(ItemData item, int price, CurrencyData currency, ShopManager manager)
    {
        if (itemImage == null || priceText == null || currencyImage == null || purchaseButton == null)
        {
            Debug.LogError($"[ShopItemUI] CRITICAL: A UI reference is missing on the prefab '{gameObject.name}'!", gameObject);
            return;
        }

        myItemData = item;
        currentDynamicPrice = price;
        myCurrencyData = currency;
        myManager = manager;

        itemImage.sprite = item.itemIcon;
        itemImage.color = item.defaultColor;

        priceText.text = price.ToString();

        currencyImage.sprite = currency.currencyIcon;
        currencyImage.color = currency.defaultColor;

        // NEW: Capture your exact Inspector settings automatically at startup!
        originalFontSize = priceText.fontSize;
        originalAlignment = priceText.alignment;
        permanentPriceColor = priceText.faceColor;

        purchaseButton.interactable = true;
    }

    public void OnPurchaseClicked()
    {
        myManager.AttemptPurchase(this, myItemData, currentDynamicPrice, myCurrencyData);
    }

    public void MarkAsSold()
    {
        purchaseButton.interactable = false;

        priceText.fontSize = 30f; // Font size specifically for "SOLD"
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

        if (currentDynamicPrice == 0)
        {
            priceText.fontSize = 18f; // Scale down ONLY for FREE text
            priceText.alignment = TextAlignmentOptions.Center;
            priceText.text = "FREE";
        }
        else
        {
            // NEW: Reverts perfectly back to your exact inspector layout settings!
            priceText.fontSize = originalFontSize;
            priceText.alignment = originalAlignment;
            priceText.text = currentDynamicPrice.ToString();
        }

        permanentPriceColor = permanentColor;
        priceText.faceColor = permanentColor;
        priceText.color = permanentColor;
    }
}