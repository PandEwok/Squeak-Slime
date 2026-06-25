using UnityEngine;

public class EndgameUIScript : MonoBehaviour
{
    public static EndgameUIScript Instance { get; private set; }
    [Header("UI Elements")]
    public GameObject gameOverUI;
    public GameObject victoryUI;
    public GameObject actionMenu;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Debug.Log($"[Singleton] Doublon de {gameObject.name} détecté et détruit.");
            Destroy(gameObject);
            return;
        }

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
