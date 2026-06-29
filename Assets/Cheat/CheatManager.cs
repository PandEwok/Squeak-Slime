using System.Collections.Generic;
using UnityEngine;
using static TeethCheat;

public class CheatManager : MonoBehaviour
{
    private FloorCheat floorCheat;
    private ItemCheat itemCheat;
    private SkillCheat skillCheat;
    private AttackCheat attackCheat;
    private HpCheat hpCheat;
    private TeethCheat teethCheat;

    [Header("Configuration des Triches")]
    [SerializeField] private List<ItemCheat.Item> itemsCheatList;
    [SerializeField] private List<TeethCheat.Teeth> teethCheatList;

    void Start()
    {
        if (Player.Instance != null && Player.Instance.cheat)
        {
            floorCheat = gameObject.AddComponent<FloorCheat>();
            itemCheat = gameObject.AddComponent<ItemCheat>();
            skillCheat = gameObject.AddComponent<SkillCheat>();
            hpCheat = gameObject.AddComponent<HpCheat>();
            teethCheat = gameObject.AddComponent<TeethCheat>();
            attackCheat = gameObject.AddComponent<AttackCheat>();

            itemCheat.Setup(itemsCheatList);
            teethCheat.Setup(teethCheatList);
            Debug.Log("Activated cheat system");
        }
        else
        {
            Debug.Log("Cheats are disabled.");
        }
    }
}