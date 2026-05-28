using UnityEngine;
using TMPro; // Needed for text
using UnityEngine.EventSystems; // Needed for hovering!

public class SkillButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public GameObject descriptionBox; // The box to show/hide
    public TextMeshProUGUI descriptionText; // The text to change

    [TextArea(3, 10)] // Makes the box bigger in the Inspector
    public string skillDescription;

    // This runs when the mouse enters the button area
    public void OnPointerEnter(PointerEventData eventData)
    {
        descriptionText.text = skillDescription;
        descriptionBox.SetActive(true);
    }

    // This runs when the mouse leaves the button area
    public void OnPointerExit(PointerEventData eventData)
    {
        descriptionBox.SetActive(false);
    }
}