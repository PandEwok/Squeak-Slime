using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerScript : MonoBehaviour
{
    public Combat_Logic combatLogic;
    Vector3 originalPosition;
    private Stats_System stats;
    [SerializeField] private GameObject projectile;
    public int originalSP = 100;
    public int SP = 100;
    public GameObject attackBoostEffect;
    public GameObject defenseBoostEffect;
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
    public GameObject gameOverUI;
    public GameObject victoryUI;
    public GameObject actionMenu;
    public GameObject qteWarning;
    private GradeScript gradeScript;

    private void Awake()
    {
        Transform gradeTransform = transform.Find("GradeDisplay");
        if (gradeTransform != null)
        {
            gradeScript = gradeTransform.GetComponent<GradeScript>();
        }
        else
        {
            Debug.LogError("GradeDisplay object not found as a child of the player.");
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
    public IEnumerator AttackFrontSequence(GameObject target, float boost)
    {
        Vector3 enemyPos = target.transform.position;
        Vector3 direction = (enemyPos - originalPosition).normalized;
        float stopDistance = 2.5f;

        Vector3 targetPos = enemyPos - (direction * stopDistance);

        // ALLER
        float elapsed = 0;
        float duration = 0.6f;
        while (elapsed < duration)
        {
            transform.position = Vector3.Lerp(originalPosition, targetPos, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.position = targetPos;

        float qteWindow = 0.2f;
        float qteElapsed = 0f;
        bool hasCrit = false;
        int baseDamage = (int)(stats.damage + (stats.damage * boost));

        showQTE(true);
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
        showQTE(false);
        // DEGATS
        if (hasCrit)
        {
            StartCoroutine(gradeScript.gradeDisplay(GradeScript.Grade.Excellent, true));
        }
        var enemyStats = target.GetComponent<Stats_System>();
        if (enemyStats != null)
        {
            Debug.Log($"Inflige des dégâts à {target.name}");
            int finalDamage = hasCrit ? Mathf.RoundToInt(baseDamage * 1.5f) : baseDamage;


            target.GetComponent<Stats_System>().takeDamage(finalDamage, false);
            yield return new WaitForSeconds(0.5f);
        }

        // RETOUR
        elapsed = 0;
        while (elapsed < duration)
        {
            transform.position = Vector3.Lerp(targetPos, originalPosition, elapsed / duration);
            Debug.DrawLine(originalPosition, targetPos, Color.red);

            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.position = originalPosition;
        switchingTurn();
    }

    public IEnumerator AttackJumpSequence(GameObject target, float boost)
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
        while (elapsed < duration)
        {
            transform.position = Vector3.Lerp(startPos, prepPos, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        //LANCER
        GameObject projectileToThrow = Instantiate(projectile, prepPos, Quaternion.identity);
        elapsed = 0;
        float jumpHeight = 4.0f;
        float jumpDuration = 0.5f;
        bool hasCrit = false;
        bool qteWindowOpen = false;

        while (elapsed < jumpDuration)
        {
            float t = elapsed / jumpDuration;

            if (t >= 0.6f && !hasCrit)
            {
                if (!qteWindowOpen)
                {
                    qteWindowOpen = true;
                    showQTE(true);
                }

                if (Pointer.current.press.wasPressedThisFrame)
                {
                    hasCrit = true;
                    showQTE(false);
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
        showQTE(false);
        //DEGATS
        if (projectileToThrow != null)
        {
            Destroy(projectileToThrow);
        }
        var enemyStats = target.GetComponent<Stats_System>();
        if (enemyStats != null)
        {
            int baseDamage = (int)(stats.damage + (stats.damage * boost));
            int finalDamage = hasCrit ? Mathf.RoundToInt(baseDamage * 1.5f) : baseDamage;
            enemyStats.takeDamage(finalDamage, false);
        }
        if(hasCrit)
        {
            StartCoroutine(gradeScript.gradeDisplay(GradeScript.Grade.Excellent, true));
        }
        yield return new WaitForSeconds(0.3f);

        //RETOUR
        elapsed = 0;
        Vector3 impactPos = transform.position;
        while (elapsed < duration)
        {
            transform.position = Vector3.Lerp(impactPos, startPos, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        transform.position = originalPosition;
        switchingTurn();

    }

    public IEnumerator AttackBiteSequence(GameObject target)
    {

        yield return AttackFrontSequence(target, 0.5f);
        if (target != null)
        {
            target.GetComponent<Stats_System>().makeBleeding();
        }
    }

    public void healPlayer(int healAmount)
    {
        stats.heal(healAmount);
    }

    public void restoreSP(int spAmount)
    {
        SP += spAmount;
        SP = Mathf.Min(SP, originalSP);
        Debug.Log($"{gameObject.name} healed for {spAmount}. Current health: {SP}");
    }

    public void actionEmpower()
    {
        empowerDelay = 4;
    }
    public void actionDefenseBuff()
    {
        defenseBuffDelay = 3;
    }
    public void switchingTurn()
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
    public void applyAttackBoost()
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
    public void decreaseBoosts()
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
    public void applyStatus()
    {
        stats.bleed();
    }
    public IEnumerator TriggerDefenseQTE(float windowDuration)
    {
        stats.blocking = false;
        float elapsed = 0f;

        Debug.Log("Def QTE");
        showQTE(true);
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
        showQTE(false);
        if(stats.blocking)
            StartCoroutine(gradeScript.gradeDisplay(GradeScript.Grade.Excellent, true));

    }

    public void gameOver()
    {
        gameOverUI.SetActive(true);
        actionMenu.SetActive(false);
        gameOverUI.GetComponent<UI_GameoverScript>().ToggleGameOverUiVisibility(true);
    }

    public void victory()
    {
        victoryUI.SetActive(true);
        actionMenu.SetActive(false);
        victoryUI.GetComponent<UI_VictoryScript>().ToggleVictoryUiVisibility(true);
    }

    public void showQTE(bool mustDisplay)
    {
        if (mustDisplay)
        {
            qteWarning.SetActive(true);
            //Faudra mettre un son
        }
        else
        {
            qteWarning.SetActive(false);
        }
    }
}