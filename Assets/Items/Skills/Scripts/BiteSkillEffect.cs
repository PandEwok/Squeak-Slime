using UnityEngine;

// This tag adds a button in Unity so you can right-click and create this asset!
[CreateAssetMenu(fileName = "Bite Skill Effect", menuName = "Skills/Effects/Bite")]
public class BiteSkillEffect : CustomEffectLogic
{
    public override void ExecuteEffect(GameObject user, GameObject target)
    {
        // 1. Grab the PlayerScript from the character using the skill
        Player playerS = user.GetComponent<Player>();

        if (playerS != null && target != null)
        {
            Debug.Log("<color=green>[Modular Skill] Executing Bite Effect!</color>");

            // 2. Trigger your friends' original Coroutine!
            playerS.StartCoroutine(playerS.AttackBiteSequence(target));
        }
        else
        {
            Debug.LogWarning("Bite Skill failed: Missing user or target.");
        }
    }
}