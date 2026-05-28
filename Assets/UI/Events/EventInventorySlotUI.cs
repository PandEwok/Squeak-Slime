using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using System.Collections;

public class InventorySlotUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [Header("Item Configuration")]
    public ItemData assignedItem;

    [Header("Component References")]
    public Image itemIconImage;
    public TextMeshProUGUI nameAndQuantityText;
    public TextMeshProUGUI descriptionText;

    [Header("Tooltip Reference")]
    public TextMeshProUGUI tooltipText;

    [Header("Fallback Placeholder")]
    public Sprite genericMysteryIcon;

    // Track the slot's current data internally
    private bool isCollected = false;
    private int currentQuantity = 0;
    private int mySlotIndex = -1;
    private EventInventoryUIController mainController;

    // NEW: Persistent memory for this specific UI slot
    private bool wasDiscovered = false;

    // Coroutine tracking to prevent multiple timers from clashing
    private Coroutine feedbackCoroutine;
    private bool isDisplayingFeedback = false;

    public void UpdateSlotDisplay(int quantity, int index, EventInventoryUIController controller)
    {
        currentQuantity = quantity;
        mySlotIndex = index;
        mainController = controller;

        // If the player ever holds more than 0, flip the discovery switch forever
        if (quantity > 0)
        {
            wasDiscovered = true;
        }

        // ==========================================
        // STATE 1: NO ASSET FILE CREATED YET
        // ==========================================
        if (assignedItem == null)
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
                itemIconImage.sprite = assignedItem.itemIcon;
                itemIconImage.color = assignedItem.defaultColor;
            }
            if (nameAndQuantityText != null) nameAndQuantityText.text = $"{assignedItem.itemName} x{quantity}";
            if (descriptionText != null) descriptionText.text = assignedItem.itemDescription;
        }
        // ==========================================
        // STATE 3: RUN OUT / EMPTY BUT PREVIOUSLY SEEN (NEW!)
        // ==========================================
        else if (wasDiscovered)
        {
            isCollected = false; // Player can't click to consume it anymore

            if (itemIconImage != null)
            {
                itemIconImage.sprite = assignedItem.itemIcon;
                itemIconImage.color = Color.gray; // Grayed out style
            }
            if (nameAndQuantityText != null)
            {
                nameAndQuantityText.text = $"{assignedItem.itemName} x0"; // Keeps real name visible
            }
            if (descriptionText != null)
            {
                descriptionText.text = assignedItem.itemDescription; // Keeps real description visible
            }
        }
        // ==========================================
        // STATE 4: TOTAL MYSTERY (Quantity is 0 and never seen before)
        // ==========================================
        else
        {
            isCollected = false;
            if (itemIconImage != null)
            {
                itemIconImage.sprite = assignedItem.itemIcon;
                itemIconImage.color = Color.white; // Plain un-tinted mystery silhouette
            }
            if (nameAndQuantityText != null) nameAndQuantityText.text = "???";
            if (descriptionText != null) descriptionText.text = "???";
        }
    }

    // ==========================================
    // CLICK DETECTION & CONSUMPTION 
    // ==========================================
    public void OnPointerClick(PointerEventData eventData)
    {
        if (!isCollected || currentQuantity <= 0 || assignedItem == null) return;

        bool isPepper = assignedItem.itemName.ToLower().Contains("pepper") ||
                        assignedItem.itemId.ToLower().Contains("pepper");

        if (isPepper)
        {
            Debug.LogWarning($"[Inventory] Cannot consume {assignedItem.itemName} outside of combat!");
            return;
        }

        if (assignedItem.actionType == ItemData.ActionType.Item)
        {
            Debug.Log($"<color=green>[PLAYER STATS]</color> Consumed {assignedItem.itemName}! Restored {assignedItem.effectValue} HP.");
        }
        else if (assignedItem.actionType == ItemData.ActionType.Skill)
        {
            Debug.Log($"<color=cyan>[PLAYER STATS]</color> Cast {assignedItem.itemName}! Consumed {assignedItem.spCost} SP.");
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
        if (tooltipText != null)
        {
            tooltipText.text = $"You consumed {assignedItem.itemName}";
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

    // ==========================================
    // HOVER LOGIC CORRECTIONS
    // ==========================================
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

        // 1. If the file is completely blank
        if (assignedItem == null)
        {
            tooltipText.text = "Not yet discovered.";
            return;
        }

        // 2. Case A: Player possesses the item right now
        if (currentQuantity > 0)
        {
            bool isPepper = assignedItem.itemName.ToLower().Contains("pepper") ||
                            assignedItem.itemId.ToLower().Contains("pepper");

            tooltipText.text = isPepper ? "" : $"Use {assignedItem.itemName}";
        }
        // 3. Case B: Player ran out, but knows what it is (New!)
        else if (wasDiscovered)
        {
            tooltipText.text = $"You do not have any {assignedItem.itemName}";
        }
        // 4. Case C: Absolute secret silhouette 
        else
        {
            tooltipText.text = "Not yet discovered.";
        }
    }
}