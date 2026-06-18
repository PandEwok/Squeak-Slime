using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using static UnityEngine.RuleTile.TilingRuleOutput;
using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime.Misc;

[CreateAssetMenu(fileName = "FractureAttack", menuName = "PlayerAction/FractureAttack")]
public class FractureAttack : PlayerAction
{
    [SerializeField] private GameObject darkLightningPrefab;
    [SerializeField] private float qteFailWindow = 0.3f;
    [SerializeField] private float qteDuration = 0.5f;
    [SerializeField] private int segmentsCount = 10;          //Qtt zigzags
    [SerializeField] private float jitterAmount = 0.5f;       //Force des déviations
    [SerializeField] private float propagationSpeed = 0.025f; //Vitesse entre chaque segment
    [SerializeField] private float delayBeforeDestroyingFracture = 0.2f;
    private WaitForSeconds _cachedDelayBeforeDestroyingFracture;
    private WaitForSeconds CachedDelayBeforeDestroyingFracture
    {
        get
        {
            if (_cachedDelayBeforeDestroyingFracture == null)
            {
                _cachedDelayBeforeDestroyingFracture = new WaitForSeconds(delayBeforeDestroyingFracture);
            }
            return _cachedDelayBeforeDestroyingFracture;
        }
    }
    private float delayBeforeSwitchingTurn = 0.3f;
    private WaitForSeconds _cachedDelayBeforeSwitchingTurn;
    private WaitForSeconds CachedDelayBeforeSwitchingTurn
    {
        get
        {
            if (_cachedDelayBeforeSwitchingTurn == null)
            {
                _cachedDelayBeforeSwitchingTurn = new WaitForSeconds(delayBeforeSwitchingTurn);
            }
            return _cachedDelayBeforeSwitchingTurn;
        }
    }
    public override void Execute(Player player, GameObject target)
    {
        player.StartCoroutine(AttackFractureSequence(player, target));
    }

    public IEnumerator AttackFractureSequence(Player player, GameObject target)
    {
        bool hasFailedEarly = false;
        bool qteSteady = false;
        float qteFailElapsed = 0f;
        var playerTransform = player.transform;
        var stats = player.GetComponent<Stats_System>();

        Debug.Log("Entering Fracture delay");

        while (qteFailElapsed < qteFailWindow)
        {
            if(!qteSteady)
            {
                qteSteady = true;
                player.uiManager.ShowQTE(true, false);
            }
            if (Pointer.current.press.wasPressedThisFrame)
            {
                Debug.Log("Fracture: QTE failed");
                hasFailedEarly = true;
                player.uiManager.DisplayGrade(GradeScript.Grade.Missed, true);
                player.uiManager.ShowQTE(false);
            }

            qteFailElapsed += Time.deltaTime;
            yield return null;
        }

        if (target == null || darkLightningPrefab == null) yield break;

        Vector3 startPos = playerTransform.position;
        Vector3 endPos = target.transform.position;

        
        float elapsed = 0f;
        bool hasQTESuccess = false;
        Debug.Log("QTE entered");

        if (!hasFailedEarly)
        {
            player.uiManager.ShowQTE(true, true);
            while (elapsed < qteDuration)
            {
                if (Pointer.current.press.wasPressedThisFrame)
                {
                    hasQTESuccess = true;
                    elapsed = qteDuration;
                }

                elapsed += Time.deltaTime;
                yield return null;
            }
            player.uiManager.ShowQTE(false);
        }
        if (hasQTESuccess) { player.uiManager.DisplayGrade(GradeScript.Grade.Excellent, true); }
        GameObject lightningInstance = Instantiate(darkLightningPrefab, startPos, Quaternion.identity);
        AudioManager.Instance.PlaySFX(attackSoundName);

        LineRenderer lightningLR = lightningInstance.GetComponent<LineRenderer>();

        if (lightningLR == null)
        {
            Debug.LogError("Erreur: LR manquant sur l'effet Fracture");
            Destroy(lightningInstance);
            player.SwitchingTurn();
            yield break;
        }
        List<Vector3> points = new List<Vector3>();
        points.Add(startPos);


        Vector3 direction = (endPos - startPos).normalized;
        float totalDistance = Vector3.Distance(startPos, endPos);
        float segmentLength = totalDistance / segmentsCount;

        Vector3 perpendicular = new Vector3(-direction.y, direction.x, 0f).normalized; //Axe deviation eclair

        for (int i = 1; i < segmentsCount; i++)
        {
            Vector3 linearPos = startPos + direction * (segmentLength * i);

            float randomOffset = Random.Range(-jitterAmount, jitterAmount);
            Vector3 jitterPos = linearPos + perpendicular * randomOffset;

            points.Add(jitterPos);
        }

        points.Add(endPos);


        lightningLR.positionCount = 1;
        lightningLR.SetPosition(0, startPos);

        for (int i = 1; i < points.Count; i++)
        {
            lightningLR.positionCount = i + 1;
            lightningLR.SetPosition(i, points[i]);

            yield return new WaitForSeconds(propagationSpeed);
        }

        //Impact
        Stats_System enemyStats = target.GetComponent<Stats_System>();
        if (enemyStats != null)
        {
            int finalDamage = hasQTESuccess ? Mathf.RoundToInt(stats.damage * qteSuccessDamageBoost) : stats.damage;
            finalDamage = player.stats.HasCriticalHit(finalDamage);
            int healthToAbsorb = enemyStats.TakeDamage(finalDamage, true, attackDirectionBoost);
            player.stats.AbsorbHealth(healthToAbsorb);
            target.GetComponent<Stats_System>().MakeDizzy();
        }


        yield return CachedDelayBeforeDestroyingFracture;

        Destroy(lightningInstance);

        yield return CachedDelayBeforeSwitchingTurn;
        player.SwitchingTurn();
    }
}
