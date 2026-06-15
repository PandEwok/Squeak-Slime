using UnityEngine;
using UnityEngine.UIElements;
using System.Collections;
using UnityEngine.SceneManagement;
public class UI_GameoverScript : MonoBehaviour
{
    [SerializeField] private UIDocument uiDocument;
    [SerializeField] private GameObject victoryUI;

    private VisualElement root;
    private VisualElement deathScreen;
    private Button goToLobbyButton;
    [SerializeField] private float duration = 0.5f;

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
        deathScreen.style.top = new Length(-100, LengthUnit.Percent);
        StartGameOverAnimation();
    }

    private void OnDisable()
    {
        if (goToLobbyButton != null)
        {
            goToLobbyButton.clicked -= GoToLobbyG;
        }
        deathScreen.style.top = new Length(-100, LengthUnit.Percent);
    }

    public void StartGameOverAnimation()
    {
        StartCoroutine(SlideDownCoroutine());
    }

    private IEnumerator SlideDownCoroutine()
    {
        float elapsedTime = 0f;
        float startPercent = -100f;
        float endPercent = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;

            float t = elapsedTime / duration;
            t = Mathf.SmoothStep(0f, 1f, t);

            float currentPercent = Mathf.Lerp(startPercent, endPercent, t);

            deathScreen.style.top = new Length(currentPercent, LengthUnit.Percent);

            yield return null;
        }

        deathScreen.style.top = new Length(endPercent, LengthUnit.Percent);
    }
    public void GoToLobbyG()
    {
        Debug.Log("Bouton gameover pressé !");
        AudioManager.Instance.PlaySFX("Button_Pressed");
        SceneManager.LoadScene(8);
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