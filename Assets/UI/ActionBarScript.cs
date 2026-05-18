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
    private playerScript playerS;
    private int playerAttack;
    private int currentEnemyTargetIndex = 0;
    private Vector3 originalPosition;

    private void Start()
    {
        playerS = player.GetComponent<playerScript>();
        playerAttack = player.GetComponent<Stats_System>().damage;
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
            StartCoroutine(playerS.AttackFrontSequence(target));
            ToggleUiVisibility(false);
        }
        Debug.Log(combatLogic.enemies.Count);
    }


    private void AttackUpClicked()
    {
        Debug.Log("Attack Up button clicked!");
        if (combatLogic.enemies.Count > 0)
        {
            GameObject target = combatLogic.enemies[currentEnemyTargetIndex];
            ToggleUiVisibility(false);
            StartCoroutine(playerS.AttackJumpSequence(target));
        }
    }

    

    public void FinalizeAttack()
    {
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
