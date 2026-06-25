using UnityEngine;

public class UiManager : MonoBehaviour
{
    [HideInInspector] public GradeScript gradeScript;
    [Header("Prefabs")]
    [SerializeField] public GameObject actionMenu;
    [SerializeField] private GameObject qteWarning;
    [SerializeField] private GameObject gradeDisplay;
    public GameObject spamIndicator;
    [SerializeField] private GameObject leftClickIndicator;
    [SerializeField] private Vector3 leftClickIndicatorPosition = new Vector3(8, 4, 1);
    private GameObject leftClickIndicatorInstance;
    [SerializeField] private Color waitColor = new Color(1f, 0, 0, 1f);
    [SerializeField] private Color readyColor = new Color(0, 1f, 0, 1f);
    private SpriteRenderer qteSprite;
    public StatsUI statsUi;

    private void Start()
    {
        gradeScript = gradeDisplay.GetComponent<GradeScript>();
        qteSprite = qteWarning.GetComponent<SpriteRenderer>();
    }
    private void OnEnable()
    {
        if (spamIndicator != null)
        {
            spamIndicator.SetActive(false);
        }
    }
    public void ShowQTE(bool mustDisplay, bool isReady = false, bool isFireBall = false)
    {
        if (mustDisplay)
        {
            qteWarning.SetActive(true);
            if (isReady)
            {
                qteSprite.color = readyColor;
                AudioManager.Instance.PlaySFX("QTE");

            }
            else
            {
                qteSprite.color = waitColor;
            }
            if (!isFireBall)
            {
                DoShowLeftClickIndicator(true);
            }
        }

        else
        {
            qteSprite.color = waitColor;
            qteWarning.SetActive(false);
            DoShowLeftClickIndicator(false);
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
    public void DoShowLeftClickIndicator(bool show)
    {
        if(leftClickIndicatorInstance == null)
        {
            leftClickIndicatorInstance = Instantiate(leftClickIndicator, leftClickIndicatorPosition, Quaternion.identity);
        }
        if (show)
        {
            if(leftClickIndicatorInstance != null)
            {
                leftClickIndicatorInstance.SetActive(true);
            }
        }
        else
        {
            if(leftClickIndicatorInstance != null)
            {
                leftClickIndicatorInstance.SetActive(false);
            }
        }
    }
}
