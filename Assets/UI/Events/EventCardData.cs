using UnityEngine;

[CreateAssetMenu(fileName = "New Event Card", menuName = "Events/Event Card")]
public class EventCardData : ScriptableObject
{
    public enum EventType { Shop, Rest, Search, Treasure, Hunt, Nothing }

    [Header("Card Identity")]
    public EventType cardType;
    public string cardName;
    [TextArea] public string tooltipDescription;
    public Sprite cardArtwork;

    [Header("Scene Routing")]
    [Tooltip("The exact name of the Unity Scene to load. Leave blank if Type is 'Nothing'.")]
    public string sceneToLoad;
}