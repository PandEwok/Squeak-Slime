using UnityEngine;
using System.Collections;

public class MainMenuIntro : MonoBehaviour
{
    [Header("UI Elements")]
    public RectTransform playButton;
    public RectTransform settingsButton;
    public RectTransform quitButton;
    public RectTransform leftGraphic;
    public RectTransform titleText;
    public CanvasGroup flashCanvasGroup;

    [Header("Timing Settings")]
    public float slideDuration = 0.5f;
    public float delayBetweenButtons = 0.15f;
    public float delayBeforeGraphic = 0.2f;
    public float delayBeforeTitle = 0.3f;
    public float flashDuration = 0.4f;

    [Header("Animation Curves")]
    public AnimationCurve slideCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    private void Start()
    {
        // Start the grand cinematic sequence!
        StartCoroutine(IntroSequenceRoutine());
    }

    private IEnumerator IntroSequenceRoutine()
    {
        // 1. INITIAL SETUP: Cache target positions & hide everything off-screen
        Vector2 playTarget = playButton.anchoredPosition;
        Vector2 settingsTarget = settingsButton.anchoredPosition;
        Vector2 quitTarget = quitButton.anchoredPosition;
        Vector2 graphicTarget = leftGraphic.anchoredPosition;
        Vector2 titleTarget = titleText.anchoredPosition;

        playButton.anchoredPosition = new Vector2(1000f, playTarget.y);
        settingsButton.anchoredPosition = new Vector2(1000f, settingsTarget.y);
        quitButton.anchoredPosition = new Vector2(1000f, quitTarget.y);
        leftGraphic.anchoredPosition = new Vector2(-1500f, graphicTarget.y);
        titleText.anchoredPosition = new Vector2(titleTarget.x, 500f);

        // FIX 1: Turn off blocking on startup so the panel is completely ignored
        flashCanvasGroup.alpha = 0f;
        flashCanvasGroup.blocksRaycasts = false;
        flashCanvasGroup.interactable = false;

        yield return new WaitForSeconds(0.2f); // Quick breath before starting

        // 2. SLIDE BUTTONS (Play -> Settings -> Quit)
        StartCoroutine(SlideElement(playButton, new Vector2(1000f, playTarget.y), playTarget));
        yield return new WaitForSeconds(delayBetweenButtons);

        StartCoroutine(SlideElement(settingsButton, new Vector2(1000f, settingsTarget.y), settingsTarget));
        yield return new WaitForSeconds(delayBetweenButtons);

        StartCoroutine(SlideElement(quitButton, new Vector2(1000f, quitTarget.y), quitTarget));
        yield return new WaitForSeconds(slideDuration + delayBeforeGraphic);

        // 3. SLIDE GRAPHIC
        StartCoroutine(SlideElement(leftGraphic, new Vector2(-1500f, graphicTarget.y), graphicTarget));
        yield return new WaitForSeconds(slideDuration + delayBeforeTitle);

        // 4. DROP TITLE DOWN
        StartCoroutine(SlideElement(titleText, new Vector2(titleTarget.x, 500f), titleTarget));
        yield return new WaitForSeconds(slideDuration * 0.5f); // Flash right as title hits home

        // 5. THE IMPACT FLASH EFFECT
        yield return StartCoroutine(FlashScreenRoutine());
    }

    /// <summary>
    /// Helper coroutine to smoothly move a UI element from A to B
    /// </summary>
    private IEnumerator SlideElement(RectTransform element, Vector2 start, Vector2 target)
    {
        float timer = 0f;
        while (timer < slideDuration)
        {
            timer += Time.deltaTime;
            float progress = slideCurve.Evaluate(timer / slideDuration);
            element.anchoredPosition = Vector2.Lerp(start, target, progress);
            yield return null;
        }
        element.anchoredPosition = target;
    }

    /// <summary>
    /// Instantly spikes the screen to white and fades out cleanly
    /// </summary>
    private IEnumerator FlashScreenRoutine()
    {
        // FIX 2: Turn on blocking DURING the white flash (prevents accidental double clicks)
        flashCanvasGroup.alpha = 1f;
        flashCanvasGroup.blocksRaycasts = true;
        flashCanvasGroup.interactable = true;

        float timer = 0f;
        while (timer < flashDuration)
        {
            timer += Time.deltaTime;
            flashCanvasGroup.alpha = Mathf.Lerp(1f, 0f, timer / flashDuration);
            yield return null;
        }

        // FIX 3: Turn blocking back OFF so players can click buttons again!
        flashCanvasGroup.alpha = 0f;
        flashCanvasGroup.blocksRaycasts = false;
        flashCanvasGroup.interactable = false;
    }
}