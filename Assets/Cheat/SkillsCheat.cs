using UnityEngine;
using UnityEngine.InputSystem;

public class SkillCheat : MonoBehaviour
{
    private bool toggleCheat = true;
    void Update()
    {
        if (Keyboard.current != null && Keyboard.current.gKey.wasPressedThisFrame)
        {
            if (Player.Instance.stats != null && Player.Instance.inventory != null)
            {
                if (toggleCheat)
                {
                    Player.Instance.stats.originalSP = 9999;
                    Player.Instance.stats.SP = 9999;
                    Player.Instance.inventory.hasAbsorption = true;
                    Player.Instance.inventory.hasBite = true;
                    Player.Instance.inventory.hasFireball = true;
                    Player.Instance.inventory.hasFracture = true;


                    Debug.Log($"Skill cheat activated");
                    toggleCheat = false;
                }
                else
                {
                    Player.Instance.stats.SP = 6;
                    Player.Instance.stats.originalSP = 6;
                    Player.Instance.inventory.hasAbsorption = false;
                    Player.Instance.inventory.hasBite = false;
                    Player.Instance.inventory.hasFireball = false;
                    Player.Instance.inventory.hasFracture = false;
                    toggleCheat = true;
                    Debug.Log($"Skill cheat desabled");
                }
            }
        }
    }
}
