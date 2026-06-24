using UnityEngine;
using Random = UnityEngine.Random;
using System.Threading.Tasks;
using System;

public class Alchemist_AI : Boss_AI
{
    enum Effect
    {
        BLEED,
        DIZZY,
        BURN,
        NONE
    }

    public async override Task playTurn(GameObject target)
    {
        Debug.Log("-- alchemist phase : " + GetCurrentPhase());

        if (GetCurrentPhase() == 1)
        {
            if (!stats.isDizzy)
            {
                int randAction = Random.Range(0, 100);

                if (randAction < 40 && enemies.Count <= 3 && summons.Count != 0)
                {
                    GameObject posToSpawn = combatLogic.GetComponent<Combat_Logic>().availableEnemyPos[0];
                    GameObject randSummon = summons[Random.Range(0, summons.Count)];

                    GameObject newEnemy = Instantiate(randSummon, posToSpawn.transform.position, Quaternion.identity, combatLogic.transform);

                    combatLogic.GetComponent<Combat_Logic>().inpendingEnemySummon.Add(newEnemy, posToSpawn);
                    /*combatLogic.GetComponent<Combat_Logic>().enemies.Add(newEnemy);
                    combatLogic.GetComponent<Combat_Logic>().enemyPositions.Add(posToSpawn);*/
                    combatLogic.GetComponent<Combat_Logic>().availableEnemyPos.RemoveAt(0);
                }

                else if (randAction < 100)
                {

                    GameObject projInstance = Instantiate(projectilePF, transform.position, Quaternion.identity, transform);
                    projInstance.GetComponent<EnemyProjectile>().Init(this.gameObject, target);

                    await distanceAttack(target);

                    int randEffect = (int)Effect.NONE;
                    randEffect = Random.Range(0, randEffect);
                    if (randEffect == (int)Effect.BLEED)
                    {
                        target.GetComponent<PlayerStats>().MakeBleeding();
                    }
                    else if (randEffect == (int)Effect.DIZZY)
                    {
                        target.GetComponent<PlayerStats>().MakeDizzy();
                    }
                    else if (randEffect == (int)Effect.BURN)
                    {
                        target.GetComponent<PlayerStats>().MakeBurned();
                    }
                }
            }
        }
        else if (GetCurrentPhase() == 2)
        {
            if (!stats.isDizzy)
            {
                GameObject projInstance = Instantiate(projectilePF, transform.position, Quaternion.identity, transform);
                projInstance.GetComponent<EnemyProjectile>().Init(this.gameObject, target);

                await distanceAttack(target);
            }
            else
            {
                
            }
        }

        await base.playTurn(target);
    }

    public override void Update()
    {
        base.Update();
    }
}
