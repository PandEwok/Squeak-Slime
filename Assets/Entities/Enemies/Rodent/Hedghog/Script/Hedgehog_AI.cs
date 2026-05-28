using UnityEngine;
using Random = UnityEngine.Random;
using System.Threading.Tasks;

public class Hedgehog_AI : Rodent_AI
{
    public async override Task playTurn(GameObject target)
    {
        int actionChoiceChance = 0;
        int actionChoice = Random.Range(0, 100);
        Debug.Log($"{this.gameObject.name} action choice: {actionChoice} (empower delay: {empowerDelay})");
        if (empowerDelay <= 0)
        {
            actionChoiceChance = 85; // 85% chance to empower if not currently empowered
        }
        if (actionChoice < actionChoiceChance)
        {
            actionEmpower(EmpowerType.DEFENSE, empowerStrenght, 3, 1);
        }
        else
        {
            await closeAttack(target);
        }

        await base.playTurn(target);
    }

    public override void Update()
    {
        base.Update();
    }
}
