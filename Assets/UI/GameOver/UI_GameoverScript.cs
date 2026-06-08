using UnityEngine;
using UnityEngine.UIElements;

public class UI_GameoverScript : MonoBehaviour
{
    [SerializeField] private UIDocument uiDocument;
    [SerializeField] private GameObject victoryUI;

    private VisualElement root;
    private VisualElement deathScreen;
    private Button goToLobbyButton;

    private void Awake()
    {
        if (uiDocument == null)
        {
            uiDocument = GetComponent<UIDocument>();
        }
    }

    private void OnEnable()
    {
        if (uiDocument == null) return;

        root = uiDocument.rootVisualElement;
        if (root != null)
        {
            deathScreen = root.Q<VisualElement>("DeathScreen");
            goToLobbyButton = root.Q<Button>("ExitButtonGO");

            if (goToLobbyButton != null)
            {
                goToLobbyButton.clicked += GoToLobbyG;
            }
        }
    }

    private void OnDisable()
    {
        if (goToLobbyButton != null)
        {
            goToLobbyButton.clicked -= GoToLobbyG;
        }
    }

    public void GoToLobbyG()
    {
        Debug.Log("Bouton gameover pressé !");
        AudioManager.Instance.PlaySFX("Button_Pressed");
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    public void ToggleGameOverUiVisibility(bool mustDisplay)
    {
        if (deathScreen == null) return;

        if (mustDisplay)
        {
            if (victoryUI != null) victoryUI.SetActive(false);
            deathScreen.style.display = DisplayStyle.Flex;
        }
        else
        {
            if (victoryUI != null) victoryUI.SetActive(true);
            deathScreen.style.display = DisplayStyle.None;
        }
    }
}