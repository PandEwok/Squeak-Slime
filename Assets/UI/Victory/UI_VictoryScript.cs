using UnityEngine;
using UnityEngine.UIElements;
using System.Collections;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class UI_VictoryScript : MonoBehaviour
{
    [SerializeField] private GameObject gameOverUi;
    [SerializeField] private VisualTreeAsset toothRowTemplate;
    [SerializeField] private UIDocument uiDocument;
    private VisualElement root;
    private VisualElement teethRoot;
    private VisualElement victoryScreen;
    private Button goToLobbyButton;
    [SerializeField] private float duration = 0.2f;
    private Vector3 playerDefPos = new Vector3(7777, 0, 0);


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
            teethRoot = root.Q<VisualElement>("TeethRoot");

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

    private void PopulateTeethSummary()
    {
        if (teethRoot == null || toothRowTemplate == null || Player.Instance.inventory == null) return;

        teethRoot.Clear();

        foreach (KeyValuePair<Tooth, int> entry in Player.Instance.inventory.TeethOfCurrentBattle)
        {
            Tooth toothData = entry.Key;
            int amount = entry.Value;

            if (amount <= 0) continue;


            TemplateContainer rowInstance = toothRowTemplate.Instantiate();

            VisualElement iconElement = rowInstance.Q<VisualElement>("ToothIcon");
            Label textLabel = rowInstance.Q<Label>("ToothText");

            if (textLabel != null)
            {
                textLabel.text = $"{toothData.itemName} : X {amount}";
            }

            if (iconElement != null && toothData.itemIcon != null)
            {
                iconElement.style.backgroundImage = new StyleBackground(toothData.itemIcon);
                iconElement.style.unityBackgroundImageTintColor = toothData.defaultColor;
            }

            teethRoot.Add(rowInstance);
            Debug.Log($"[UI] Ligne ajoutée pour {toothData.itemName}. Nombre total d'enfants dans TeethRoot : {teethRoot.childCount}");
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
        if (Combat_Logic.Instance != null)
        {
            Combat_Logic.Instance.StopAllCoroutines();
        }
        
        Debug.Log("Exit button pressed in Victory UI");
        AudioManager.Instance.PlaySFX("Button_Pressed");
        if (Player.Instance != null)
        {
            Player.Instance.inventory.ClearCurrentBattleTeeth();
        }
        SceneManager.LoadSceneAsync(3);
    }

    public void ToggleVictoryUiVisibility(bool mustDisplay)
    {
        if(victoryScreen == null) {return; }
        if (mustDisplay)
        {
            if(gameOverUi != null) gameOverUi.SetActive(false);
            PopulateTeethSummary();
            victoryScreen.style.display = DisplayStyle.Flex;

        }
        else
        {
            if(gameOverUi != null) gameOverUi.SetActive(true);
            victoryScreen.style.display = DisplayStyle.None;
        }
    }
}
