using UnityEngine;
using UnityEngine.UIElements;
using System.Collections;
using UnityEngine.SceneManagement;

public class UI_VictoryScript : MonoBehaviour
{
    [SerializeField] private GameObject gameOverUi;
    [SerializeField] private UIDocument uiDocument;
    private VisualElement root;
    private VisualElement victoryScreen;
    private Button goToLobbyButton;
    [SerializeField] private float duration = 0.2f;


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

            if (goToLobbyButton != null)
            {
                goToLobbyButton.clicked += GoToLobbyV;
            }
        }
        if (victoryScreen != null)
        {
            victoryScreen.style.transformOrigin = new TransformOrigin(Length.Percent(50), Length.Percent(50));
            victoryScreen.style.translate = new Translate(Length.Percent(100), Length.Percent(0));
            StartVictoryAnimation();
        }
    }

    private void OnDisable()
    {
        if (goToLobbyButton != null)
        {
            goToLobbyButton.clicked -= GoToLobbyV;
        }
        if (victoryScreen != null)
        {
            victoryScreen.style.translate = new Translate(Length.Percent(100), Length.Percent(0));
        }
    }

    public void StartVictoryAnimation()
    {
        StartCoroutine(SlideFromRightCoroutine());
    }

    private IEnumerator SlideFromRightCoroutine()
    {
        float elapsedTime = 0f;
        float startPercent = 100f;
        float endPercent = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;

            float t = elapsedTime / duration;
            t = Mathf.SmoothStep(0f, 1f, t);

            float currentPercent = Mathf.Lerp(startPercent, endPercent, t);

            victoryScreen.style.translate = new Translate(Length.Percent(currentPercent), Length.Percent(0));

            yield return null;
        }

        victoryScreen.style.translate = new Translate(Length.Percent(endPercent), Length.Percent(0));
    }

    public void GoToLobbyV()
    {
        Debug.Log("Exit button pressed in Victory UI");
        AudioManager.Instance.PlaySFX("Button_Pressed");
        SceneManager.LoadScene(3);
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
