using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class EventCard : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public enum EventType { Shop, Rest, Elite, Scavenge }
    public EventType type; // Set this in the Inspector for each prefab!

    [Header("Description Settings")]
    [TextArea(3, 10)]
    public string description; // What shows in the tooltip

    // We will find this automatically so you don't have to drag it in every time
    private TextMeshProUGUI tooltipText;

    void Start()
    {
        // This finds the text object named "MasterTooltipText" in your scene
        GameObject go = GameObject.Find("MasterTooltipText");
        if (go != null) tooltipText = go.GetComponent<TextMeshProUGUI>();

        // Setup the button click
        GetComponent<Button>().onClick.AddListener(OnCardClicked);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (tooltipText != null)
        {
            tooltipText.text = description;
            tooltipText.enabled = true;
        }
        transform.localScale = new Vector3(1.05f, 1.05f, 1.05f);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (tooltipText != null) tooltipText.enabled = false;
        transform.localScale = Vector3.one;
    }

    void OnCardClicked()
    {
        switch (type)
        {
            case EventType.Shop:
                Debug.Log("Opening Shop...");
                // SceneManager.LoadScene("ShopScene");
                break;
            case EventType.Rest:
                Debug.Log("Resting... HP Restored.");
                // PlayerStats.Heal(20);
                break;
            case EventType.Elite:
                Debug.Log("Starting Elite Battle!");
                break;
        }
    }
}