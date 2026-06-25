using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class EventInventoryUIController : MonoBehaviour
{
    public enum PageType
    {
        Items,
        Teeth
    }

    [System.Serializable]
    public struct InventoryPage
    {
        public string pageName;
        public PageType pageType;
        public GameObject pagePanel;

        [Header("UI Slots (Item_0 to Item_3)")]
        public List<InventorySlotUI> uiSlots;

        [Header("Real Asset Mapping")]
        public List<ItemData> pageItems;
        public List<Tooth> pageTeeth;
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

    public void UpdatePageDisplay()
    {
        if (inventoryPages.Count == 0) return;

        for (int i = 0; i < inventoryPages.Count; i++)
        {
            bool isCurrentPage = (i == currentPageIndex);
            inventoryPages[i].pagePanel.SetActive(isCurrentPage);

            if (isCurrentPage)
            {
                if (pageTitleText != null) pageTitleText.text = inventoryPages[i].pageName;
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

            int quantity = 0;
            string aName = "???";
            string aDesc = "???";
            string aId = "";
            Sprite aIcon = null;
            Color aColor = Color.white;
            bool hasAsset = false;
            bool isToothType = false;

            bool playerExists = (Player.Instance != null && Player.Instance.inventory != null);

            if (page.pageType == PageType.Items)
            {
                isToothType = false;
                if (i < page.pageItems.Count && page.pageItems[i] != null)
                {
                    ItemData targetItem = page.pageItems[i];
                    hasAsset = true;
                    aName = targetItem.itemName;
                    aDesc = targetItem.itemDescription;
                    aId = targetItem.itemId;
                    aIcon = targetItem.itemIcon;
                    aColor = targetItem.defaultColor;

                    if (playerExists && Player.Instance.inventory.itemsPossessed.ContainsKey(targetItem))
                    {
                        quantity = Player.Instance.inventory.itemsPossessed[targetItem];
                    }
                }
            }
            else if (page.pageType == PageType.Teeth)
            {
                isToothType = true;
                if (i < page.pageTeeth.Count && page.pageTeeth[i] != null)
                {
                    Tooth targetTooth = page.pageTeeth[i];
                    hasAsset = true;
                    aName = targetTooth.itemName;
                    aDesc = targetTooth.itemDescription;
                    aId = targetTooth.itemId;
                    aIcon = targetTooth.itemIcon;
                    aColor = targetTooth.defaultColor;

                    if (playerExists && Player.Instance.inventory.teethPossessed.ContainsKey(targetTooth))
                    {
                        quantity = Player.Instance.inventory.teethPossessed[targetTooth];
                    }
                }
            }

            slot.UpdateSlotDisplay(quantity, i, aName, aDesc, aId, aIcon, aColor, hasAsset, isToothType, this);
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

    public void ConsumeItemFromSlot(int slotIndex)
    {
        if (Player.Instance == null || Player.Instance.inventory == null) return;
        if (currentPageIndex >= inventoryPages.Count) return;

        InventoryPage currentPage = inventoryPages[currentPageIndex];

        if (currentPage.pageType == PageType.Items)
        {
            if (slotIndex < currentPage.pageItems.Count && currentPage.pageItems[slotIndex] != null)
            {
                ItemData targetItem = currentPage.pageItems[slotIndex];
                if (Player.Instance.inventory.itemsPossessed.ContainsKey(targetItem) && Player.Instance.inventory.itemsPossessed[targetItem] > 0)
                {
                    // 1. Consume the item backend data
                    targetItem.UseItem(Player.Instance.gameObject);

                    // 2. Clear any canvas race conditions by forcing Unity 
                    // to wait until the end of the frame before drawing the numbers
                    StartCoroutine(RefreshUIDelayed());
                }
            }
        }
    }

    // Add this small helper coroutine at the bottom of the script
    private System.Collections.IEnumerator RefreshUIDelayed()
    {
        yield return new WaitForEndOfFrame();
        UpdatePageDisplay();
    }
}