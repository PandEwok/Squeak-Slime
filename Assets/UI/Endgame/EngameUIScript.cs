using UnityEngine;

public class EngameUIScript : MonoBehaviour
{
    public static EngameUIScript Instance { get; private set; }
    [Header("UI Elements")]
    public GameObject gameOverUI;
    public GameObject victoryUI;
    public GameObject actionMenu;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

    }
    void Start()
    {
        gameOverUI.SetActive(false);
        victoryUI.SetActive(false);
    }
    public void GameOver()
    {
        Player.Instance.IsInBattle = false;
        Player.Instance.IsDead = true;
        gameOverUI.SetActive(true);
        if (actionMenu == null)
        {
            actionMenu = Player.Instance.uiManager.actionMenu;
        }
        actionMenu.SetActive(false);
        gameOverUI.GetComponent<UI_GameoverScript>().ToggleGameOverUiVisibility(true);
    }

    public void Victory()
    {
        Player.Instance.IsInBattle = false;
        if (actionMenu == null)
        {
            actionMenu = Player.Instance.uiManager.actionMenu;
        }
        victoryUI.SetActive(true);
        actionMenu.SetActive(false);
        victoryUI.GetComponent<UI_VictoryScript>().ToggleVictoryUiVisibility(true);
    }
}
