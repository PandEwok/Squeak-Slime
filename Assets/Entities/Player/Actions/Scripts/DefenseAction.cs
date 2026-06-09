using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

[CreateAssetMenu(fileName = "DefenseAction", menuName = "PlayerAction/DefenseAction")]
public class DefenseAction : PlayerAction
{
    public override void Execute(Player player, float windowDuration)
    {
        player.StartCoroutine(TriggerDefenseQTE(player, windowDuration));
    }

    public IEnumerator TriggerDefenseQTE(Player player, float windowDuration)
    {
        var stats = player.stats;
        stats.blocking = false;
        float elapsed = 0f;

        Debug.Log("Def QTE");
        player.ShowQTE(true);
        while (elapsed < windowDuration)
        {
            if (Pointer.current.press.wasPressedThisFrame)
            {
                stats.blocking = true;
                Debug.Log("Blocked!");
                break;
            }

            elapsed += Time.deltaTime;
            yield return null;
        }
        player.ShowQTE(false);
        if (stats.blocking)
        {
            player.DisplayGrade(GradeScript.Grade.Excellent, true);
        }

    }
}
