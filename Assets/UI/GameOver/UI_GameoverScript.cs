using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class UI_GameoverScript : MonoBehaviour
{
    [SerializeField] private UIDocument uiDocument;
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

        var GoToLobbyButton = root.Q<Button>("ExitButton");
        GoToLobbyButton?.RegisterCallback<ClickEvent>(ev => GoToLobby());
    }

    private void GoToLobby()
    {
        UnityEditor.EditorApplication.isPlaying = false;
    }

    public void ToggleGameOverUiVisibility(bool mustDisplay)
    {
        if (mustDisplay)
        {
            deathScreen.style.display = DisplayStyle.Flex;
        }
        else
        {
            deathScreen.style.display = DisplayStyle.None;
        }
    }
}
