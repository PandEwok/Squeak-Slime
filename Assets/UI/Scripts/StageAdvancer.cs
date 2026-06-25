using UnityEngine;
using UnityEngine.SceneManagement; // Required for changing scenes

public class StageAdvancer : MonoBehaviour
{
    [Tooltip("Type the exact name of your combat or map scene here")]
    public string nextSceneName = "SampleScene 2";

    // We will attach this function to your "Leave" buttons
    public void FinishEventAndAdvance()
    {
        // 1. Tell the invisible GameManager to increase the stage numbers
        if (GameManager.Instance != null)
        {
            GameManager.Instance.AdvanceStage();
        }
        else
        {
            Debug.LogWarning("GameManager is missing! Make sure you started from the Main Menu.");
        }

        // 2. Load the next scene to continue the loop
        Player.Instance.SwitchSceneInCaseOfVictory();
    }
}