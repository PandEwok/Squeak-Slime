using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using System.Collections;

public class EventCardUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [Header("UI References")]
    public TextMeshProUGUI nameText;
    public Image artworkImage;

    private EventCardData myCardData;
    private EventCardManager myManager;
    private RectTransform rectTransform;

    // FIX 1: Make this a class variable so it's remembered globally
    private Vector2 targetPosition;
    private bool isAnimating = false;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();

        // FIX 2: Cache the exact (0,0) slot position you set in the editor right now!
        targetPosition = rectTransform.anchoredPosition;

        // FIX 3: Instantly banish the card way below the screen before the game even draws frame 1
        rectTransform.anchoredPosition = new Vector2(targetPosition.x, -1500f);
    }

    public void SetupCard(EventCardData data, EventCardManager manager)
    {
        myCardData = data;
        myManager = manager;

        if (nameText != null) nameText.text = myCardData.cardName;
        if (artworkImage != null && myCardData.cardArtwork != null)
        {
            artworkImage.sprite = myCardData.cardArtwork;
        }
    }

    public void TriggerDealAnimation(float duration, AnimationCurve easeCurve)
    {
        StartCoroutine(DealAnimationRoutine(duration, easeCurve));
    }

    private IEnumerator DealAnimationRoutine(float duration, AnimationCurve easeCurve)
    {
        isAnimating = true;

        // FIX 4: We don't read anchoredPosition here anymore because it's already hidden!
        // We use the clean 'targetPosition' we cached back in Awake.
        Vector2 startingPosition = new Vector2(targetPosition.x, -1500f);
        rectTransform.anchoredPosition = startingPosition;

        float timer = 0f;
        while (timer < duration)
        {
            timer += Time.deltaTime;
            float progress = timer / duration;
            float evaluatedProgress = easeCurve.Evaluate(progress);

            // Smoothly slide from the basement up to its authentic slot home
            rectTransform.anchoredPosition = Vector2.Lerp(startingPosition, targetPosition, evaluatedProgress);
            yield return null;
        }

        rectTransform.anchoredPosition = targetPosition;
        isAnimating = false;
    }

    // ==========================================
    // SAFETY BLOCKS (Keep these the same)
    // ==========================================
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (isAnimating || myManager == null || myCardData == null) return;
        myManager.ShowTooltip(myCardData.tooltipDescription);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (myManager != null) myManager.HideTooltip();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (isAnimating || myManager == null || myCardData == null) return;
        myManager.SelectCard(myCardData);
    }
}