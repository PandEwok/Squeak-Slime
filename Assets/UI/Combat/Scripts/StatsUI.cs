using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class StatsUI : MonoBehaviour
{
    [Header("Text References")]
    public TextMeshProUGUI HP;
    public TextMeshProUGUI SP;
    public TextMeshProUGUI Floor;

    [Header("Fill Bar References")]
    public Image hpFill;
    public Image spFill;

    [Header("Floor Settings")]
    [Tooltip("The maximum floor count before a biome transition.")]
    public int maxFloorCount = 6; // NEW: Customizable max count in the inspector!

    // Cache tracking variables to prevent rewriting text components every frame
    private float lastHealth;
    private float lastMaxHealth;
    private float lastSP;
    private float lastMaxSP;
    private int lastFloor; // NEW: Cache to prevent string allocations every frame

    void Start()
    {
        // Force an initial setup when the scene starts
        UpdateHP();
        UpdateSP();
        UpdateFloor();
    }

    void Update()
    {
        // Safety check: Make sure the player actually exists in the scene
        if (Player.Instance == null || Player.Instance.stats == null) return;

        // Automatically detect if Health or Max Health changed this frame
        if (Player.Instance.stats.health != lastHealth || Player.Instance.stats.originalHealth != lastMaxHealth)
        {
            UpdateHP();
        }

        // Automatically detect if SP or Max SP changed this frame
        if (Player.Instance.stats.SP != lastSP || Player.Instance.stats.originalSP != lastMaxSP)
        {
            UpdateSP();
        }

        // NEW: Automatically detect if the current floor value has bumped up
        if (Player.Instance.floor != lastFloor)
        {
            UpdateFloor();
        }
    }

    public void UpdateHP()
    {
        // Store the values we are about to display
        lastHealth = Player.Instance.stats.health;
        lastMaxHealth = Player.Instance.stats.originalHealth;

        // Update Text
        if (HP != null) HP.text = lastHealth.ToString();

        // Update Fill Bar
        if (hpFill != null && lastMaxHealth > 0)
        {
            hpFill.fillAmount = lastHealth / lastMaxHealth;
        }
    }

    public void UpdateSP()
    {
        // Store the values we are about to display
        lastSP = Player.Instance.stats.SP;
        lastMaxSP = Player.Instance.stats.originalSP;

        // Update Text
        if (SP != null) SP.text = lastSP.ToString();

        // Update Fill Bar
        if (spFill != null && lastMaxSP > 0)
        {
            spFill.fillAmount = lastSP / lastMaxSP;
        }
    }

    // FIXED: Now completely bound to Player.Instance data!
    public void UpdateFloor()
    {
        if (Player.Instance == null) return;

        // Update the tracked cache
        lastFloor = Player.Instance.floor;

        // Display the text seamlessly as "Floor X / Y"
        if (Floor != null)
        {
            Floor.text = "Floor " + lastFloor + " / " + maxFloorCount;
        }
    }
}