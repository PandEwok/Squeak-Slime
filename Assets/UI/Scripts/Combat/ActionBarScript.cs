using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.InputSystem;

public class ActionBarScript : MonoBehaviour
{

    // ==========================================
    // MODULAR SYSTEM MEMORY
    // ==========================================
    private ItemData pendingModularAction = null;


    [Header("Collaboration & Testing Toggle")]
    [Tooltip("Check this to test the new modular system. Leave unchecked to use the old hardcoded logic.")]
    public bool useModularItemSystem = false;

    [Header("Modular Action Assets (Items)")]
    public ItemData cheeseAsset;
    public ItemData bananaAsset;
    public ItemData pepperAttAsset;
    public ItemData pepperDefAsset;

    [Header("Modular Action Assets (Skills)")]
    public ItemData biteSkillAsset;
    public ItemData fireballSkillAsset;
    public ItemData fragmentationSkillAsset;
    public ItemData absorptionSkillAsset;

    // Memory variable to remember which skill you clicked before picking an enemy
    private ItemData pendingModularSkill = null;


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
    enum AttackType { MELEE, RANGED, BITE, MODULAR, NONE };
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

        cheeseQtyLabel = root.Q<Label>("CheeseQuantity");
        bananaQtyLabel = root.Q<Label>("BananaQuantity");
        pepperAttQtyLabel = root.Q<Label>("PepperAttQuantity");
        pepperDefQtyLabel = root.Q<Label>("PepperDefQuantity");

        UpdateInventoryUI();

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
        var Cheese = root.Q<Button>("Cheese");
        var Banana = root.Q<Button>("Banana");
        var PepperAtt = root.Q<Button>("PepperAtt");
        var PepperDef = root.Q<Button>("PepperDef");
        var Bite = root.Q<Button>("Bite");
        var Fracture = root.Q<Button>("Fracture");
        var Fireball = root.Q<Button>("Fireball");
        var Absorption = root.Q<Button>("Absorption");
        page1 = root.Query<VisualElement>(className: "ActionMenuButton1").ToList();
        page2 = root.Query<VisualElement>(className: "ActionMenuButton2").ToList();
        page3 = root.Query<VisualElement>(className: "ActionMenuButton3").ToList();
        page4 = root.Query<VisualElement>(className: "ActionMenuButton4").ToList();
        Attack?.RegisterCallback<ClickEvent>(ev => AttackClicked());
        Items?.RegisterCallback<ClickEvent>(ev => ItemsClicked());
        Skills?.RegisterCallback<ClickEvent>(ev => SkillsClicked());
        Defend?.RegisterCallback<ClickEvent>(ev => DefendClicked());
        CancelP1?.RegisterCallback<ClickEvent>(ev => CancelToPage1());
        AttackFront?.RegisterCallback<ClickEvent>(ev => AttackFrontClicked());
        AttackUp?.RegisterCallback<ClickEvent>(ev => AttackUpClicked());
        Cheese?.RegisterCallback<ClickEvent>(ev => UseCheese());
        Cheese?.RegisterCallback<PointerEnterEvent>(ev => ShowDescription("Cheese"));
        Cheese?.RegisterCallback<PointerLeaveEvent>(ev => ShowDescription(""));
        Banana?.RegisterCallback<ClickEvent>(ev => UseBanana());
        Banana?.RegisterCallback<PointerEnterEvent>(ev => ShowDescription("Banana"));
        Banana?.RegisterCallback<PointerLeaveEvent>(ev => ShowDescription(""));
        PepperAtt?.RegisterCallback<ClickEvent>(ev => UsePepperAtt());
        PepperAtt?.RegisterCallback<PointerEnterEvent>(ev => ShowDescription("PepperAtt"));
        PepperAtt?.RegisterCallback<PointerLeaveEvent>(ev => ShowDescription(""));
        PepperDef?.RegisterCallback<ClickEvent>(ev => UsePepperDef());
        PepperDef?.RegisterCallback<PointerEnterEvent>(ev => ShowDescription("PepperDef"));
        PepperDef?.RegisterCallback<PointerLeaveEvent>(ev => ShowDescription(""));
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

            // This is the line that got accidentally deleted!
            if (Pointer.current != null && Pointer.current.press.wasPressedThisFrame)
            {
                if (combatLogic.enemies.Count > 0)
                {
                    GameObject target = combatLogic.enemies[targetCount];

                    // ==========================================
                    // FIRE MODULAR SKILL SCRIPT
                    // ==========================================
                    if (attackType == AttackType.MODULAR && pendingModularAction != null)
                    {
                        isSelectingEnnemy = false;
                        attackType = AttackType.NONE;
                        ToggleUiVisibility(false);

                        // Deduct SP
                        // Deduct SP
                        playerS.SP -= pendingModularAction.spCost;
                        if (playerS.SP < 0) playerS.SP = 0;

                        // Trigger the custom script plugged into your ItemData!
                        if (pendingModularAction.specialEffectLogic != null)
                        {
                            pendingModularAction.specialEffectLogic.ExecuteEffect(player.gameObject, target);
                        }

                        pendingModularAction = null; // Clear memory
                    }
                    // ==========================================
                    // LEGACY FIRE SCRIPT
                    // ==========================================
                    else if (attackType == AttackType.MELEE)
                    {
                        StartCoroutine(playerS.AttackFrontSequence(target, 0));
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
                }
            }
        }
    }
    public void UpdateInventoryUI()
    {
        int currentCheese = playerInventory.cheeseInv;
        int currentBanana = playerInventory.bananaInv;
        int currentPepperAtt = playerInventory.pepperAttInv;
        int currentPepperDef = playerInventory.pepperDefInv;

        if (cheeseQtyLabel != null)
        {
            cheeseQtyLabel.text = "X" + currentCheese.ToString();
        }

        if (bananaQtyLabel != null)
        {
            bananaQtyLabel.text = "X" + currentBanana.ToString();
        }

        if (pepperAttQtyLabel != null)
        {
            pepperAttQtyLabel.text = "X" + currentPepperAtt.ToString();
        }

        if (pepperDefQtyLabel != null)
        {
            pepperDefQtyLabel.text = "X" + currentPepperDef.ToString();
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
        playerS.switchingTurn();

    }
    private void CancelToPage1()
    {
        Debug.Log("Cancel Attack button clicked!");
        TogglePage2Visibility(false);
        TogglePage3Visibility(false);
        TogglePage4Visibility(false);
        ToggleCancelToPage1Visibility(false);
        TogglePage1Visibility(true);
    }
    private void AttackFrontClicked()
    {
        isSelectingEnnemy = true;
        Debug.Log("Confirm Attack button clicked!");

        attackType = AttackType.MELEE;
    }


    private void AttackUpClicked()
    {
        isSelectingEnnemy = true;
        Debug.Log("Attack Up button clicked!");

        attackType = AttackType.RANGED;
    }

    private void UseBite()
    {
        // ==========================================
        // NEW MODULAR BITE
        // ==========================================
        if (useModularItemSystem && biteSkillAsset != null)
        {
            if (playerS.SP < biteSkillAsset.spCost) // (You could even change this to biteSkillAsset.spCost in the future!)
            {
                Debug.Log("Not enough SP to use Bite!");
                return;
            }

            isSelectingEnnemy = true;
            pendingModularAction = biteSkillAsset; // Save the asset to memory!
            attackType = AttackType.MODULAR; // Tell the Update loop to use the modular logic
            Debug.Log($"Use Modular Skill clicked: {biteSkillAsset.itemName}");
        }
        // ==========================================
        // LEGACY CODE
        // ==========================================
        else
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
            }
        }
    }

    private void UseFracture()
    {
        Debug.Log("Use Fracture button clicked!");
    }

    private void UseFireball()
    {
        Debug.Log("Use Fireball button clicked!");
    }

    private void UseAbsorption()
    {
        Debug.Log("Use Absorption button clicked!");
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
            }
            else
            {
                element.style.display = DisplayStyle.None;
            }
        }
        if (mustDisplay)
        {
            playerS.decreaseBoosts();
            playerS.applyStatus();


        }
    }
    private void TogglePage2Visibility(bool mustDisplay)
    {
        foreach (var element in page2)
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

    private void TogglePage3Visibility(bool mustDisplay)
    {
        foreach (var element in page3)
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
    private void TogglePage4Visibility(bool mustDisplay)
    {
        foreach (var element in page4)
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


    private void UseCheese()
    {
        Debug.Log("Use Cheese button clicked!");
        if (useModularItemSystem && cheeseAsset != null)
        {
            if (playerInventory.GetItemQuantity(cheeseAsset.itemId) > 0)
            {
                playerInventory.RemoveItem(cheeseAsset.itemId, 1);
                playerS.healPlayer(cheeseAsset.effectValue); // Reads the dynamic stat from your asset!
                UpdateInventoryUI();
                FinalizeAttack();
                ToggleUiVisibility(false);
                playerS.switchingTurn();
            }
        }
        else // 
        {
            if (playerInventory.cheeseInv > 0)
            {
                playerInventory.removeCheese(1);
                playerS.healPlayer(50);
                UpdateInventoryUI();
                FinalizeAttack();
                ToggleUiVisibility(false);
                playerS.switchingTurn();
            }
        }
    }

    private void UseBanana()
    {
        Debug.Log("Use Banana button clicked!");
        if (useModularItemSystem && bananaAsset != null)
        {
            if (playerInventory.GetItemQuantity(bananaAsset.itemId) > 0)
            {
                playerInventory.RemoveItem(bananaAsset.itemId, 1);
                playerS.restoreSP(bananaAsset.effectValue); // Reads the dynamic stat from your asset!
                UpdateInventoryUI();
                FinalizeAttack();
                ToggleUiVisibility(false);
                playerS.switchingTurn();
            }
        }
        else // LEGACY CODE
        {
            if (playerInventory.bananaInv > 0)
            {
                playerInventory.removeBanana(1);
                playerS.restoreSP(50);
                UpdateInventoryUI();
                FinalizeAttack();
                ToggleUiVisibility(false);
                playerS.switchingTurn();
            }
        }
    }

    private void UsePepperAtt()
    {
        Debug.Log("Use Pepper Attack button clicked!");
        if (useModularItemSystem && pepperAttAsset != null)
        {
            if (playerInventory.GetItemQuantity(pepperAttAsset.itemId) > 0)
            {
                playerInventory.RemoveItem(pepperAttAsset.itemId, 1);
                playerS.actionEmpower();
                UpdateInventoryUI();
                FinalizeAttack();
                ToggleUiVisibility(false);
                playerS.switchingTurn();
            }
        }
        else // LEGACY CODE
        {
            if (playerInventory.pepperAttInv > 0)
            {
                playerInventory.removePepperAtt(1);
                playerS.actionEmpower();
                UpdateInventoryUI();
                FinalizeAttack();
                ToggleUiVisibility(false);
                playerS.switchingTurn();
            }
        }
    }

    private void UsePepperDef()
    {
        Debug.Log("Use Pepper Defense button clicked!");
        if (useModularItemSystem && pepperDefAsset != null)
        {
            if (playerInventory.GetItemQuantity(pepperDefAsset.itemId) > 0)
            {
                playerInventory.RemoveItem(pepperDefAsset.itemId, 1);
                playerS.actionDefenseBuff(); // <-- Just empty parentheses!
                UpdateInventoryUI();
                FinalizeAttack();
                ToggleUiVisibility(false);
                playerS.switchingTurn();
            }
        }
        else // LEGACY CODE
        {
            if (playerInventory.pepperDefInv > 0)
            {
                playerInventory.removePepperDef(1);
                playerS.actionDefenseBuff();
                UpdateInventoryUI();
                FinalizeAttack();
                ToggleUiVisibility(false);
                playerS.switchingTurn();
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

        // ==========================================
        // NEW MODULAR DESCRIPTIONS
        // ==========================================
        if (useModularItemSystem)
        {
            string newText = "Description missing!";

            // Match the ID to our new assets and pull the description directly!

            // --- ITEMS ---
            if (id == "Cheese" && cheeseAsset != null) newText = cheeseAsset.itemDescription;
            else if (id == "Banana" && bananaAsset != null) newText = bananaAsset.itemDescription;
            else if (id == "PepperAtt" && pepperAttAsset != null) newText = pepperAttAsset.itemDescription;
            else if (id == "PepperDef" && pepperDefAsset != null) newText = pepperDefAsset.itemDescription;

            // --- SKILLS ---
            else if (id == "Bite" && biteSkillAsset != null) newText = biteSkillAsset.itemDescription;
            else if (id == "Fracture" && fragmentationSkillAsset != null) newText = fragmentationSkillAsset.itemDescription;
            else if (id == "Fireball" && fireballSkillAsset != null) newText = fireballSkillAsset.itemDescription;
            else if (id == "Absorption" && absorptionSkillAsset != null) newText = absorptionSkillAsset.itemDescription;

            // Note: You will need to add assets for Fracture, Fireball, etc., at the top of your script 
            // and add their 'else if' lines here once you make them!

            descriptionDisplayLabel.text = newText;
            descriptionDisplayLabel.style.visibility = Visibility.Visible;
            return;
        }
        // ==========================================
        // OLD CODE
        // ==========================================
        else
        {
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
    }
}
