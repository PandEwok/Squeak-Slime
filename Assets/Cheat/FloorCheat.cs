using UnityEngine;
using UnityEngine.InputSystem;

public class FloorCheat : MonoBehaviour
{
    void Update()
    {
        if (Keyboard.current != null && Keyboard.current.fKey.wasPressedThisFrame)
        {
            if (Player.Instance != null)
            {
                Player.Instance.floor++;
                Debug.Log($"Floor cheat activated. Current floor : {Player.Instance.floor}");

                if (Player.Instance.floor > Player.Instance.maxFloor)
                {
                    Player.Instance.floor = 1;
                }
            }
        }
    }
}