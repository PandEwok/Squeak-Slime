using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.InputSystem;

public class ActionBarScript : MonoBehaviour
{
    [Header("Dynamic Inventory Settings")]
    [SerializeField] private VisualTreeAsset itemRowTemplate;
    private ScrollView inventoryContainer;
    [SerializeField] private UIDocument uiDocument;
    private VisualElement root;
    private List<VisualElement> page1;
    private List<VisualElement> page2;
    private List<VisualElement> page3;
    private List<VisualElement> page4;
    [SerializeField] private Combat_Logic combatLogic;
    [SerializeField] private GameObject player;
    private PlayerScript playerS;
    private PlayerInventory playerInventory;
    private int playerAttack;
    private Vector3 originalPosition;
    private Label cheeseQtyLabel;
    private Label bananaQtyLabel;
    private Label pepperAttQtyLabel;
    private Label pepperDefQtyLabel;
    private bool isSelectingEnnemy = false;
    private int targetCount = 0;
    private bool confirmedAttack = false;

    enum AttackType { MELEE, RANGED, BITE, FRACTURE, NONE };
    AttackType attackType = AttackType.NONE;
    [System.Serializable]
    public struct ItemDescription
    {
        public string itemId;
        [TextArea(2, 4)]
        public string descriptionText;
    }
    [SerializeField] private List<ItemDescription> itemDescriptions;
    private Label descriptionDisplayLabel;

    private void Awake()
    {
        playerS = player.GetComponent<PlayerScript>();
        playerAttack = player.GetComponent<Stats_System>().damage;

        playerInventory = player.GetComponent<PlayerInventory>();
    }
    private void OnEnable()
    {
        uiDocument = GetComponent<UIDocument>();
        root = uiDocument.rootVisualElement;
        inventoryContainer = root.Q<ScrollView>("InventoryGrid");
        UQueryBuilder<Button> allButtons = root.Query<Button>();
        allButtons.ForEach(button =>
        {
            button.clicked += () => PlayClickSound();
        });
        descriptionDisplayLabel = root.Q<Label>("Description");
    }


    private void Start()
    {

        var Attack = root.Q<Button>("Attack");
        var Items = root.Q<Button>("Items");
        var Skills = root.Q<Button>("Skills");
        var Defend = root.Q<Button>("Defend");
        var CancelP1 = root.Q<Button>("CancelToPage1");
        var AttackFront = root.Q<Button>("AttackFront");
        var AttackUp = root.Q<Button>("AttackUp");
        var Bite = root.Q<Button>("Bite");
        var Fracture = root.Q<Button>("Fracture");
        var Fireball = root.Q<Button>("Fireball");
        var Absorption = root.Q<Button>("Absorption");
        var Confirm = root.Q<Button>("Confirm");
        page1 = root.Query<VisualElement>(className: "ActionMenuButton1").ToList();
        page2 = root.Query<VisualElement>(className: "ActionMenuButton2").ToList();
        page3 = new List<VisualElement> { inventoryContainer };
        page4 = root.Query<VisualElement>(className: "ActionMenuButton4").ToList();
        Attack?.RegisterCallback<ClickEvent>(ev => AttackClicked());
        Items?.RegisterCallback<ClickEvent>(ev => ItemsClicked());
        Skills?.RegisterCallback<ClickEvent>(ev => SkillsClicked());
        Defend?.RegisterCallback<ClickEvent>(ev => DefendClicked());
        CancelP1?.RegisterCallback<ClickEvent>(ev => CancelToPage1());
        AttackFront?.RegisterCallback<ClickEvent>(ev => AttackFrontClicked());
        AttackUp?.RegisterCallback<ClickEvent>(ev => AttackUpClicked());
        Bite?.RegisterCallback<ClickEvent>(ev => UseBite());
        Bite?.RegisterCallback<PointerEnterEvent>(ev => ShowDescription("Bite"));
        Bite?.RegisterCallback<PointerLeaveEvent>(ev => ShowDescription(""));
        Fracture?.RegisterCallback<ClickEvent>(ev => UseFracture());
        Fracture?.RegisterCallback<PointerEnterEvent>(ev => ShowDescription("Fracture"));
        Fracture?.RegisterCallback<PointerLeaveEvent>(ev => ShowDescription(""));
        Fireball?.RegisterCallback<ClickEvent>(ev => UseFireball());
        Fireball?.RegisterCallback<PointerEnterEvent>(ev => ShowDescription("Fireball"));
        Fireball?.RegisterCallback<PointerLeaveEvent>(ev => ShowDescription(""));
        Absorption?.RegisterCallback<ClickEvent>(ev => UseAbsorption());
        Absorption?.RegisterCallback<PointerEnterEvent>(ev => ShowDescription("Absorption"));
        Absorption?.RegisterCallback<PointerLeaveEvent>(ev => ShowDescription(""));
        Confirm?.RegisterCallback<ClickEvent>(ev => ConfirmPressed());
    }

    private void Update()
    {
        foreach (var enemy in combatLogic.enemies)
        {
            enemy.GetComponent<Enemy_AI>().deselect();
        }
        if (isSelectingEnnemy)
        {
            combatLogic.enemies[targetCount].GetComponent<Enemy_AI>().select();
            if (Keyboard.current != null && Keyboard.current.tabKey.wasPressedThisFrame)
            {
                if (targetCount >= combatLogic.enemies.Count - 1)
                {
                    targetCount = 0;
                }
                else
                {
                    targetCount++;
                }
            }
            if (confirmedAttack)
            {
                if (combatLogic.enemies.Count > 0)
                {
                    GameObject target = combatLogic.enemies[targetCount];



                    if (attackType == AttackType.MELEE)
                    {
                        StartCoroutine(playerS.AttackFrontSequence(target, 0, false));
                        isSelectingEnnemy = false;
                        attackType = AttackType.NONE;
                        ToggleUiVisibility(false);
                        
                    }
                    else if (attackType == AttackType.RANGED)
                    {
                        StartCoroutine(playerS.AttackJumpSequence(target, 0));
                        isSelectingEnnemy = false;
                        attackType = AttackType.NONE;
                        ToggleUiVisibility(false);
                    }
                    else if (attackType == AttackType.BITE)
                    {
                        isSelectingEnnemy = false;
                        attackType = AttackType.NONE;
                        ToggleUiVisibility(false);
                        playerS.SP -= 5;
                        if (playerS.SP < 0)
                        {
                            playerS.SP = 0;
                        }
                        StartCoroutine(playerS.AttackBiteSequence(target));

                    }
                    else if (attackType == AttackType.FRACTURE)
                    {
                        isSelectingEnnemy = false;
                        attackType = AttackType.NONE;
                        ToggleUiVisibility(false);
                        playerS.SP -= 7;
                        if (playerS.SP < 0)
                        {
                            playerS.SP = 0;
                        }
                        StartCoroutine(playerS.AttackFractureSequence(target));
                    }
                    targetCount = 0;
                    confirmedAttack = false;

                }
            }

        }
    }
    public void UpdateInventoryUI()
    {
        if (inventoryContainer == null || itemRowTemplate == null) return;

        inventoryContainer.Clear();

        foreach (var pair in playerInventory.itemsPossessed)
        {
            ItemData item = pair.Key;
            int count = pair.Value;

            VisualElement itemRow = itemRowTemplate.Instantiate();

            itemRow.Q<Label>("ItemName").text = item.itemName;
            itemRow.Q<Label>("ItemCount").text = "X" + count.ToString();

            VisualElement icon = itemRow.Q<VisualElement>("ItemIcon");
            if (icon != null && item.itemIcon != null)
            {
                icon.style.backgroundImage = new StyleBackground(item.itemIcon);
            }

            Button itemButton = itemRow.Q<Button>("ItemButton");
            if (itemButton != null)
            {
                itemButton.RegisterCallback<ClickEvent>(ev =>
                {
                    item.UseItem(player);
                    UpdateInventoryUI();

                    
                    ToggleUiVisibility(false);
                    playerS.SwitchingTurn();
                });

                itemButton.RegisterCallback<PointerEnterEvent>(ev => ShowDescription(item.itemDescription));
                itemButton.RegisterCallback<PointerLeaveEvent>(ev => ShowDescription(""));
            }

            inventoryContainer.Add(itemRow);
        }
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
        UpdateInventoryUI();
        TogglePage1Visibility(false);
        TogglePage3Visibility(true);
        ToggleCancelToPage1Visibility(true);
    }
    private void SkillsClicked()
    {
        Debug.Log("Skills button clicked!");
        TogglePage1Visibility(false);
        TogglePage4Visibility(true);
        ToggleCancelToPage1Visibility(true);
    }

    private void DefendClicked()
    {
        Debug.Log("Defend button clicked!");
        TogglePage1Visibility(false);
        ToggleUiVisibility(false);
        player.GetComponent<Stats_System>().defending = true;
        playerS.SwitchingTurn();

    }
    private void CancelToPage1()
    {
        Debug.Log("Cancel Attack button clicked!");
        TogglePage2Visibility(false);
        TogglePage3Visibility(false);
        TogglePage4Visibility(false);
        ToggleCancelToPage1Visibility(false);
        TogglePage1Visibility(true);
        isSelectingEnnemy = false;
        attackType = AttackType.NONE;
        targetCount = 0;
    }
    private void AttackFrontClicked()
    {
        isSelectingEnnemy = true;
        Debug.Log("Confirm Attack button clicked!");

        attackType = AttackType.MELEE;
        ToggleConfirmVisibility(true);
    }


    private void AttackUpClicked()
    {
        isSelectingEnnemy = true;
        Debug.Log("Attack Up button clicked!");

        attackType = AttackType.RANGED;
        ToggleConfirmVisibility(true);
    }

    private void UseBite()
    {
        if (playerS.SP < 5)
        {
            Debug.Log("Not enough SP to use Bite!");
            return;
        }
        else
        {
            isSelectingEnnemy = true;
            Debug.Log("Use Bite button clicked!");

            attackType = AttackType.BITE;
            ToggleConfirmVisibility(true);

        }
    }

    private void UseFracture()
    {
        if (playerS.SP < 7)
        {
            Debug.Log("Not enough SP to use Fracture!");
            return;
        }
        else
        {
            isSelectingEnnemy = true;
            Debug.Log("Use Fracture button clicked!");
            attackType = AttackType.FRACTURE;
            ToggleConfirmVisibility(true);
        }
    }

    private void UseFireball()
    {
        if (playerS.SP < 12)
        {
            Debug.Log("Not enough SP to use Fireball!");
            return;
        }
        else
        {
            playerS.SP -= 12;
            if (playerS.SP < 0)
            {
                playerS.SP = 0;
            }
            Debug.Log("Use Fireball button clicked!");
            StartCoroutine(playerS.AttackFireSequence(combatLogic.enemies));
            ToggleUiVisibility(false);
        }
    }

    private void UseAbsorption()
    {
        Debug.Log("Use Absorption button clicked!");
        if (playerS.SP < 10)
        {
            Debug.Log("Not enough SP to use Absorption!");
            return;
        }
        else
        {
            playerS.SP -= 10;
            if (playerS.SP < 0)
            {
                playerS.SP = 0;
            }
            player.GetComponent<Stats_System>().ActivateAbsorption();
            playerS.SwitchingTurn();
            ToggleUiVisibility(false);
        }
    }



    public void FinalizeAttack()
    {
        ToggleUiVisibility(true);
        TogglePage2Visibility(false);
        TogglePage3Visibility(false);
        TogglePage4Visibility(false);
        ToggleCancelToPage1Visibility(false);
        TogglePage1Visibility(true);
    }
    private void ToggleUiVisibility(bool mustDisplay)
    {
        if (mustDisplay && player.GetComponent<Stats_System>().health > 0 && combatLogic.enemies.Count > 0)
        {
            root.style.display = DisplayStyle.Flex;
            playerS.DecreaseBoosts();
            playerS.ApplyAttackBoost();
            Stats_System playerStats = player.GetComponent<Stats_System>();
            playerStats.defending = false;
            StartCoroutine(player.GetComponent<Stats_System>().ApplyStatus());

        }
        else
        {
            TogglePage1Visibility(true);
            TogglePage2Visibility(false);
            TogglePage3Visibility(false);
            TogglePage4Visibility(false);
            ToggleCancelToPage1Visibility(false);
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
                ToggleConfirmVisibility(false);
            }
            else
            {
                element.style.display = DisplayStyle.None;
            }
        }
    }
    private void TogglePage2Visibility(bool mustDisplay)
    {
        foreach (var element in page2)
        {
            if (mustDisplay)
            {
                element.style.display = DisplayStyle.Flex;
                ToggleConfirmVisibility(false);
            }
            else
            {
                element.style.display = DisplayStyle.None;
            }
        }
    }

    private void TogglePage3Visibility(bool mustDisplay)
    {
        foreach (var element in page3)
        {
            if (mustDisplay)
            {
                element.style.display = DisplayStyle.Flex;
                ToggleConfirmVisibility(false);
            }
            else
            {
                element.style.display = DisplayStyle.None;
            }
        }
    }
    private void TogglePage4Visibility(bool mustDisplay)
    {
        foreach (var element in page4)
        {
            if (mustDisplay)
            {
                element.style.display = DisplayStyle.Flex;
                ToggleConfirmVisibility(false);
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

    private void ToggleConfirmVisibility(bool mustDisplay)
    {
        var confirmBtn = root.Q<Button>("Confirm");
        if (confirmBtn != null)
        {
            if (mustDisplay)
            {
                confirmBtn.style.display = DisplayStyle.Flex;
                TogglePage1Visibility(false);
                TogglePage2Visibility(false);
                TogglePage3Visibility(false);
                TogglePage4Visibility(false);
            }
            else
            {
                confirmBtn.style.display = DisplayStyle.None;
            }
        }
    }
    //private void UseCheese()
    //{
    //    Debug.Log("Use Cheese button clicked!");
    //    if (playerInventory.cheeseInv > 0)
    //    {
    //        playerInventory.removeCheese(1);
    //        playerS.HealPlayer(50);
    //        UpdateInventoryUI();
    //        //FinalizeAttack();
    //        ToggleUiVisibility(false);
    //        playerS.SwitchingTurn();
    //    }
    //}

    //private void UseBanana()
    //{
    //    Debug.Log("Use Banana button clicked!");
    //    if (playerInventory.bananaInv > 0)
    //    {
    //        playerInventory.removeBanana(1);
    //        playerS.RestoreSP(50);
    //        UpdateInventoryUI();
    //        //FinalizeAttack();
    //        ToggleUiVisibility(false);
    //        playerS.SwitchingTurn();
    //    }
    //}

    //private void UsePepperAtt()
    //{
    //    Debug.Log("Use Pepper Attack button clicked!");
    //    if (playerInventory.pepperAttInv > 0)
    //    {
    //        playerInventory.removePepperAtt(1);
    //        playerS.ActionEmpower();
    //        UpdateInventoryUI();
    //        //FinalizeAttack();
    //        ToggleUiVisibility(false);
    //        playerS.SwitchingTurn();
    //    }
    //}

    //private void UsePepperDef()
    //{
    //    Debug.Log("Use Pepper Defense button clicked!");
    //    if (playerInventory.pepperDefInv > 0)
    //    {
    //        playerInventory.removePepperDef(1);
    //        playerS.ActionDefenseBuff();
    //        UpdateInventoryUI();
    //        //FinalizeAttack();
    //        ToggleUiVisibility(false);
    //        playerS.SwitchingTurn();
    //    }
    //}

    public void ShowDescription(string id)
    {
        if (descriptionDisplayLabel == null) return;

        if (string.IsNullOrEmpty(id))
        {
            descriptionDisplayLabel.style.visibility = Visibility.Hidden;
            return;
        }

        foreach (var item in itemDescriptions)
        {
            if (item.itemId == id)
            {
                descriptionDisplayLabel.text = item.descriptionText;
                descriptionDisplayLabel.style.visibility = Visibility.Visible;
                return;
            }
        }
    }

    private void ConfirmPressed()
    {
        Debug.Log("Confirm button clicked!");
        confirmedAttack = true;
        ToggleConfirmVisibility(false);
    }

    private void PlayClickSound()
    {
        AudioManager.Instance.PlaySFX("Button_Pressed");
    }
}
