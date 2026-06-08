using UnityEngine;
using UnityEngine.UIElements;

public class UI_VictoryScript : MonoBehaviour
{
    [SerializeField] private GameObject gameOverUi;
    [SerializeField] private UIDocument uiDocument;
    private VisualElement root;
    private VisualElement victoryScreen;
    private Button goToLobbyButton;


    private void Awake()
    {
        if(uiDocument == null)
        {
            uiDocument = GetComponent<UIDocument>();
        }
    }
    private void OnEnable()
    {
        if (uiDocument == null) return;

        root = uiDocument.rootVisualElement;
        if(root != null)
        {
            victoryScreen = root.Q<VisualElement>("VictoryScreen");
            goToLobbyButton = root.Q<Button>("ExitButtonV");

            if (goToLobbyButton != null) goToLobbyButton.clicked += GoToLobbyV;
        }
    }


    public void GoToLobbyV()
    {
        Debug.Log("Exit button pressed in Victory UI");
        AudioManager.Instance.PlaySFX("Button_Pressed");
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
    }

    public void ToggleVictoryUiVisibility(bool mustDisplay)
    {
        if(victoryScreen == null) {return; }
        if (mustDisplay)
        {
            if(gameOverUi != null) gameOverUi.SetActive(false);
            victoryScreen.style.display = DisplayStyle.Flex;

        }
        else
        {
            if(gameOverUi != null) gameOverUi.SetActive(true);
            victoryScreen.style.display = DisplayStyle.None;
        }
    }
}
