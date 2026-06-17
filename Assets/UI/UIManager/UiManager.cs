using UnityEngine;

public class UiManager : MonoBehaviour
{
    [HideInInspector] public GradeScript gradeScript;
    [Header("Prefabs")]
    [SerializeField] public GameObject actionMenu;
    [SerializeField] private GameObject qteWarning;
    [SerializeField] private GameObject gradeDisplay;
    [SerializeField] private Color waitColor = new Color(1f, 0, 0, 1f);
    [SerializeField] private Color readyColor = new Color(0, 1f, 0, 1f);
    private SpriteRenderer qteSprite;
    public StatsUI statsUi;

    private void Start()
    {
        gradeScript = gradeDisplay.GetComponent<GradeScript>();
        qteSprite = qteWarning.GetComponent<SpriteRenderer>();
    }

    public void ShowQTE(bool mustDisplay, bool isReady = false)
    {
        if (mustDisplay)
        {
            qteWarning.SetActive(true);
            if(isReady)
            {
                qteSprite.color = readyColor;
                AudioManager.Instance.PlaySFX("QTE");
            }
            else
            {
                qteSprite.color = waitColor;
            }
        }
        else
        {
            qteSprite.color = waitColor;
            qteWarning.SetActive(false);
        }
    }
    public void DisplayGrade(GradeScript.Grade grade, bool display)
    {
        if (gradeScript != null)
        {
            gradeScript.StopAllCoroutines();
            gradeScript.gameObject.SetActive(true);
            StartCoroutine(gradeScript.GradeDisplay(grade, display));
        }
    }
}
