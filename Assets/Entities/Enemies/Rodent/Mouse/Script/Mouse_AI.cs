using UnityEngine;
using Random = UnityEngine.Random;
using System.Threading.Tasks;

public class Mouse_AI : Rodent_AI
{
    public async override Task playTurn(GameObject target)
    {
        await base.playTurn(target);

        int actionChoiceChance = 0;
        int actionChoice = Random.Range(0, 100);
        if (empowerDelay <= 0)
        {
            actionChoiceChance = 65; // 65% chance to empower if not currently empowered
        }
        if (actionChoice < actionChoiceChance)
        {
            actionEmpower(empowerStrenght);
        }
        else
        {
            await closeAttack(target);
        }
    }

    public override void Update()
    {
        base.Update();
    }
}
