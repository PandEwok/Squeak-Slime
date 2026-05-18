using UnityEngine;
using UnityEngine.SceneManagement;

public class HubManager : MonoBehaviour
{
    public Animator panelAnimator;

    public void StartGame()
    {
        SceneManager.LoadScene("SampleScene 2");
    }

    public void OpenUpgrades()
    {
        // Must match the name of the BOX in the Animator window exactly!
        panelAnimator.Play("UpgradePanel_SlideIn");
    }

    public void CloseUpgrades()
    {
        // Must match the name of the BOX in the Animator window exactly!
        panelAnimator.Play("UpgradePanel_SlideOut");
    }
}