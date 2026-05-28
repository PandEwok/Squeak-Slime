using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using System.Collections; // Required for slide coroutine timers

public class EventCardUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [Header("UI References")]
    public TextMeshProUGUI nameText;
    public Image artworkImage;

    private EventCardData myCardData;
    private EventCardManager myManager;
    private RectTransform rectTransform;

    // Track state to prevent clicking cards before they finish sliding up
    private bool isAnimating = false;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
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

    /// <summary>
    /// Slides the card from a hidden low position up to its slot destination.
    /// </summary>
    public void TriggerDealAnimation(float duration, AnimationCurve easeCurve)
    {
        StartCoroutine(DealAnimationRoutine(duration, easeCurve));
    }

    private IEnumerator DealAnimationRoutine(float duration, AnimationCurve easeCurve)
    {
        isAnimating = true;

        // 1. Snapshot where the layout group WANTS this card to sit normally
        Vector2 targetPosition = Vector2.zero;

        // 2. Drop it way below the screen to start out
        Vector2 startingPosition = new Vector2(targetPosition.x, -1000f);
        rectTransform.anchoredPosition = startingPosition;

        float timer = 0f;
        while (timer < duration)
        {
            timer += Time.deltaTime;
            float progress = timer / duration;

            // Apply the physics ease curve to make it snap nicely
            float evaluatedProgress = easeCurve.Evaluate(progress);

            // Interpolate position smoothly
            rectTransform.anchoredPosition = Vector2.Lerp(startingPosition, targetPosition, evaluatedProgress);
            yield return null;
        }

        // Lock perfectly at final destination
        rectTransform.anchoredPosition = targetPosition;
        isAnimating = false;
    }

    // ==========================================
    // SAFETY BLOCKS DURING ANIMATION
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