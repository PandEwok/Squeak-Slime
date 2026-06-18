using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;

[CreateAssetMenu(fileName = "Fireball", menuName = "PlayerAction/Fireball")]
public class FireballAttack : PlayerAction
{
    [SerializeField] private GameObject fireball;
    [SerializeField] private int clicksToSucceed = 10;
    [SerializeField] private float chargeDuration = 2f;
    [SerializeField] private string fireballMovingSoundName;
    [SerializeField] private float distanceThreshold = 0.2f; //Distance a laquelle la boule de feu est consideree comme ayant atteint sa cible
    [SerializeField] private float projectileSpeed = 10f;
    [SerializeField] private float postAttackDelay = 0.5f;
    private WaitForSeconds _cachedPostAttackDelay;
    private WaitForSeconds CachedPostAttackDelay
    {
        get
        {
            if (_cachedPostAttackDelay == null)
            {
                _cachedPostAttackDelay = new WaitForSeconds(postAttackDelay);
            }
            return _cachedPostAttackDelay;
        }
    }

    public override void Execute(Player player, List<GameObject> targets)
    {
        player.StartCoroutine(AttackFireSequence(player, targets));
    }
    public IEnumerator AttackFireSequence(Player player, List<GameObject> targets)
    {
        var stats = player.GetComponent<Stats_System>();
        var playerTransform = player.transform;


        //QTE
        
        float elapsedCharge = 0f;
        int clickCount = 0;

        Debug.Log("QTE entered");

        player.uiManager.ShowQTE(true, true);
        player.uiManager.spamIndicator.SetActive(true);
        while (elapsedCharge < chargeDuration)
        {
            if (Pointer.current.press.wasPressedThisFrame)
            {
                clickCount++;
            }

            elapsedCharge += Time.deltaTime;
            yield return null;
        }
        player.uiManager.spamIndicator.SetActive(false);
        player.uiManager.ShowQTE(false);
        Debug.Log($"Nb of clicks: {clickCount}");

        //Securite
        if (targets == null || targets.Count == 0)
        {
            Debug.LogWarning("Liste ennemie vide");
            player.SwitchingTurn();
            yield break;
        }

        // Calcul des degats
        bool successed = clickCount >= clicksToSucceed;
        int baseDamage = stats.damage;
        int finalDamage = successed ? Mathf.RoundToInt(stats.damage * qteSuccessDamageBoost) : 0;
        if (!successed)
        {
            Debug.Log("QTE failed, no damage dealt.");
            player.SwitchingTurn();
            player.uiManager.DisplayGrade(GradeScript.Grade.Missed, true);
            yield break;
        }
        else { player.uiManager.DisplayGrade(GradeScript.Grade.Excellent, true); }

        //BDF
        List<Coroutine> activeProjectiles = new List<Coroutine>();


        foreach (GameObject enemy in targets)
        {
            if (enemy == null) continue;

            Vector3 spawnPos = playerTransform.position;

            GameObject fb = Instantiate(fireball, spawnPos, Quaternion.identity);
            AudioManager.Instance.PlaySFX(fireballMovingSoundName);

            Coroutine projMovement = player.StartCoroutine(MoveProjectileToTarget(fb, enemy, finalDamage, player));
            activeProjectiles.Add(projMovement);
        }

        foreach (Coroutine projRoutine in activeProjectiles)
        {
            yield return projRoutine;
        }

        yield return CachedPostAttackDelay;
        player.SwitchingTurn();
    }

    private IEnumerator MoveProjectileToTarget(GameObject proj, GameObject target, int damage, Player player)
    {
        if (proj == null || target == null) yield break;

        Stats_System enemyStats = target.GetComponent<Stats_System>();
        Vector3 targetPos = target.transform.position;
        while (proj != null && target != null)
        {
            proj.transform.position = Vector3.MoveTowards(proj.transform.position, targetPos, projectileSpeed * Time.deltaTime);

            if (Vector3.Distance(proj.transform.position, targetPos) <= distanceThreshold)
            {
                break;
            }

            yield return null;
        }

        //Impact
        if (target != null && enemyStats != null)
        {
            int finalDamage = player.stats.HasCriticalHit(damage);
            int healthToAbsorb = enemyStats.TakeDamage(finalDamage, false);
            player.stats.AbsorbHealth((int)healthToAbsorb);
            enemyStats.MakeBurned();
        }

        if (proj != null)
        {
            Destroy(proj);
        }
        AudioManager.Instance.PlaySFX(attackSoundName);
    }
}
