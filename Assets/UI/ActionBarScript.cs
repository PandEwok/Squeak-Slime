using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.InputSystem;

public class ActionBarScript : MonoBehaviour
{
    [SerializeField] private UIDocument uiDocument;
    private VisualElement root;
    private List<VisualElement> page1;
    private List<VisualElement> page2;
    [SerializeField] private Combat_Logic combatLogic;
    [SerializeField] private GameObject player;
    [SerializeField] private GameObject projectile;
    private int currentEnemyTargetIndex = 0;
    private Vector3 originalPosition;

    private void Start()
    {
        root = uiDocument.rootVisualElement;
        var Attack = root.Q<Button>("Attack");
        var Items = root.Q<Button>("Items");
        var Skills = root.Q<Button>("Skills");
        var Defend = root.Q<Button>("Defend");
        var CancelP1 = root.Q<Button>("CancelToPage1");
        
        page1 = root.Query<VisualElement>(className: "ActionMenuButton1").ToList();
        page2 = root.Query<VisualElement>(className: "ActionMenuButton2").ToList();
        Attack?.RegisterCallback<ClickEvent>(ev => AttackClicked());
        Items?.RegisterCallback<ClickEvent>(ev => ItemsClicked());
        Skills?.RegisterCallback<ClickEvent>(ev => SkillsClicked());
        Defend?.RegisterCallback<ClickEvent>(ev => DefendClicked());
        CancelP1?.RegisterCallback<ClickEvent>(ev => CancelToPage1());

        //Position de depart du slime
        originalPosition = player.transform.position;

        var AttackFront = root.Q<Button>("AttackFront");
        var AttackUp = root.Q<Button>("AttackUp");
        AttackFront?.RegisterCallback<ClickEvent>(ev => AttackFrontClicked());
        AttackUp?.RegisterCallback<ClickEvent>(ev => AttackUpClicked());

    }

    private void AttackClicked()
    {
        Debug.Log("Attack button clicked!");
        TogglePage1Visibility(false);
        ToggleCancelToPage1Visibility(true);
        TogglePage2Visibility(true);

    }
    private void ItemsClicked()
    {
        Debug.Log("Items button clicked!");
        TogglePage1Visibility(false);
        ToggleCancelToPage1Visibility(true);
    }
    private void SkillsClicked()
    {
        Debug.Log("Skills button clicked!");
        TogglePage1Visibility(false);
        ToggleCancelToPage1Visibility(true);
    }

    private void DefendClicked()
    {
        Debug.Log("Defend button clicked!");
        TogglePage1Visibility(false);
        ToggleCancelToPage1Visibility(true);
    }
    private void CancelToPage1()
    {
        Debug.Log("Cancel Attack button clicked!");
        TogglePage2Visibility(false);
        ToggleCancelToPage1Visibility(false);
        TogglePage1Visibility(true);
    }
    private void AttackFrontClicked()
    {
        Debug.Log("Confirm Attack button clicked!");
        
        if (combatLogic.enemies.Count > 0)
        {
            GameObject target = combatLogic.enemies[currentEnemyTargetIndex];

            //Debut de l'attaque
            StartCoroutine(AttackFrontSequence(target));
            ToggleUiVisibility(false);
        }
        Debug.Log(combatLogic.enemies.Count);
    }

    private IEnumerator AttackFrontSequence(GameObject target)
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
            player.transform.position = Vector3.Lerp(originalPosition, targetPos, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        player.transform.position = targetPos;

        float qteWindow = 0.2f;
        float qteElapsed = 0f;
        bool hasCrit = false;
        int baseDamage = 20;


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
            player.transform.position = Vector3.Lerp(targetPos, originalPosition, elapsed / duration);
            Debug.DrawLine(originalPosition, targetPos, Color.red);

            elapsed += Time.deltaTime;
            yield return null;
        }

        FinalizeAttack();
    }

    private void AttackUpClicked()
    {
        Debug.Log("Attack Up button clicked!");
        if (combatLogic.enemies.Count > 0)
        {
            GameObject target = combatLogic.enemies[currentEnemyTargetIndex];
            ToggleUiVisibility(false);
            StartCoroutine(AttackJumpSequence(target));
        }
    }

    private IEnumerator AttackJumpSequence(GameObject target)
    {
        Vector3 startPos = originalPosition;
        Vector3 enemyPos = target.transform.position;
        Vector3 direction = (enemyPos - startPos).normalized;

        
        float prepDistance = 3.0f; //Fin deplacement avant le saut
        Vector3 prepPos = enemyPos - (direction * prepDistance);
        Vector3 arrivalPos = enemyPos;

        float duration = 0.6f;
        float elapsed = 0;

        //APPROCHE
        while (elapsed < duration)
        {
            player.transform.position = Vector3.Lerp(startPos, prepPos, elapsed / duration);
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

            if(projectileToThrow != null)
            {
                projectileToThrow.transform.position = currentPos;
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        //DEGATS
        if(projectileToThrow != null)
        {
            Destroy(projectileToThrow);
        }
        var enemyStats = target.GetComponent<Stats_System>();
        if (enemyStats != null)
        {
            int baseDamage = 20;
            int finalDamage = hasCrit ? Mathf.RoundToInt(baseDamage * 1.5f) : baseDamage;
            enemyStats.takeDamage(finalDamage);
        }
        yield return new WaitForSeconds(0.3f);

        //RETOUR
        elapsed = 0;
        Vector3 impactPos = player.transform.position;
        while (elapsed < duration)
        {
            player.transform.position = Vector3.Lerp(impactPos, startPos, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        FinalizeAttack();

    }

    private void FinalizeAttack()
    {
        player.transform.position = originalPosition;
        ToggleUiVisibility(true);
        TogglePage2Visibility(false);
        ToggleCancelToPage1Visibility(false);
        TogglePage1Visibility(true);
    }
    private void ToggleUiVisibility(bool mustDisplay)
    {
        if (mustDisplay) 
        {
            root.style.display = DisplayStyle.Flex;
        }
        else
        {
            root.style.display = DisplayStyle.None;
        }
    }
    private void TogglePage1Visibility(bool mustDisplay)
    {
        foreach (var element in page1)
        {
            if (mustDisplay)
            {
                element.style.display = DisplayStyle.Flex;
            }
            else
            {
                element.style.display = DisplayStyle.None;
            }
        }
    }
    private void TogglePage2Visibility(bool mustDisplay)
    {
        foreach(var element in page2)
        {
            if(mustDisplay)
            { 
                element.style.display = DisplayStyle.Flex;
            }
            else
            {
                element.style.display = DisplayStyle.None;
            }
        }
    }

    private void ToggleCancelToPage1Visibility(bool mustDisplay)
    {
        var cancelBtn = root.Q<Button>("CancelToPage1");
        if (cancelBtn != null)
        {
            if (mustDisplay)
            {
                cancelBtn.style.display = DisplayStyle.Flex;
            }
            else
            {
                cancelBtn.style.display = DisplayStyle.None;
            }
        }
    }
}
