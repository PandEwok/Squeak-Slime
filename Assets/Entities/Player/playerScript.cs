using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEngine.GraphicsBuffer;

public class PlayerScript : MonoBehaviour
{
    Vector3 originalPosition;
    [Header("Game scripts")]
    public Combat_Logic combatLogic;
    private Stats_System stats;
    [Header("Attack Prefabs")]
    [SerializeField] private GameObject projectile;
    [SerializeField] private GameObject fireball;
    [Header("Player only Stats")]
    public int originalSP = 100;
    public int SP = 100;
    public float attackBoostStrenght = 0.5f;
    public float defenseBoostStrenght = 0.5f;
    public int baseDamage = 0;
    public int baseDefense = 0;
    protected int empowerDelay = 0;
    protected int defenseBuffDelay = 0;
    protected bool empowered = false;
    protected bool defenseBuffed = false;
    protected float particleSpawnTimer = 0;
    [HideInInspector] public bool hasWon = false;
    [Header("UI Elements")]
    public GameObject gameOverUI;
    public GameObject victoryUI;
    private GameObject actionMenu;
    private GameObject qteWarning;
    private GradeScript gradeScript;
    [Header("VFX Prefabs")]
    [SerializeField] private GameObject darkLightningPrefab;
    [SerializeField] private GameObject attackBoostEffect;
    [SerializeField] private GameObject defenseBoostEffect;
    [HideInInspector] public SpriteRenderer sprite;

    private void Awake()
    {
        Transform gradeTransform = transform.Find("GradeDisplay");
        Transform actionTransform = transform.Find("ActionMenu");
        Transform qteWarningTransform = transform.Find("QTEWarning");
        Transform spriteTransform = transform.Find("slime");
        if (gradeTransform != null)
        {
            gradeScript = gradeTransform.GetComponent<GradeScript>();
        }
        else
        {
            Debug.LogError("GradeDisplay object not found as a child of the player.");
        }
        if (actionTransform != null)
        {
            actionMenu = actionTransform.gameObject;
        }
        else
        {
            Debug.LogError("ActionMenu object not found as a child of the player.");
        }
        if (qteWarningTransform != null)
        {
            qteWarning = qteWarningTransform.gameObject;
        }
        else
        {
            Debug.LogError("QTE Warning object not found as a child of the player.");
        }
        if (spriteTransform != null)
        {
            sprite = spriteTransform.gameObject.GetComponent<SpriteRenderer>();
        }
        else
        {
            Debug.LogError("Sprite object not found as a child of the player.");
        }
    }
    private void Start()
    {
        stats = GetComponent<Stats_System>();
        originalPosition = transform.position;
        baseDamage = stats.damage;
        baseDefense = stats.defense;
        gameOverUI.SetActive(false);
        victoryUI.SetActive(false);
    }

    private void Update()
    {
        if (!hasWon)
        {
            particleSpawnTimer += Time.deltaTime;

            empowered = (empowerDelay > 0);
            if (empowered && particleSpawnTimer > 0.1f)
            {
                particleSpawnTimer = 0;
                for (int i = 0; i < 4; i++)
                {
                    float randomX = Random.Range(-0.6f, 0.6f);
                    float randomY = Random.Range(-0.25f, 0.25f);
                    Instantiate(attackBoostEffect, this.transform.position + new Vector3(randomX, -0.2f + randomY, 0), Quaternion.identity, this.transform);
                }
            }
            defenseBuffed = (defenseBuffDelay > 0);
            if (defenseBuffed && particleSpawnTimer > 0.1f)
            {
                particleSpawnTimer = 0;
                for (int i = 0; i < 4; i++)
                {
                    float randomX = Random.Range(-0.6f, 0.6f);
                    float randomY = Random.Range(-0.25f, 0.25f);
                    Instantiate(defenseBoostEffect, this.transform.position + new Vector3(randomX, -0.2f + randomY, 0), Quaternion.identity, this.transform);
                }
            }
        }
    }
    public IEnumerator AttackFrontSequence(GameObject target, float boost, bool isBite)
    {
        Vector3 enemyPos = target.transform.position;
        Vector3 direction = (enemyPos - originalPosition).normalized;
        float stopDistance = 2.5f;

        Vector3 targetPos = enemyPos - (direction * stopDistance);

        // ALLER
        AudioManager.Instance.PlayLoopingSFX("Slime_Moving");
        float elapsed = 0;
        float duration = 0.6f;
        bool hasFailedQTE = false;
        while (elapsed < duration)
        {
            transform.position = Vector3.Lerp(originalPosition, targetPos, elapsed / duration);
            if (Pointer.current.press.wasPressedThisFrame)
            {
                hasFailedQTE = true;
                Debug.Log("QTE Failed");
                DisplayGrade(GradeScript.Grade.Missed, true);
            }
            elapsed += Time.deltaTime;
            yield return null;
        }
        AudioManager.Instance.StopLoopingSFX();
        transform.position = targetPos;

        
        
        float qteWindow = 0.2f;
        float qteElapsed = 0f;
        bool hasCrit = false;
        int baseDamage = (int)(stats.damage + (stats.damage * boost));
        
        if (!hasFailedQTE)
        {
            ShowQTE(true);
            while (qteElapsed < qteWindow)
            {
                //Clic gauche souris
                if (Pointer.current.press.wasPressedThisFrame)
                {
                    hasCrit = true;
                    Debug.Log("Coup Critique");
                    break;
                }

                qteElapsed += Time.deltaTime;
                yield return null;
            }
        ShowQTE(false);
        }
        // DEGATS
        if (hasCrit)
        {
            DisplayGrade(GradeScript.Grade.Excellent, true);
        }
        var enemyStats = target.GetComponent<Stats_System>();
        if (enemyStats != null)
        {
            Debug.Log($"Inflige des dégâts à {target.name}");
            int finalDamage = hasCrit ? Mathf.RoundToInt(baseDamage * 1.5f) : baseDamage;


            int healthToAbsorb = target.GetComponent<Stats_System>().takeDamage(finalDamage, false);
            if(isBite)
            {
                AudioManager.Instance.PlaySFX("Player_Bite");
            }
            else
            {
                AudioManager.Instance.PlaySFX("Player_Melee");
            }
                AbsorbHealth(healthToAbsorb);
            yield return new WaitForSeconds(0.5f);
        }

        // RETOUR
        elapsed = 0;
        AudioManager.Instance.PlayLoopingSFX("Slime_Moving");
        while (elapsed < duration)
        {
            transform.position = Vector3.Lerp(targetPos, originalPosition, elapsed / duration);
            Debug.DrawLine(originalPosition, targetPos, Color.red);

            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.position = originalPosition;
        AudioManager.Instance.StopLoopingSFX();
        SwitchingTurn();
    }

    public IEnumerator AttackRangedSequence(GameObject target, float boost)
    {
        Vector3 startPos = originalPosition;
        Vector3 enemyPos = target.transform.position;
        Vector3 direction = (enemyPos - startPos).normalized;


        float prepDistance = 10f; //Fin deplacement avant le lancer
        Vector3 prepPos = enemyPos - (direction * prepDistance);
        Vector3 arrivalPos = enemyPos;

        float duration = 0.6f;
        float elapsed = 0;

        //APPROCHE
        AudioManager.Instance.PlayLoopingSFX("Slime_Moving");
        while (elapsed < duration)
        {
            transform.position = Vector3.Lerp(startPos, prepPos, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        AudioManager.Instance.StopLoopingSFX();
        AudioManager.Instance.PlaySFX("Player_Prepare_Proj");
        yield return new WaitForSeconds(0.3f);
        //LANCER
        GameObject projectileToThrow = Instantiate(projectile, prepPos, Quaternion.identity);
        elapsed = 0;
        float jumpHeight = 4.0f;
        float jumpDuration = 0.5f;
        bool hasCrit = false;
        bool qteWindowOpen = false;
        bool hasFailedQTE = false;
        AudioManager.Instance.PlaySFX("Player_Proj_Moving");
        while (elapsed < jumpDuration)
        {
            float t = elapsed / jumpDuration;
            if (t < 0.6f && !hasCrit)
            {
                if(Pointer.current.press.wasPressedThisFrame)
                {
                    hasFailedQTE = true;
                    Debug.Log("QTE Failed");
                    DisplayGrade(GradeScript.Grade.Missed, true);
                }
            }
            if (t >= 0.6f && !hasCrit && !hasFailedQTE)
            {
                if (!qteWindowOpen)
                {
                    qteWindowOpen = true;
                    ShowQTE(true);
                }

                if (Pointer.current.press.wasPressedThisFrame && !hasFailedQTE)
                {
                    hasCrit = true;
                    ShowQTE(false);
                    Debug.Log("Coup critique");
                }
            }

            //Mouvement horizontal
            Vector3 currentPos = Vector3.Lerp(prepPos, enemyPos, t);

            //Courbe
            float height = Mathf.Sin(Mathf.PI * t) * jumpHeight;
            currentPos.y += height;

            if (projectileToThrow != null)
            {
                projectileToThrow.transform.position = currentPos;
            }

            elapsed += Time.deltaTime;
            yield return null;
        }
        ShowQTE(false);
        //DEGATS
        if (projectileToThrow != null)
        {
            Destroy(projectileToThrow);
        }
        AudioManager.Instance.PlaySFX("Player_Proj_Impact");
        var enemyStats = target.GetComponent<Stats_System>();
        if (enemyStats != null)
        {
            int baseDamage = (int)(stats.damage + (stats.damage * boost));
            int finalDamage = hasCrit ? Mathf.RoundToInt(baseDamage * 1.5f) : baseDamage;
            int healthToAbsorb = enemyStats.takeDamage(finalDamage, false);
            AbsorbHealth(healthToAbsorb);
        }
        if (hasCrit)
        {
            DisplayGrade(GradeScript.Grade.Excellent, true);
        }
        yield return new WaitForSeconds(0.3f);

        //RETOUR
        elapsed = 0;
        Vector3 impactPos = transform.position;
        AudioManager.Instance.PlayLoopingSFX("Slime_Moving");
        while (elapsed < duration)
        {
            transform.position = Vector3.Lerp(impactPos, startPos, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        transform.position = originalPosition;
        AudioManager.Instance.StopLoopingSFX();
        SwitchingTurn();

    }

    public IEnumerator AttackBiteSequence(GameObject target)
    {
        yield return AttackFrontSequence(target, 0.5f, true);
        if (target != null)
        {
            target.GetComponent<Stats_System>().MakeBleeding();
        }
    }

    public IEnumerator AttackFireSequence(List<GameObject> targets)
    {
        //QTE
        float chargeDuration = 2f;
        float elapsedCharge = 0f;
        int clickCount = 0;

        Debug.Log("QTE entered");

        ShowQTE(true);
        while (elapsedCharge < chargeDuration)
        {
            if (Pointer.current.press.wasPressedThisFrame)
            {
                clickCount++;
            }

            elapsedCharge += Time.deltaTime;
            yield return null;
        }
        ShowQTE(false);
        Debug.Log($"Nb of clicks: {clickCount}");

        //Securite
        if (targets == null || targets.Count == 0)
        {
            Debug.LogWarning("Liste ennemie vide");
            SwitchingTurn();
            yield break;
        }

        // Calcul des degats
        bool successed = clickCount >= 10;
        int baseDamage = stats.damage;
        int finalDamage = successed ? Mathf.RoundToInt(stats.damage * 1.5f) : 0;
        if(!successed)
        {
            Debug.Log("QTE failed, no damage dealt.");
            SwitchingTurn();
            DisplayGrade(GradeScript.Grade.Missed, true);
            yield break;
        }
        else { DisplayGrade(GradeScript.Grade.Excellent, true); }

            //BDF
            List<Coroutine> activeProjectiles = new List<Coroutine>();


        foreach (GameObject enemy in targets)
        {
            if (enemy == null) continue;

            Vector3 spawnPos = transform.position;

            GameObject fb = Instantiate(fireball, spawnPos, Quaternion.identity);
            AudioManager.Instance.PlaySFX("FB");

            Coroutine projMovement = StartCoroutine(MoveProjectileToTarget(fb, enemy, 10, finalDamage));
            activeProjectiles.Add(projMovement);
        }

        foreach (Coroutine projRoutine in activeProjectiles)
        {
            yield return projRoutine;
        }
        
        yield return new WaitForSeconds(0.5f);
        SwitchingTurn();
    }

    private IEnumerator MoveProjectileToTarget(GameObject proj, GameObject target, float speed, int damage)
    {
        if (proj == null || target == null) yield break;

        Stats_System enemyStats = target.GetComponent<Stats_System>();
        float distanceThreshold = 0.2f;
        Vector3 targetPos = target.transform.position;
        while (proj != null && target != null)
        {
            proj.transform.position = Vector3.MoveTowards(proj.transform.position, targetPos, speed * Time.deltaTime);

            if (Vector3.Distance(proj.transform.position, targetPos) <= distanceThreshold)
            {
                break;
            }

            yield return null;
        }

        //Impact
        if (target != null && enemyStats != null)
        {
            int healthToAbsorb = enemyStats.takeDamage(damage, false);
            AbsorbHealth(healthToAbsorb);
            enemyStats.MakeBurned();
        }

        if (proj != null)
        {
            Destroy(proj);
        }
        AudioManager.Instance.PlaySFX("FB_Explosion");
    }

    public IEnumerator AttackFractureSequence(GameObject target)
    {
        bool hasFailedEarly = false;
        float introDuration = 0.3f;
        float introElapsed = 0f;

        Debug.Log("Entering Fracture delay");

        while (introElapsed < introDuration)
        {
            if (Pointer.current.press.wasPressedThisFrame)
            {
                Debug.Log("Fracture: QTE failed");
                hasFailedEarly = true;
                DisplayGrade(GradeScript.Grade.Missed, true);
            }

            introElapsed += Time.deltaTime;
            yield return null;
        }

        if (target == null || darkLightningPrefab == null) yield break;

        Vector3 startPos = transform.position;
        Vector3 endPos = target.transform.position;

        float duration = 0.5f;
        float elapsed = 0f;
        bool hasQTESuccess = false;
        Debug.Log("QTE entered");

        if (!hasFailedEarly)
        {
            ShowQTE(true);
            while (elapsed < duration)
            {
                if (Pointer.current.press.wasPressedThisFrame)
                {
                    hasQTESuccess = true;
                    elapsed = duration;
                }

                elapsed += Time.deltaTime;
                yield return null;
            }
            ShowQTE(false);
        }
        if(hasQTESuccess) { DisplayGrade(GradeScript.Grade.Excellent, true); }
        GameObject lightningInstance = Instantiate(darkLightningPrefab, startPos, Quaternion.identity);
        AudioManager.Instance.PlaySFX("Fracture");
        LineRenderer lightningLR = lightningInstance.GetComponent<LineRenderer>();

        if (lightningLR == null)
        {
            Debug.LogError("Erreur: LR manquant sur l'effet Fracture");
            Destroy(lightningInstance);
            SwitchingTurn();
            yield break;
        }

        int segmentsCount = 10;          //Qtt zigzags
        float jitterAmount = 0.5f;       //Force des déviations
        float propagationSpeed = 0.025f; //Vitesse entre chaque segment

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
            int finalDamage = hasQTESuccess ? Mathf.RoundToInt(stats.damage * 1.5f) : stats.damage;
            int healthToAbsorb = enemyStats.takeDamage(finalDamage, true);
            AbsorbHealth(healthToAbsorb);
            target.GetComponent<Stats_System>().MakeDizzy();
        }

        
        yield return new WaitForSeconds(0.2f);

        Destroy(lightningInstance);

        yield return new WaitForSeconds(0.3f);
        SwitchingTurn();
    }

    public void HealPlayer(float healAmount)
    {
        stats.heal((int)healAmount);
    }
    public void AbsorbHealth(int damages)
    {
        if(stats.hasAbsorption)
        {
            int healAmount = Mathf.RoundToInt(damages * 0.5f);
            HealPlayer(healAmount);
        }
    }
    public void RestoreSP(float spAmount)
    {
        SP += (int)spAmount;
        SP = Mathf.Min(SP, originalSP);
        Debug.Log($"{gameObject.name} healed for {spAmount}. Current health: {SP}");
    }

    public void ActionEmpower(int duration, float effectValue)
    {
        empowerDelay = duration;
        attackBoostStrenght = effectValue;
        AudioManager.Instance.PlaySFX("Powerup");
    }
    public void ActionDefenseBuff(int duration, float effectValue)
    {
       defenseBuffDelay = duration;
        defenseBoostStrenght = effectValue;
        AudioManager.Instance.PlaySFX("Powerup");
    }
    public void SwitchingTurn()
    {
       
        defenseBuffed = (defenseBuffDelay > 0);
        if (defenseBuffed)
        {
            stats.defense = baseDefense + (int)(baseDefense * defenseBoostStrenght);
        }
        else
        {
            stats.defense = baseDefense;
        }
        combatLogic.switchTurn();
    }
    public void ApplyAttackBoost()
    {
        empowered = (empowerDelay > 0);
        if (empowered)
        {
            stats.damage = baseDamage + (int)(baseDamage * attackBoostStrenght);
        }
        else
        {
            stats.damage = baseDamage;
        }
    }
    public void DecreaseBoosts()
    {
        if (empowerDelay > 0)
        {
            empowerDelay--;
            Debug.Log($"[BOOST ATK] Diminution ! Nouveau délai : {empowerDelay}");
        }
        if (defenseBuffDelay > 0)
        {
            defenseBuffDelay--;
        }

    }
    
    public IEnumerator TriggerDefenseQTE(float windowDuration)
    {
        stats.blocking = false;
        float elapsed = 0f;

        Debug.Log("Def QTE");
        ShowQTE(true);
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
        ShowQTE(false);
        if (stats.blocking)
        {
            DisplayGrade(GradeScript.Grade.Excellent, true);
        }

    }

    public void GameOver()
    {
        gameOverUI.SetActive(true);
        actionMenu.SetActive(false);
        gameOverUI.GetComponent<UI_GameoverScript>().ToggleGameOverUiVisibility(true);
    }

    public void Victory()
    {
        victoryUI.SetActive(true);
        actionMenu.SetActive(false);
        victoryUI.GetComponent<UI_VictoryScript>().ToggleVictoryUiVisibility(true);
    }

    public void ShowQTE(bool mustDisplay)
    {
        if (mustDisplay)
        {
            qteWarning.SetActive(true);
            AudioManager.Instance.PlaySFX("QTE");
        }
        else
        {
            qteWarning.SetActive(false);
        }
    }
    public void DisplayGrade(GradeScript.Grade grade, bool display)
    {
        if (gradeScript != null)
        {
            gradeScript.StopAllCoroutines();
            gradeScript.gameObject.SetActive(true);
            StartCoroutine(gradeScript.GradeDisplay(grade, display));
        }
    }
}