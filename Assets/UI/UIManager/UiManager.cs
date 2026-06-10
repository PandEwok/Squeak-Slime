using UnityEngine;

public class UiManager : MonoBehaviour
{
    [HideInInspector] public GradeScript gradeScript;
    [Header("Prefabs")]
    [SerializeField] private GameObject actionMenu;
    [SerializeField] private GameObject qteWarning;
    [SerializeField] private GameObject gradeDisplay;

    private void Start()
    {
        gradeScript = gradeDisplay.GetComponent<GradeScript>();
    }

    public void ShowQTE(bool mustDisplay)
    {
        if (mustDisplay)
        {
            qteWarning.SetActive(true);
            AudioManager.Instance.PlaySFX("QTE");
        }
        else
        {
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
