using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class EventInventoryUIController : MonoBehaviour
{
    [System.Serializable]
    public struct InventoryPage
    {
        public string pageName;
        public GameObject pagePanel;

        [Header("UI Slots (Item_0 to Item_3)")]
        public List<InventorySlotUI> uiSlots;

        [Header("Debug Mock Inventory Data")]
        [Tooltip("Match the sizes of your UI slots above. Set quantities for debug testing.")]
        public List<int> mockQuantities;
    }

    [Header("Inventory Pages Setup")]
    public List<InventoryPage> inventoryPages = new List<InventoryPage>();

    [Header("Global UI References")]
    public TextMeshProUGUI pageTitleText;

    [Header("Animation Settings")]
    public Animator inventoryAnimator;
    public string openTriggerName = "OpenMenu";
    public string closeTriggerName = "CloseMenu";

    private int currentPageIndex = 0;
    private bool isInventoryOpen = false;

    private void Start()
    {
        UpdatePageDisplay();
    }

    public void CycleRight()
    {
        if (inventoryPages.Count == 0) return;
        currentPageIndex = (currentPageIndex + 1) % inventoryPages.Count;
        UpdatePageDisplay();
    }

    public void CycleLeft()
    {
        if (inventoryPages.Count == 0) return;
        currentPageIndex = (currentPageIndex - 1 + inventoryPages.Count) % inventoryPages.Count;
        UpdatePageDisplay();
    }

    private void UpdatePageDisplay()
    {
        if (inventoryPages.Count == 0) return;

        for (int i = 0; i < inventoryPages.Count; i++)
        {
            bool isCurrentPage = (i == currentPageIndex);
            inventoryPages[i].pagePanel.SetActive(isCurrentPage);

            if (isCurrentPage)
            {
                pageTitleText.text = inventoryPages[i].pageName;
                RenderPageSlots(inventoryPages[i]);
            }
        }
    }

    private void RenderPageSlots(InventoryPage page)
    {
        int slotCount = Mathf.Min(page.uiSlots.Count, 4);

        for (int i = 0; i < slotCount; i++)
        {
            InventorySlotUI slot = page.uiSlots[i];
            if (slot == null) continue;

            // Grab the quantity from our debug list safely
            int quantity = 0;
            if (i < page.mockQuantities.Count)
            {
                quantity = page.mockQuantities[i];
            }

            // Tell the slot to directly render itself using its own ItemData!
            slot.UpdateSlotDisplay(quantity, i, this);
        }
    }

    public void ToggleInventory()
    {
        if (inventoryAnimator == null) return;

        if (isInventoryOpen)
        {
            inventoryAnimator.SetTrigger(closeTriggerName);
            isInventoryOpen = false;
        }
        else
        {
            inventoryAnimator.SetTrigger(openTriggerName);
            isInventoryOpen = true;
            UpdatePageDisplay();
        }
    }
    /// <summary>
    /// Called by individual slots to permanently reduce quantities upon clicking
    /// </summary>
    public void ConsumeItemFromSlot(int slotIndex)
    {
        if (currentPageIndex >= inventoryPages.Count) return;
        InventoryPage currentPage = inventoryPages[currentPageIndex];

        if (slotIndex < currentPage.mockQuantities.Count && currentPage.mockQuantities[slotIndex] > 0)
        {
            // Deduct item count
            currentPage.mockQuantities[slotIndex]--;

            // Redraw current page graphics immediately to represent correct numbers (or trigger silhouette if 0)
            UpdatePageDisplay();
        }
    }
}