using UnityEngine;
using UnityEngine.UIElements;
using System.Collections;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
public class UI_GameoverScript : MonoBehaviour
{
    [SerializeField] private UIDocument uiDocument;
    [SerializeField] private VisualTreeAsset toothRowTemplate;
    [SerializeField] private GameObject victoryUI;

    private VisualElement root;
    private VisualElement deathScreen;
    private Button goToLobbyButton;
    private VisualElement teethRoot;
    private Label floorLabel;
    private Label biomeLabel;
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
            teethRoot = root.Q<VisualElement>("TeethRoot");
            goToLobbyButton = root.Q<Button>("ExitButtonGO");
            floorLabel = root.Q<Label>("Floor");
            biomeLabel = root.Q<Label>("Biome");

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

    private void PopulateTeethSummary()
    {
        if (teethRoot == null || toothRowTemplate == null || Player.Instance.inventory == null) return;

        teethRoot.Clear();

        foreach (KeyValuePair<Tooth, int> entry in Player.Instance.inventory.teethOfCurrentRun)
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

    private void SetLevelInformations()
    {
        if (floorLabel != null)
        {
            floorLabel.text = $"Floor : {Player.Instance.floor}";
        }
        if (biomeLabel != null)
        {
            biomeLabel.text = $"Biome : {Player.Instance.GetBiomeToString()}";
        }
    }
    public void StartGameOverAnimation()
    {
        StopAllCoroutines();
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
        Player.Instance.inventory.ClearCurrentRunTeeth();
        SceneManager.LoadScene(8);
        
    }

    public void ToggleGameOverUiVisibility(bool mustDisplay)
    {
        Debug.Log($"ToggleGameOverUiVisibility appelé avec mustDisplay={mustDisplay}", this);
        if (uiDocument == null) return;

        if (mustDisplay)
        {
            if (victoryUI != null) victoryUI.SetActive(false);

            PopulateTeethSummary();
            SetLevelInformations();

            if (deathScreen != null)
            {
                deathScreen.style.display = DisplayStyle.Flex;
                deathScreen.style.top = new Length(-100, LengthUnit.Percent);
                StartGameOverAnimation();
            }
        }
        else
        {
            if (victoryUI != null) victoryUI.SetActive(true);
            if (deathScreen != null) deathScreen.style.display = DisplayStyle.None;
            deathScreen.style.top = new Length(-100, LengthUnit.Percent);
        }
    }
}