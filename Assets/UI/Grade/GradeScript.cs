using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;
using static GradeScript;

public class GradeScript : MonoBehaviour
{
    [SerializeField] private UIDocument uiDocument;
    private VisualElement root;
    private Label excellent;
    private Label missed;
    private Label critical;
    private float displayDuration = 1f;
    public enum Grade
    {
        Excellent,
        Missed,
        Critical
    }
    void OnEnable()
    {
        uiDocument = GetComponent<UIDocument>();
        root = uiDocument.rootVisualElement;
        excellent = root.Q<Label>("Excellent");
        missed = root.Q<Label>("Missed");
        critical = root.Q<Label>("Crit");
    }


    public IEnumerator GradeDisplay(Grade grade, bool mustDisplay)
    {
        allGradeDisplay(false);
        if (mustDisplay)
        {
            switch (grade)
            {
                case Grade.Excellent:
                    excellent.style.display = DisplayStyle.Flex;
                    break;
                case Grade.Missed:
                    missed.style.display = DisplayStyle.Flex;
                    break;
                case Grade.Critical:
                    critical.style.display = DisplayStyle.Flex;
                    break;
                default:
                    break;
            }
            yield return new WaitForSeconds(displayDuration);
            allGradeDisplay(false);
        }
        else
        {
            switch (grade)
            {
                case Grade.Excellent:
                    excellent.style.display = DisplayStyle.None;
                    break;
                case Grade.Missed:
                    missed.style.display = DisplayStyle.None;
                    break;
                case Grade.Critical:
                    critical.style.display = DisplayStyle.None;
                    break;
                default:
                    break;
            }
        }
        gameObject.SetActive(false);
    }
    public void allGradeDisplay(bool mustDisplay)
    {
        if (mustDisplay)
        {
            excellent.style.display = DisplayStyle.Flex;
            missed.style.display = DisplayStyle.Flex;
            critical.style.display = DisplayStyle.Flex;
        }
        else
        {
            excellent.style.display = DisplayStyle.None;
            missed.style.display = DisplayStyle.None;
            critical.style.display = DisplayStyle.None;
        }

    }
    public void StartCoroutine(Grade grade, bool display)
    {
    StartCoroutine(GradeDisplay(grade, display));
    }

}



