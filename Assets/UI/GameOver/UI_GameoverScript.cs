using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class UI_GameoverScript : MonoBehaviour
{
    [SerializeField] private UIDocument uiDocument;
    [SerializeField] private GameObject victoryUI;
    private VisualElement root;
    private VisualElement deathScreen;

    private void OnEnable()
    {
        uiDocument = GetComponent<UIDocument>();
        root = uiDocument.rootVisualElement;
        deathScreen = root.Q<VisualElement>("DeathScreen");
    }


    private void Start()
    {

        var GoToLobbyButton = root.Q<Button>("ExitButtonGO");
        GoToLobbyButton?.RegisterCallback<ClickEvent>(ev => GoToLobbyG());
    }

    private void GoToLobbyG()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
    }

    public void ToggleGameOverUiVisibility(bool mustDisplay)
    {
        if (mustDisplay)
        {
            victoryUI.SetActive(false);
            deathScreen.style.display = DisplayStyle.Flex;
        }
        else
        {
            victoryUI.SetActive(true);
            deathScreen.style.display = DisplayStyle.None;
        }
    }
}
