using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopCurrencyDisplay : MonoBehaviour
{
    [System.Serializable]
    public struct ToothConfig
    {
        public Player.BiomeType biome;
        public Tooth toothData;
    }

    [Header("Prefab & Layout Setup")]
    public GameObject slotPrefab;
    public Transform contentParent;

    [Header("Tooth Visual Configurations")]
    public ToothConfig[] toothConfigs;

    private List<GameObject> spawnedSlots = new List<GameObject>();

    private void OnEnable()
    {
        Debug.Log("⚙️ [Shop UI] OnEnable triggered. Waiting for Player to load...");
        StartCoroutine(WaitAndGenerateDisplay());
    }

    private IEnumerator WaitAndGenerateDisplay()
    {
        // This loops safely in the background, waiting exactly one frame at a time
        // until the Player and Inventory are fully loaded into the game.
        while (Player.Instance == null || Player.Instance.inventory == null)
        {
            yield return null;
        }

        // The moment the Player is ready, we draw the shop UI!
        GenerateDynamicDisplay();
    }

    public void GenerateDynamicDisplay()
    {
        if (Player.Instance == null || Player.Instance.inventory == null) return;

        // Clean up ALL leftover slots from the previous shop visit
        foreach (var slot in spawnedSlots)
        {
            if (slot != null) Destroy(slot);
        }
        spawnedSlots.Clear();

        Player.BiomeType activeBiome = Player.Instance.currentBiome;

        // Loop through EVERY configuration and spawn a prefab for EVERY match
        foreach (var config in toothConfigs)
        {
            if (config.biome == activeBiome && config.toothData != null)
            {
                GameObject newSlot = Instantiate(slotPrefab, contentParent);
                spawnedSlots.Add(newSlot);

                Image uiIcon = newSlot.GetComponentInChildren<Image>();
                TextMeshProUGUI uiText = newSlot.GetComponentInChildren<TextMeshProUGUI>();

                if (uiIcon != null)
                {
                    uiIcon.sprite = config.toothData.itemIcon;

                    // ---> NEW CODE: Apply the ScriptableObject's default color tint! <---
                    uiIcon.color = config.toothData.defaultColor;
                }

                if (uiText != null)
                {
                    uiText.text = GetToothCountFromInventory(config.toothData).ToString();
                }
            }
        }
    }

    private int GetToothCountFromInventory(Tooth toothAsset)
    {
        if (toothAsset == null) return 0;
        if (Player.Instance.inventory.teethPossessed.TryGetValue(toothAsset, out int quantity))
        {
            return quantity;
        }
        return 0;
    }
}