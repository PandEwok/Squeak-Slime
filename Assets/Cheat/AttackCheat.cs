using UnityEngine;
using UnityEngine.InputSystem;

public class AttackCheat : MonoBehaviour
{
    private bool toggleCheat = true;

    void Update()
    {
        if (Keyboard.current != null && Keyboard.current.dKey.wasPressedThisFrame)
        {
            if (Player.Instance.stats != null && Player.Instance.inventory != null)
            {
                if (toggleCheat)
                {
                    Player.Instance.stats.damage = 9999;
                    Player.Instance.stats.baseDamage = 9999;
                    Debug.Log($"Attack cheat activated");
                    toggleCheat = false;
                }
                else
                {
                    Player.Instance.stats.damage = 9;
                    Player.Instance.stats.baseDamage = 9;
                    toggleCheat = true;
                    Debug.Log($"Attack cheat desabled");
                }
            }
        }
    }
}
