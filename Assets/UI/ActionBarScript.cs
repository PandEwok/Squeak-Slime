using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;
using System.Collections;


public class ActionBarScript : MonoBehaviour
{
    [SerializeField] private UIDocument uiDocument;
    private VisualElement root;
    private List<VisualElement> page1;
    private List<VisualElement> page2;
    [SerializeField] private Combat_Logic combatLogic;
    [SerializeField] private GameObject player;
    private int currentEnemyTargetIndex = 0;
    private Vector3 originalPosition;

    private void Start()
    {
        root = uiDocument.rootVisualElement;
        var Attack = root.Q<Button>("Attack");
        var Items = root.Q<Button>("Items");
        var Skills = root.Q<Button>("Skills");
        page1 = root.Query<VisualElement>(className: "ActionMenuButton1").ToList();
        page2 = root.Query<VisualElement>(className: "ActionMenuButton2").ToList();
        Attack?.RegisterCallback<ClickEvent>(ev => AttackClicked());
        Items?.RegisterCallback<ClickEvent>(ev => ItemsClicked());
        Skills?.RegisterCallback<ClickEvent>(ev => SkillsClicked());

        //Position de depart du slime
        originalPosition = player.transform.position;

        var confirmAttackBtn = root.Q<Button>("AttackFront");
        confirmAttackBtn?.RegisterCallback<ClickEvent>(ev => AttackFrontClicked());

    }

    private void AttackClicked()
    {
        Debug.Log("Attack button clicked!");
        TogglePage1Visibility(false);
        TogglePage2Visibility(true);

    }
    private void ItemsClicked()
    {
        Debug.Log("Items button clicked!");
        TogglePage1Visibility(false);
    }
    private void SkillsClicked()
    {
        Debug.Log("Skills button clicked!");
        TogglePage1Visibility(false);
    }

    private void AttackFrontClicked()
    {
        Debug.Log("Confirm Attack button clicked!");
        // 1. Récupérer l'ennemi en premiere position (temporaire avant les fleches)
        if (combatLogic.enemies.Count > 0)
        {
            GameObject target = combatLogic.enemies[currentEnemyTargetIndex];

            //Debut de l'attaque
            StartCoroutine(AttackSequence(target));
            ToggleUiVisibility(false);
        }
        Debug.Log(combatLogic.enemies.Count);
    }

    private IEnumerator AttackSequence(GameObject target)
    {
        Vector3 enemyPos = target.transform.position;
        Vector3 direction = (enemyPos - originalPosition).normalized;
        float stopDistance = 2.5f;

        Vector3 targetPos = enemyPos - (direction * stopDistance);

        // ALLER
        float elapsed = 0;
        float duration = 0.8f;
        while (elapsed < duration)
        {
            player.transform.position = Vector3.Lerp(originalPosition, targetPos, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        // DEGATS
        var enemyStats = target.GetComponent<Stats_System>();
        if (enemyStats != null)
        {
            Debug.Log($"Inflige des dégâts à {target.name}");
            target.GetComponent<Stats_System>().takeDamage(20);
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

        player.transform.position = originalPosition;
        ToggleUiVisibility(true);
        TogglePage2Visibility(false);
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
}
