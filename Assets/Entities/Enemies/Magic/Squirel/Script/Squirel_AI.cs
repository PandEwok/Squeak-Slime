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

    int getAllyDamaged()
    {
        int count = 0;
        
        for (int i = 0; i < enemies.Count; i++)
        {
            if (i != ownIndex && enemies[i].GetComponent<Stats_System>().health < enemies[i].GetComponent<Stats_System>().originalHealth)
            {
                count++;
            }
        }

        return count;
    }

    bool getSelfDamaged()
    {
        return gameObject.GetComponent<Stats_System>().health < gameObject.GetComponent<Stats_System>().originalHealth;
    }


    void healAlly()
    {
        if (enemies.Count > 1 && getAllyDamaged() > 0)
        {
            int randomAllyIndex = GetRandomIndexExcept(enemies.Count, ownIndex);
            enemies[randomAllyIndex].GetComponent<Stats_System>().Heal(10);
            Debug.Log($"{this.gameObject.name} healed {enemies[randomAllyIndex].name} for 10 health.");
        }
        else if ( enemies.Count > 0 && getSelfDamaged() )
        {
            gameObject.GetComponent<Stats_System>().Heal(10);
            Debug.Log($"{this.gameObject.name} healed itself for 10 health.");
        }
    }

    public async override Task playTurn(GameObject target)
    {
        if (!stats.isDizzy)
        {
            int healChance = 0;
            int actionChoice = Random.Range(0, 100);
            Debug.Log($"{this.gameObject.name} action choice: {actionChoice} (empower delay: {empowerDelay})");
            if (empowerDelay <= 0 && (getSelfDamaged() || getAllyDamaged() > 0))
            {
                healChance = 70;
            }
            if (actionChoice < healChance)
            {
                healAlly();
            }
            else
            {
                int buffChance = 50;
                actionChoice = Random.Range(0, 100);
                if (actionChoice < buffChance && empowerDelay <= 0 && enemies.Count > 1)
                {
                    int randomAllyIndex = GetRandomIndexExcept(enemies.Count, ownIndex);
                    int duration = 2;
                    empowerDelay = duration + 1;
                    enemies[randomAllyIndex].GetComponent<Enemy_AI>().addBuff(Enemy_AI.EmpowerType.DEFENSE, 0.5f, duration);
                }
                else
                {
                    await distanceAttack(target);
                }
            }
        }

        await base.playTurn(target);
    }

    public override void Update()
    {
        base.Update();
    }
}
