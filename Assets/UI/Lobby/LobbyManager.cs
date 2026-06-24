using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class LobbyManager : MonoBehaviour
{
    [Header("Max Stats UI References")]
    public TextMeshProUGUI maxHpText;
    public TextMeshProUGUI maxSpText;
  
    [Header("Scene Transition Settings")]
    [Tooltip("Type the exact name of the combat/gameplay scene you want to load.")]
    public int nextSceneName = 9;

    private Vector3 playerDefPos = new Vector3(7777, 0 , 0);

    // Cache tracking variables to update UI live when skills are bought
    private float lastMaxHp;
    private float lastMaxSp;

    void Start()
    {
        Player.Instance.transform.position = playerDefPos;
        UpdateStatsUI();
        Player.Instance.floor = 1;
        Player.Instance.currentBiome = Player.BiomeType.FOREST;
    }

    void Update()
    {
        // Safety check to ensure the persistent player exists in the lobby
        if (Player.Instance == null || Player.Instance.stats == null) return;

        // Automatically detect if Max HP or Max SP increased via the skill trees!
        if (Player.Instance.stats.originalHealth != lastMaxHp || Player.Instance.stats.originalSP != lastMaxSp)
        {
            UpdateStatsUI();
        }
    }

    public void UpdateStatsUI()
    {
        if (Player.Instance == null || Player.Instance.stats == null) return;

        // Grab the updated values from your friend's stats script
        lastMaxHp = Player.Instance.stats.originalHealth;
        lastMaxSp = Player.Instance.stats.originalSP;

        // Update the UI text layouts
        if (maxHpText != null) maxHpText.text = lastMaxHp.ToString();
        if (maxSpText != null) maxSpText.text = lastMaxSp.ToString();
    }

    // ==========================================
    // BUTTON INTERACTION HOOK
    // ==========================================
    [ContextMenu("Trigger Scene Change")]
    public void LeaveLobbyAndStartGame()
    {
        SceneManager.LoadSceneAsync(nextSceneName);
    }
}