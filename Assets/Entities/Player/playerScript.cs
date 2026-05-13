using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class playerScript : MonoBehaviour
{
    public Combat_Logic combatLogic;
    public List<BaseItem> inventory;
    Vector3 originalPosition;
    private Stats_System stats;
    private ActionBarScript actionUI;
    [SerializeField] private GameObject projectile;
    private void Start()
    {
        actionUI = transform.Find("ActionMenu").GetComponent<ActionBarScript>();
        stats = GetComponent<Stats_System>();
        originalPosition = transform.position;
    }
    public IEnumerator AttackFrontSequence(GameObject target)
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
        int baseDamage = stats.damage;


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

        // DEGATS
        var enemyStats = target.GetComponent<Stats_System>();
        if (enemyStats != null)
        {
            Debug.Log($"Inflige des dégâts à {target.name}");
            int finalDamage = hasCrit ? Mathf.RoundToInt(baseDamage * 1.5f) : baseDamage;

            target.GetComponent<Stats_System>().takeDamage(finalDamage);
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

        actionUI.FinalizeAttack();
        transform.position = originalPosition;
    }

    public IEnumerator AttackJumpSequence(GameObject target)
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
                }

                if (Pointer.current.press.wasPressedThisFrame)
                {
                    hasCrit = true;
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

        //DEGATS
        if (projectileToThrow != null)
        {
            Destroy(projectileToThrow);
        }
        var enemyStats = target.GetComponent<Stats_System>();
        if (enemyStats != null)
        {
            int baseDamage = stats.damage;
            int finalDamage = hasCrit ? Mathf.RoundToInt(baseDamage * 1.5f) : baseDamage;
            enemyStats.takeDamage(finalDamage);
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
        actionUI.FinalizeAttack();
        transform.position = originalPosition;

    }
}
