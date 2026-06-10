using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

[CreateAssetMenu(fileName = "MeleeAttack", menuName = "PlayerAction/MeleeAttack")]
public class MeleeAttack : PlayerAction
{
    [SerializeField] private bool isBite;
    [SerializeField] private float biteBoost;
    private readonly float movingTowardsTargetDuration = 0.6f;
    private readonly float qteWindowDuration = 0.2f;
    private readonly float waitAfterDamageDuration = 0.5f;
    private WaitForSeconds _cachedWaitAfterDamageDuration;
    private WaitForSeconds CachedWaitAfterDamageDuration
    {
        get
        {
            if (_cachedWaitAfterDamageDuration == null)
            {
                _cachedWaitAfterDamageDuration = new WaitForSeconds(waitAfterDamageDuration);
            }
            return _cachedWaitAfterDamageDuration;
        }
    }
    public override void Execute(Player player, GameObject target) 
    {
        player.StartCoroutine(AttackFrontSequence(player, target));
    }

    public IEnumerator AttackFrontSequence(Player player, GameObject target)
    {
        Vector3 enemyPos = target.transform.position;
        Vector3 direction = (enemyPos - player.originalPosition).normalized;
        float stopDistance = 2.5f;
        Vector3 targetPos = enemyPos - (direction * stopDistance);
        Vector3 playerOriginalPosition = player.originalPosition;
        var stats = player.GetComponent<Stats_System>();
        var playerTransform = player.transform;


        // ALLER
        AudioManager.Instance.PlayLoopingSFX(slimeMovingSound);
        float elapsed = 0;
        bool hasFailedQTE = false;
        while (elapsed < movingTowardsTargetDuration)
        {
            playerTransform.position = Vector3.Lerp(playerOriginalPosition, targetPos, elapsed / movingTowardsTargetDuration);
            if (Pointer.current.press.wasPressedThisFrame)
            {
                hasFailedQTE = true;
                Debug.Log("QTE Failed");
                player.uiManager.DisplayGrade(GradeScript.Grade.Missed, true);
            }
            elapsed += Time.deltaTime;
            yield return null;
        }
        AudioManager.Instance.StopLoopingSFX();
        playerTransform.position = targetPos;



        float qteElapsed = 0f;
        bool succeededQte = false;
        int baseDamage;
        if (isBite)
        {
            baseDamage = (int)(stats.damage + (stats.damage * biteBoost));
        }
        else
        {
            baseDamage = stats.damage;
        }


        if (!hasFailedQTE)
        {
            player.uiManager.ShowQTE(true);
            while (qteElapsed < qteWindowDuration)
            {
                //Clic gauche souris
                if (Pointer.current.press.wasPressedThisFrame)
                {
                    succeededQte = true;
                    Debug.Log("Coup Critique");
                    break;
                }

                qteElapsed += Time.deltaTime;
                yield return null;
            }
        player.uiManager.ShowQTE(false);
        }
        // DEGATS
        if (succeededQte)
        {
            player.uiManager.DisplayGrade(GradeScript.Grade.Excellent, true);
        }
        var enemyStats = target.GetComponent<Stats_System>();
        if (enemyStats != null)
        {
            Debug.Log($"Inflige des dégâts à {target.name}");
            int finalDamage = succeededQte ? Mathf.RoundToInt(baseDamage * qteSuccessDamageBoost) : baseDamage;
            finalDamage = player.stats.HasCriticalHit(finalDamage);

            int healthToAbsorb = target.GetComponent<Stats_System>().TakeDamage(finalDamage, false);
            AudioManager.Instance.PlaySFX(attackSoundName);
            player.stats.AbsorbHealth(healthToAbsorb);
            if(isBite)
            {
                enemyStats.MakeBleeding();
            }
            yield return CachedWaitAfterDamageDuration;
        }

        // RETOUR
        elapsed = 0;
        AudioManager.Instance.PlayLoopingSFX(slimeMovingSound);
        while (elapsed < movingTowardsTargetDuration)
        {
            playerTransform.position = Vector3.Lerp(targetPos, playerOriginalPosition, elapsed / movingTowardsTargetDuration);
            Debug.DrawLine(playerOriginalPosition, targetPos, Color.red);

            elapsed += Time.deltaTime;
            yield return null;
        }

        playerTransform.position = playerOriginalPosition;
        AudioManager.Instance.StopLoopingSFX();
        player.SwitchingTurn();
    }
}
