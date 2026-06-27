using UnityEngine;
using TMPro;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using System.Collections; // Required for staggered coroutine

public class EventCardManager : MonoBehaviour
{
    [Header("Audio Settings")]
    [Tooltip("The exact name of your main menu track in the AudioManager database.")]
    public string musicTrackName = "EventScene";

    [Header("The Master Deck")]
    public List<EventCardData> allPossibleCards = new List<EventCardData>();

    [Header("UI Layout")]
    public List<EventCardUI> activeCardSlots;
    public TextMeshProUGUI globalTooltipText;

    [Header("Deal Animation Settings")]
    [Tooltip("How long it takes an individual card to finish its slide journey.")]
    public float cardSlideDuration = 0.4f;

    [Tooltip("The time delay wait window before dealing the next card.")]
    public float delayBetweenCards = 0.15f;

    [Tooltip("Smooth physics ease curve. Create a small hill at the end for a bouncy layout snap!")]
    public AnimationCurve dealEaseCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    private Vector3 playerDefPos = new Vector3(7777, 0, 0);
    public int nextSceneName = 9;
    private void Start()
    {
        if (Player.Instance != null)
        {
            Player.Instance.transform.position = playerDefPos;
            Player.Instance.StopAllCoroutines();
        }
        HideTooltip();
        if (AudioManager.Instance != null && !string.IsNullOrEmpty(musicTrackName))
        {
            AudioManager.Instance.StopLoopingSFX();
            AudioManager.Instance.StopMusic();
            AudioManager.Instance.PlayMusic(musicTrackName);
        }


            // Switch to calling our new animated sequence engine
            StartCoroutine(AnimateCardDealingSequence());
    }
    public IEnumerator AnimateCardDealingSequence()
    {
        if (allPossibleCards.Count < 3)
        {
            Debug.LogError("You need at least 3 cards in your Master Deck!");
            yield break;
        }

        // Shuffle deck logic
        List<EventCardData> deckToShuffle = new List<EventCardData>(allPossibleCards);
        for (int i = 0; i < deckToShuffle.Count; i++)
        {
            EventCardData temp = deckToShuffle[i];
            int randomIndex = Random.Range(i, deckToShuffle.Count);
            deckToShuffle[i] = deckToShuffle[randomIndex];
            deckToShuffle[randomIndex] = temp;
        }

        // Deal with staggered animation timings
        for (int i = 0; i < activeCardSlots.Count; i++)
        {
            if (activeCardSlots[i] == null) continue;

            // Setup data, then immediately fire its slide sequence
            activeCardSlots[i].SetupCard(deckToShuffle[i], this);
            activeCardSlots[i].TriggerDealAnimation(cardSlideDuration, dealEaseCurve);

            // Wait a tiny bit before dispensing the next physical card card slot
            yield return new WaitForSeconds(delayBetweenCards);
        }
    }

    public void ShowTooltip(string description) => globalTooltipText.text = description;
    public void HideTooltip() => globalTooltipText.text = "";

    public void SelectCard(EventCardData selectedCard)
    {
        Debug.Log($"Selected: {selectedCard.cardName}");

        // ==========================================
        // NEW: Tell the persistent Player exactly what card was clicked 
        // so the receiving scene knows what to draw.
        // ==========================================
        if (Player.Instance != null)
        {
            Player.Instance.pendingEventID = selectedCard.cardName;
        }

        if (selectedCard.cardType == EventCardData.EventType.Nothing)
        {
            if (Player.Instance != null)
            {
                // Advance progression floor
                Player.Instance.floor++;

                // Check against the player's own maxFloor setting
                if (Player.Instance.floor > Player.Instance.maxFloor)
                {
                    // Reset floor back to 1
                    Player.Instance.floor = 1;

                    // Advance biome enum mapping
                    int nextBiomeIndex = (int)Player.Instance.currentBiome + 1;

                    if (System.Enum.IsDefined(typeof(Player.BiomeType), nextBiomeIndex))
                    {
                        Player.Instance.currentBiome = (Player.BiomeType)nextBiomeIndex;
                        Debug.Log($"[Progression] Biome shifted successfully to: {Player.Instance.currentBiome}");
                    }
                    else
                    {
                        Debug.LogWarning("[Progression] Max Biome exceeded!");
                    }
                }
                Player.Instance.SwitchSceneInCaseOfVictory();
            }
        }
        else if (selectedCard.sceneToLoad != 0)
        {
            SceneManager.LoadScene(selectedCard.sceneToLoad);
        }
    }
}