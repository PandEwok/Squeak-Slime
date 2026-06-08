using UnityEngine;

public class SkillsMenuController : MonoBehaviour
{
    private Animator panelAnimator;

    [Header("External Elements")]
    [Tooltip("Drag the external flipping button GameObject here!")]
    public Animator flipButtonAnimator;

    private void Awake()
    {
        // Grab the animator on the Skill Panel itself
        panelAnimator = GetComponent<Animator>();
    }

    /// <summary>
    /// Call this from your main Lobby "Open Skills" Button
    /// </summary>
    public void OpenSkillsPanel()
    {
        // 1. Open the main panel
        if (panelAnimator != null)
        {
            panelAnimator.SetBool("IsOpen", true);
        }

        // 2. NEW: Tell the external button to flip!
        if (flipButtonAnimator != null)
        {
            flipButtonAnimator.SetBool("IsOpen", true);
        }
    }

    /// <summary>
    /// Call this from the "X" or "Close" Button inside the skills menu
    /// </summary>
    public void CloseSkillsPanel()
    {
        // 1. Close the main panel
        if (panelAnimator != null)
        {
            panelAnimator.SetBool("IsOpen", false);
        }

        // 2. NEW: Tell the external button to flip back to normal!
        if (flipButtonAnimator != null)
        {
            flipButtonAnimator.SetBool("IsOpen", false);
        }
    }
}