using UnityEngine;
using Random = UnityEngine.Random;
using System.Threading.Tasks;

public class Dormouse_AI : Rodent_AI
{
    public async override Task playTurn(GameObject target)
    {
        if (!stats.isDizzy)
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
                await Task.Run(() =>
                {
                    actionEmpower(EmpowerType.DAMAGE, 1.2f, 3, 1);
                });

                //await distanceAttack(target);
            }
            else
            {

                await distanceAttack(target);
            }
        }

        await base.playTurn(target);
    }

    public override void Update()
    {
        base.Update();
    }
}
