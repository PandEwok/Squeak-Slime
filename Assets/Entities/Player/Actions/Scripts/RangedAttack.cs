using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

[CreateAssetMenu(fileName = "RangedAttack", menuName = "PlayerAction/RangedAttack")]
public class RangedAttack : PlayerAction
{
    [SerializeField] private GameObject projectile;
    [SerializeField] private string projectilePreparationSound;
    [SerializeField] private string projectileMovingSound;
    [SerializeField] private float preparationDistance = 10f; //Fin deplacement avant le lancer
    [SerializeField] private float movingTowardsTargetDuration = 0.6f;
    [SerializeField] private float projectileHeight = 4.0f;
    [SerializeField] private float projectileThrowedDuration = 0.5f;
    [SerializeField] private float qteWindowFrame = 0.6f;
    [SerializeField] private float projectilePreparationDuration = 0.3f;
    [SerializeField] private float waitAfterDamageDuration = 0.3f;
    private WaitForSeconds _cachedProjectilePreparationDuration;
    private WaitForSeconds CachedProjectilePreparationDuration
    {
        get
        {
            if (_cachedProjectilePreparationDuration == null)
            {
                _cachedProjectilePreparationDuration = new WaitForSeconds(projectilePreparationDuration);
            }
            return _cachedProjectilePreparationDuration;
        }
    }
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
        player.StartCoroutine(AttackRangedSequence(player, target));
    }

    public IEnumerator AttackRangedSequence(Player player, GameObject target)
    {
        Vector3 startPos = player.originalPosition;
        Vector3 enemyPos = target.transform.position;
        Vector3 direction = (enemyPos - startPos).normalized;
        var playerTransform = player.transform;
        var stats = player.stats;


        
        Vector3 prepPos = enemyPos - (direction * preparationDistance);
        Vector3 arrivalPos = enemyPos;

        float elapsed = 0;

        //APPROCHE
        AudioManager.Instance.PlayLoopingSFX(slimeMovingSound);
        while (elapsed < movingTowardsTargetDuration)
        {
            playerTransform.position = Vector3.Lerp(startPos, prepPos, elapsed / movingTowardsTargetDuration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        AudioManager.Instance.StopLoopingSFX();
        AudioManager.Instance.PlaySFX(projectilePreparationSound);
        yield return CachedProjectilePreparationDuration;
        //LANCER
        GameObject projectileToThrow = Instantiate(projectile, prepPos, Quaternion.identity);
        elapsed = 0;
        
        bool succeededQte = false;
        bool qteWindowOpen = false;
        bool hasFailedQTE = false; //Si le joueur appuie trop tot (ne pas confondre avec de pas avoir appuye du tout)
        AudioManager.Instance.PlaySFX(projectileMovingSound);
        while (elapsed < projectileThrowedDuration)
        {
            float t = elapsed / projectileThrowedDuration;
            if (t < qteWindowFrame && !succeededQte)
            {
                if (Pointer.current.press.wasPressedThisFrame)
                {
                    hasFailedQTE = true;
                    Debug.Log("QTE Failed");
                    player.uiManager.DisplayGrade(GradeScript.Grade.Missed, true);
                }
            }
            if (t >= qteWindowFrame && !succeededQte && !hasFailedQTE)
            {
                if (!qteWindowOpen)
                {
                    qteWindowOpen = true;
                    player.uiManager.ShowQTE(true);
                }

                if (Pointer.current.press.wasPressedThisFrame && !hasFailedQTE)
                {
                    succeededQte = true;
                    player.uiManager.ShowQTE(false);
                    Debug.Log("Coup critique");
                }
            }

            //Mouvement horizontal
            Vector3 currentPos = Vector3.Lerp(prepPos, enemyPos, t);

            //Courbe
            float height = Mathf.Sin(Mathf.PI * t) * projectileHeight;
            currentPos.y += height;

            if (projectileToThrow != null)
            {
                projectileToThrow.transform.position = currentPos;
            }

            elapsed += Time.deltaTime;
            yield return null;
        }
        player.uiManager.ShowQTE(false);
        //DEGATS
        if (projectileToThrow != null)
        {
            Destroy(projectileToThrow);
        }
        AudioManager.Instance.PlaySFX("Player_Proj_Impact");
        var enemyStats = target.GetComponent<Stats_System>();
        if (enemyStats != null)
        {
            int baseDamage = stats.damage;
            int finalDamage = succeededQte ? Mathf.RoundToInt(baseDamage * qteSuccessDamageBoost) : baseDamage;
            int healthToAbsorb = enemyStats.TakeDamage(finalDamage, false);
            player.stats.AbsorbHealth(healthToAbsorb);
        }
        if (succeededQte)
        {
            player.uiManager.DisplayGrade(GradeScript.Grade.Excellent, true);
        }
        yield return CachedWaitAfterDamageDuration;

        //RETOUR
        elapsed = 0;
        Vector3 impactPos = playerTransform.position;
        AudioManager.Instance.PlayLoopingSFX("Slime_Moving");
        while (elapsed < movingTowardsTargetDuration)
        {
            playerTransform.position = Vector3.Lerp(impactPos, startPos, elapsed / movingTowardsTargetDuration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        playerTransform.position = startPos;
        AudioManager.Instance.StopLoopingSFX();
        player.SwitchingTurn();

    }
}
