using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class UI_VictoryScript : MonoBehaviour
{
    [SerializeField] private GameObject gameOverUi;
    [SerializeField] private UIDocument uiDocument;
    private VisualElement root;
    private VisualElement victoryScreen;

    private void OnEnable()
    {
        
        uiDocument = GetComponent<UIDocument>();
        root = uiDocument.rootVisualElement;
        victoryScreen = root.Q<VisualElement>("VictoryScreen");
        
    }

    private void Start()
    {
        var goToLobbyButton = root.Q<Button>("ExitButtonV");
        goToLobbyButton?.RegisterCallback<ClickEvent>(ev => GoToLobbyV());
    }


    private void GoToLobbyV()
    {
        Debug.Log("Exit button pressed in Victory UI");
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
    }

    public void ToggleVictoryUiVisibility(bool mustDisplay)
    {
        if (mustDisplay)
        {
            gameOverUi.SetActive(false);
            victoryScreen.style.display = DisplayStyle.Flex;

        }
        else
        {
            gameOverUi.SetActive(true);
            victoryScreen.style.display = DisplayStyle.None;
        }
    }
}
