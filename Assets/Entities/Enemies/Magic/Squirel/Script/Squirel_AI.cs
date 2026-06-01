using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;
using System.Threading.Tasks;

public class Squirel_AI : Magic_AI
{

    int GetRandomIndexExcept(int count, int excluded)
    {
        System.Random rnd = new System.Random();
        int r = rnd.Next(count - 1);

        return (r < excluded) ? r : r + 1;
    }


    void healAlly()
    {
        if (enemies.Count > 1 )
        {
            int randomAllyIndex = GetRandomIndexExcept(enemies.Count, ownIndex);
            enemies[randomAllyIndex].GetComponent<Stats_System>().heal(10);
            Debug.Log($"{this.gameObject.name} healed {enemies[randomAllyIndex].name} for 10 health.");
        }
        else if ( enemies.Count > 0 )
        {
            enemies[ownIndex].GetComponent<Stats_System>().heal(10);
            Debug.Log($"{this.gameObject.name} healed itself for 10 health.");
        }
    }

    public async override Task playTurn(GameObject target)
    {
        int actionChoiceChance = 0;
        int actionChoice = Random.Range(0, 100);
        Debug.Log($"{this.gameObject.name} action choice: {actionChoice} (empower delay: {empowerDelay})");
        if (empowerDelay <= 0)
        {
            actionChoiceChance = 100;
        }
        if (actionChoice < actionChoiceChance)
        {
            healAlly();
        }
        else
        {

            await distanceAttack(target);
        }

        await base.playTurn(target);
    }

    public override void Update()
    {
        base.Update();
    }
}
