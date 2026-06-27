using UnityEngine;
using UnityEngine.UIElements;

public class CreditsScript : MonoBehaviour
{
    [SerializeField] private UIDocument uiDocument;
    private VisualElement root;
    private Button closeButton;


    private void Awake()
    {
        if (Player.Instance != null)
        {
            Destroy(Player.Instance.gameObject);
        }
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
            closeButton = root.Q<Button>("Close");

            if (closeButton != null)
            {
                closeButton.clicked += QuitGame;
            }
        }
    }

    public void QuitGame()
    {
        Debug.Log("Closing game...");

        //Debug
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;

        //App
#else
            Application.Quit();
#endif
    }
}
