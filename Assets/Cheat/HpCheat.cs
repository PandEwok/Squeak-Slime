using UnityEngine;
using UnityEngine.InputSystem;

public class HpCheat : MonoBehaviour
{
    bool toggleCheat = true;
    void Update()
    {
        if (Keyboard.current != null && Keyboard.current.hKey.wasPressedThisFrame)
        {
            if (Player.Instance.stats != null)
            {
                if (toggleCheat)
                {
                    Player.Instance.stats.health = 9999;
                    Player.Instance.stats.originalHealth = 9999;


                    Debug.Log($"Health cheat activated");
                    toggleCheat = false;
                }
                else
                {
                    Player.Instance.stats.health = 30;
                    Player.Instance.stats.originalHealth = 30;
                    toggleCheat = true;
                    Debug.Log($"Health cheat desabled");
                }
            }
        }
    }
}
