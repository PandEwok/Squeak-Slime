using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class GradeScript : MonoBehaviour
{
    [SerializeField] private UIDocument uiDocument;
    private VisualElement root;
    private Label excellent;
    private Label missed;
    private Label critical;
    [HideInInspector] public float displayDuration = 3f;
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
    }

    private void Start()
    {
        excellent = root.Q<Label>("Excellent");
        missed = root.Q<Label>("Missed");
        critical = root.Q<Label>("Crit");
    }
    
    public IEnumerator gradeDisplay(Grade grade, bool mustDisplay)
    {
        if (mustDisplay)
        {
            switch (grade)
            {
                case Grade.Excellent:
                    Debug.Log("Displaying Excellent grade");
                    excellent.style.visibility = Visibility.Visible;
                    yield return new WaitForSeconds(displayDuration);
                    Debug.Log("Hiding Excellent grade");
                    excellent.style.visibility = Visibility.Hidden;
                    break;
                case Grade.Missed:
                    missed.style.visibility = Visibility.Visible;
                    yield return new WaitForSeconds(displayDuration);
                    missed.style.visibility = Visibility.Hidden;
                    break;
                case Grade.Critical:
                    critical.style.visibility = Visibility.Visible;
                    yield return new WaitForSeconds(displayDuration);
                    critical.style.visibility = Visibility.Hidden;
                    break;
                default:
                    break;
            }
        }
        else
        {
            switch (grade)
            {
                case Grade.Excellent:
                    excellent.style.visibility = Visibility.Hidden;
                    break;
                case Grade.Missed:
                    missed.style.visibility = Visibility.Hidden;
                    break;
                case Grade.Critical:
                    critical.style.visibility = Visibility.Hidden;
                    break;
                default:
                    break;
            }
        }
    }
    public void allGradeDisplay(bool mustDisplay)
    {
        if (mustDisplay)
        {
            excellent.style.visibility = Visibility.Visible;
            missed.style.visibility = Visibility.Visible;
            critical.style.visibility = Visibility.Visible;
        }
        else
        {
            excellent.style.visibility = Visibility.Hidden;
            missed.style.visibility = Visibility.Hidden;
            critical.style.visibility = Visibility.Hidden;
        }

    }
}



