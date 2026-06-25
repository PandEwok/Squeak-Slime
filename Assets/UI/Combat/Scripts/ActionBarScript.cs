using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.InputSystem;

public class ActionBarScript : MonoBehaviour
{
    [SerializeField] private StyleSheet itemRowStyleSheet;
    [Header("Dynamic Inventory Settings")]
    [SerializeField] private VisualTreeAsset itemRowTemplate;
    private ScrollView inventoryContainer;
    [SerializeField] private UIDocument uiDocument;
    [SerializeField] private GameObject tabIndicator;
    private VisualElement root;
    private List<VisualElement> page1;
    private List<VisualElement> page2;
    private List<VisualElement> page3;
    private List<VisualElement> page4;
    private Button Skills;
    private Button Bite;
    private Button Fracture;
    private Button Fireball;
    private Button Absorption;
    [SerializeField] public Combat_Logic combatLogic;
    [SerializeField] private GameObject playerGameObject;
    [HideInInspector] public Player playerScript;
    private PlayerInventory playerInventory;
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
        playerScript = playerGameObject.GetComponent<Player>();

        playerInventory = playerGameObject.GetComponent<PlayerInventory>();
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
        if (inventoryContainer is ScrollView scrollView)
        {
            scrollView.verticalScrollerVisibility = ScrollerVisibility.Hidden;
            scrollView.horizontalScrollerVisibility = ScrollerVisibility.Hidden;
        }
        //Supprimer si fail
        var Attack = root.Q<Button>("Attack");
        var Items = root.Q<Button>("Items");
        var Defend = root.Q<Button>("Defend");
        var CancelP1 = root.Q<Button>("CancelToPage1");
        var AttackFront = root.Q<Button>("AttackFront");
        var AttackUp = root.Q<Button>("AttackUp");
        Skills = root.Q<Button>("Skills");
        Bite = root.Q<Button>("Bite");
        Fracture = root.Q<Button>("Fracture");
        Fireball = root.Q<Button>("Fireball");
        Absorption = root.Q<Button>("Absorption");
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
        if (playerScript.inventory.DoesHaveAnySkill())
        {
            Skills.style.display = DisplayStyle.Flex;
        }
        else
        {
            Skills.style.display = DisplayStyle.None;
        }
    }


    private void Start()
    {

        //Ancien emplacement boutons
    }

    private void Update()
    {
        if (combatLogic != null)
        {
            foreach (var enemy in combatLogic.enemies)
            {
                if (isSelectingEnnemy)
                {
                    if (enemy != combatLogic.enemies[targetCount])
                    {
                        enemy.GetComponent<Enemy_AI>().deselect();
                    }
                }
                else
                {
                    enemy.GetComponent<Enemy_AI>().deselect();
                }
            }
            if (isSelectingEnnemy)
            {
                if(tabIndicator.activeSelf == false)
                {
                    tabIndicator.SetActive(true);
                }
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
                            playerScript.inventory.meleeAttack.Execute(playerScript, target);
                            isSelectingEnnemy = false;
                            attackType = AttackType.NONE;
                            ToggleUiVisibility(false);

                        }
                        else if (attackType == AttackType.RANGED)
                        {
                            playerScript.inventory.rangedAttack.Execute(playerScript, target);
                            isSelectingEnnemy = false;
                            attackType = AttackType.NONE;
                            ToggleUiVisibility(false);
                        }
                        else if (attackType == AttackType.BITE)
                        {
                            isSelectingEnnemy = false;
                            attackType = AttackType.NONE;
                            ToggleUiVisibility(false);
                            playerScript.stats.SP -= playerScript.inventory.biteAttack.actionCost;
                            if (playerScript.stats.SP < 0)
                            {
                                playerScript.stats.SP = 0;
                            }
                            playerScript.inventory.biteAttack.Execute(playerScript, target);

                        }
                        else if (attackType == AttackType.FRACTURE)
                        {
                            isSelectingEnnemy = false;
                            attackType = AttackType.NONE;
                            ToggleUiVisibility(false);
                            playerScript.stats.SP -= playerScript.inventory.fractureAttack.actionCost;
                            if (playerScript.stats.SP < 0)
                            {
                                playerScript.stats.SP = 0;
                            }
                            playerScript.inventory.fractureAttack.Execute(playerScript, target);
                        }
                        targetCount = 0;
                        confirmedAttack = false;
                        

                    }
                }

            }
            else
            {
                if(tabIndicator.activeSelf == true)
                {
                    tabIndicator.SetActive(false);
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
            if (itemRowStyleSheet != null)
            {
                itemRow.styleSheets.Add(itemRowStyleSheet);
            }
            itemRow.Q<Label>("ItemName").text = item.itemName;
            itemRow.Q<Label>("ItemCount").text = "X" + count.ToString();

            VisualElement icon = itemRow.Q<VisualElement>("ItemIcon");
            if (icon != null && item.itemIcon != null)
            {
                icon.style.backgroundImage = new StyleBackground(item.itemIcon.texture);
                icon.style.unityBackgroundImageTintColor = item.defaultColor;
            }

            Button itemButton = itemRow.Q<Button>("ItemButton");
            if (itemButton != null)
            {
                itemButton.RegisterCallback<ClickEvent>(ev =>
                {
                    item.UseItem(playerGameObject);
                    UpdateInventoryUI();

                    
                    ToggleUiVisibility(false);
                    playerScript.SwitchingTurn();
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
        TogglePage2Visibility(true);
        ToggleCancelToPage1Visibility(true);
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
        playerGameObject.GetComponent<PlayerStats>().defending = true;
        playerScript.SwitchingTurn();

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
        if (playerScript.stats.SP < playerScript.inventory.biteAttack.actionCost)
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
        if (playerScript.stats.SP < playerScript.inventory.fractureAttack.actionCost)
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
        if (playerScript.stats.SP < playerScript.inventory.fireballAttack.actionCost)
        {
            Debug.Log("Not enough SP to use Fireball!");
            return;
        }
        else
        {
            playerScript.stats.SP -= playerScript.inventory.fireballAttack.actionCost;
            if (playerScript.stats.SP < 0)
            {
                playerScript.stats.SP = 0;
            }
            Debug.Log("Use Fireball button clicked!");
            playerScript.inventory.fireballAttack.Execute(playerScript, combatLogic.enemies);
            ToggleUiVisibility(false);
        }
    }

    private void UseAbsorption()
    {
        Debug.Log("Use Absorption button clicked!");
        if (playerScript.stats.SP < playerScript.inventory.absorptionAction.actionCost)
        {
            Debug.Log("Not enough SP to use Absorption!");
            return;
        }
        else
        {
            playerScript.stats.SP -= playerScript.inventory.absorptionAction.actionCost;
            if (playerScript.stats.SP < 0)
            {
                playerScript.stats.SP = 0;
            }
            playerScript.inventory.absorptionAction.Execute(playerScript);
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
        if (mustDisplay && playerGameObject.GetComponent<Stats_System>().health > 0 && combatLogic.enemies.Count > 0)
        {
            root.style.display = DisplayStyle.Flex;
            playerScript.stats.DecreaseBoosts();
            playerScript.stats.ApplyAttackBoost();
            playerScript.stats.defending = false;
            StartCoroutine(playerGameObject.GetComponent<Stats_System>().ApplyStatus());
            descriptionDisplayLabel.style.visibility = Visibility.Hidden;

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
                descriptionDisplayLabel.style.visibility = Visibility.Hidden;
                if (!playerScript.inventory.DoesHaveAnySkill())
                {
                    Skills.style.display = DisplayStyle.None;
                }
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
                if (!playerScript.inventory.hasBite)
                {
                    Bite.style.display = DisplayStyle.None;
                }
                if (!playerScript.inventory.hasFracture)
                {
                    Fracture.style.display = DisplayStyle.None;
                }
                if (!playerScript.inventory.hasFireball)
                {
                    Fireball.style.display = DisplayStyle.None;
                }
                if (!playerScript.inventory.hasAbsorption)
                {
                    Absorption.style.display = DisplayStyle.None;
                }
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
        descriptionDisplayLabel.text = id;
        descriptionDisplayLabel.style.visibility = Visibility.Visible;
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
