using UnityEngine;
using TMPro;
using System.Collections;

public class ShopkeeperDialogue : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI speechText;

    [Header("Dialogue Content")]
    [TextArea(2, 5)] // Makes the text box bigger in the Inspector
    public string[] dialogues;

    // Making this 'static' means your SettingsMenu script can easily talk to it later!
    // A smaller number means faster typing.
    public static float typeSpeed = 0.05f;

    private int currentIndex = 0;
    private Coroutine typingCoroutine;

    void Start()
    {
        // Ensure the text box is empty when the shop first opens
        if (speechText != null) speechText.text = "";
    }

    // Attach this to your "Talk" Button!
    public void TalkToShopkeeper()
    {
        // If the text is currently typing, ignore the button click so it doesn't overlap
        if (typingCoroutine != null) return;

        // Start the typewriter effect with the current line of dialogue
        typingCoroutine = StartCoroutine(TypeWriterEffect(dialogues[currentIndex]));
    }

    private IEnumerator TypeWriterEffect(string line)
    {
        speechText.text = ""; // Clear the previous text

        // Loop through every single letter in the sentence
        foreach (char letter in line.ToCharArray())
        {
            speechText.text += letter; // Add the letter
            yield return new WaitForSeconds(typeSpeed); // Wait a tiny bit
        }

        // The sentence is fully typed out!
        // Move to the next dialogue line, but if we are at the end, stay on the last one.
        if (currentIndex < dialogues.Length - 1)
        {
            currentIndex++;
        }

        // Set the coroutine to null so the player can click the button again
        typingCoroutine = null;
    }
}