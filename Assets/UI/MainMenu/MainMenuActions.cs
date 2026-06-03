using UnityEngine;
using UnityEngine.SceneManagement; // CRITICAL: This gives us scene-switching powers!

public class MainMenuActions : MonoBehaviour
{
    [Header("Configuration")]
    [Tooltip("The exact name of your gameplay scene as written in your Project files.")]
    public string gameplaySceneName = "GameplayScene";

    /// <summary>
    /// Call this from the Play Button
    /// </summary>
    public void PlayGame()
    {
        // Smoothly transitions to your next scene
        SceneManager.LoadScene(gameplaySceneName);
    }

    /// <summary>
    /// Call this from the Quit Button
    /// </summary>
    public void QuitGame()
    {
        Debug.Log("Quit button pressed! Closing application...");

        // This closes the game completely (only works in a standalone built .exe/.app)
        Application.Quit();

        // Optional Quality-of-Life: This stops Play Mode right inside the Unity Editor 
        // so you don't have to wonder if the button actually worked!
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}