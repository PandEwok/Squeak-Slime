using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using System.Collections;

public class InventorySlotUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [Header("Component References")]
    public Image itemIconImage;
    public TextMeshProUGUI nameAndQuantityText;
    public TextMeshProUGUI descriptionText;

    [Header("Tooltip Reference")]
    public TextMeshProUGUI tooltipText;

    [Header("Fallback Placeholder")]
    public Sprite genericMysteryIcon;

    // Internal slot state tracking
    private bool isCollected = false;
    private int currentQuantity = 0;
    private int mySlotIndex = -1;
    private EventInventoryUIController mainController;
    private bool hasAssetAssigned = false;

    // Cached visual strings for the hover tooltip systems
    private string cachedName = "???";
    private string cachedDescription = "???";
    private string cachedId = "";

    // Persistent memory for this specific UI slot
    private bool wasDiscovered = false;

    // Coroutine tracking to prevent multiple timers from clashing
    private Coroutine feedbackCoroutine;
    private bool isDisplayingFeedback = false;

    // FIXED: The Manager passes all graphic properties here dynamically!
    public void UpdateSlotDisplay(int quantity, int index, string assetName, string assetDesc, string assetId, Sprite assetIcon, Color assetColor, bool hasAsset, EventInventoryUIController controller)
    {
        currentQuantity = quantity;
        mySlotIndex = index;
        mainController = controller;
        hasAssetAssigned = hasAsset;

        cachedName = assetName;
        cachedDescription = assetDesc;
        cachedId = assetId;

        if (quantity > 0)
        {
            wasDiscovered = true;
        }

        // ==========================================
        // STATE 1: NO ASSET FILE ASSIGNED IN MANAGER
        // ==========================================
        if (!hasAssetAssigned)
        {
            isCollected = false;
            if (itemIconImage != null)
            {
                if (genericMysteryIcon != null) itemIconImage.sprite = genericMysteryIcon;
                itemIconImage.color = Color.white;
            }
            if (nameAndQuantityText != null) nameAndQuantityText.text = "???";
            if (descriptionText != null) descriptionText.text = "???";
            return;
        }

        // ==========================================
        // STATE 2: ACTIVE (Quantity > 0)
        // ==========================================
        if (quantity > 0)
        {
            isCollected = true;
            if (itemIconImage != null)
            {
                itemIconImage.sprite = assetIcon;
                itemIconImage.color = assetColor;
            }
            if (nameAndQuantityText != null) nameAndQuantityText.text = $"{cachedName} x{quantity}";
            if (descriptionText != null) descriptionText.text = cachedDescription;
        }
        // ==========================================
        // STATE 3: RUN OUT / EMPTY BUT PREVIOUSLY SEEN
        // ==========================================
        else if (wasDiscovered)
        {
            isCollected = false;

            if (itemIconImage != null)
            {
                itemIconImage.sprite = assetIcon;
                itemIconImage.color = Color.gray; // Grayed out
            }
            if (nameAndQuantityText != null) nameAndQuantityText.text = $"{cachedName} x0";
            if (descriptionText != null) descriptionText.text = cachedDescription;
        }
        // ==========================================
        // STATE 4: TOTAL MYSTERY
        // ==========================================
        else
        {
            isCollected = false;
            if (itemIconImage != null)
            {
                itemIconImage.sprite = assetIcon;
                itemIconImage.color = Color.white; // Plain white silhouette
            }
            if (nameAndQuantityText != null) nameAndQuantityText.text = "???";
            if (descriptionText != null) descriptionText.text = "???";
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!isCollected || currentQuantity <= 0 || !hasAssetAssigned) return;

        // Uses our clean cached data values safely
        bool isPepper = cachedName.ToLower().Contains("pepper") || cachedId.ToLower().Contains("pepper");

        if (isPepper)
        {
            Debug.LogWarning($"[Inventory] Cannot consume {cachedName} outside of combat!");
            return;
        }

        if (mainController != null && mySlotIndex != -1)
        {
            mainController.ConsumeItemFromSlot(mySlotIndex);
        }

        if (feedbackCoroutine != null) StopCoroutine(feedbackCoroutine);
        feedbackCoroutine = StartCoroutine(DisplayConsumptionFeedback());
    }

    private IEnumerator DisplayConsumptionFeedback()
    {
        isDisplayingFeedback = true;
        if (tooltipText != null) tooltipText.text = $"You consumed {cachedName}";

        yield return new WaitForSeconds(2.5f);
        isDisplayingFeedback = false;

        if (EventSystem.current.IsPointerOverGameObject() && tooltipText != null)
        {
            UpdateHoverText();
        }
        else if (tooltipText != null)
        {
            tooltipText.text = "";
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (isDisplayingFeedback) return;
        UpdateHoverText();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (isDisplayingFeedback) return;
        if (tooltipText != null) tooltipText.text = "";
    }

    private void UpdateHoverText()
    {
        if (tooltipText == null) return;

        if (!hasAssetAssigned)
        {
            tooltipText.text = "Not yet discovered.";
            return;
        }

        if (currentQuantity > 0)
        {
            bool isPepper = cachedName.ToLower().Contains("pepper") || cachedId.ToLower().Contains("pepper");
            tooltipText.text = isPepper ? "" : $"Use {cachedName}";
        }
        else if (wasDiscovered)
        {
            tooltipText.text = $"You do not have any {cachedName}";
        }
        else
        {
            tooltipText.text = "Not yet discovered.";
        }
    }
}