using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;

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
    private bool isToothSlot = false;

    // Cached visual strings for the hover tooltip systems
    private string cachedName = "???";
    private string cachedDescription = "???";
    private string cachedId = "";

    private bool wasDiscovered = false;

    // SOLUTION: A static session registry that remembers discoveries without altering core inventories
    private static HashSet<string> sessionDiscoveredItemIds = new HashSet<string>();

    // Coroutine tracking to prevent multiple timers from clashing
    private Coroutine feedbackCoroutine;
    private bool isDisplayingFeedback = false;

    public void UpdateSlotDisplay(int quantity, int index, string assetName, string assetDesc, string assetId, Sprite assetIcon, Color assetColor, bool hasAsset, bool isTooth, EventInventoryUIController controller)
    {
        currentQuantity = quantity;
        mySlotIndex = index;
        mainController = controller;
        hasAssetAssigned = hasAsset;
        isToothSlot = isTooth;

        cachedName = assetName;
        cachedDescription = assetDesc;
        cachedId = assetId;

        // If we have 1 or more, permanently log it in the session memory
        if (quantity > 0 && !string.IsNullOrEmpty(cachedId))
        {
            sessionDiscoveredItemIds.Add(cachedId);
        }

        // Read discovery state directly from our clean session memory
        if (!string.IsNullOrEmpty(cachedId) && sessionDiscoveredItemIds.Contains(cachedId))
        {
            wasDiscovered = true;
        }

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

        // STATE 2: ACTIVE (Quantity > 0)
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
        // STATE 3: RUN OUT / EMPTY BUT PREVIOUSLY SEEN (Grayed out x0)
        else if (wasDiscovered)
        {
            isCollected = false;

            if (itemIconImage != null)
            {
                itemIconImage.sprite = assetIcon;
                itemIconImage.color = Color.gray;
            }
            if (nameAndQuantityText != null) nameAndQuantityText.text = $"{cachedName} x0";
            if (descriptionText != null) descriptionText.text = cachedDescription;
        }
        // STATE 4: TOTAL MYSTERY (Never found)
        else
        {
            isCollected = false;
            if (itemIconImage != null)
            {
                itemIconImage.sprite = assetIcon;
                itemIconImage.color = Color.white;
            }
            if (nameAndQuantityText != null) nameAndQuantityText.text = "???";
            if (descriptionText != null) descriptionText.text = "???";
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!hasAssetAssigned || isToothSlot) return;

        // If we click an item that is already empty but discovered, show out-of-stock info immediately
        if (currentQuantity <= 0)
        {
            if (wasDiscovered)
            {
                if (feedbackCoroutine != null) StopCoroutine(feedbackCoroutine);
                feedbackCoroutine = StartCoroutine(DisplayOutofStockFeedback());
            }
            return;
        }

        bool isPepper = cachedName.ToLower().Contains("pepper") || cachedId.ToLower().Contains("pepper");

        if (isPepper)
        {
            Debug.LogWarning($"[Inventory] Cannot consume {cachedName} outside of combat!");
            return;
        }

        if (mainController != null && mySlotIndex != -1)
        {
            mainController.ConsumeItemFromSlot(mySlotIndex);

            // FIX: Manually decrement the count and update visual labels instantly
            currentQuantity--;
            if (currentQuantity <= 0)
            {
                currentQuantity = 0;
                isCollected = false;
                if (itemIconImage != null) itemIconImage.color = Color.gray;
                if (nameAndQuantityText != null) nameAndQuantityText.text = $"{cachedName} x0";

                if (!string.IsNullOrEmpty(cachedId))
                {
                    sessionDiscoveredItemIds.Add(cachedId);
                    wasDiscovered = true;
                }
            }
            else
            {
                if (nameAndQuantityText != null) nameAndQuantityText.text = $"{cachedName} x{currentQuantity}";
            }
        }

        if (feedbackCoroutine != null) StopCoroutine(feedbackCoroutine);
        feedbackCoroutine = StartCoroutine(DisplayConsumptionFeedback());
    }

    private IEnumerator DisplayConsumptionFeedback()
    {
        isDisplayingFeedback = true;

        // Evaluates against the newly sync'd UI count state instantly
        if (currentQuantity <= 0)
        {
            if (tooltipText != null) tooltipText.text = $"You do not have any {cachedName}";
            isCollected = false;
        }
        else
        {
            if (tooltipText != null) tooltipText.text = $"You consumed {cachedName}";
        }

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

    private IEnumerator DisplayOutofStockFeedback()
    {
        isDisplayingFeedback = true;
        if (tooltipText != null) tooltipText.text = $"You do not have any {cachedName}";

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

        if (isToothSlot)
        {
            tooltipText.text = "";
            return;
        }

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