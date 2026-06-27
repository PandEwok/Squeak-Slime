using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

public class Alchemist_AI : Boss_AI
{
    enum Effect
    {
        BLEED,
        DIZZY,
        BURN,
        NONE
    }

    protected override void DeathAction()
    {
        SceneManager.LoadSceneAsync(14);
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
            if (GetComponent<Stats_System>().isDizzy && !HasInvincibility())
            {
                actionEmpower(EmpowerType.DEFENSE, 1f, 0, permBuffID);
                newPhase = false;
            }

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

    bool HasInvincibility()
    {
        for (int i = 0; i < defBuffTimers.Count; i++)
        {
            if (defBuffTimers[i] == permBuffID)
            {
                return true;
            }
        }
        return false;
    }

    public void getDizzy()
    {
        for (int i = 0; i < defBuffTimers.Count; i++)
        {
            if (defBuffTimers[i] == permBuffID)
            {
                defBuffTimers.RemoveAt(i);
                defBuffs.RemoveAt(i);
            }
        }
    }

    public override void Update()
    {
        base.Update();

        GetCurrentPhase();   // for variable updating purpose
        if (newPhase)
        {
            if (latestPhase == 2 && !HasInvincibility())
            {
                actionEmpower(EmpowerType.DEFENSE, 1f, 0, permBuffID);
                newPhase = false;
            }
            else if (latestPhase == 1)
            {
                newPhase = false;
            }
        }
    }
}
