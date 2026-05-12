using UnityEngine;
using Random = UnityEngine.Random;
using System.Threading.Tasks;

public class Mouse_AI : Rodent_AI
{
    public GameObject powerEffect;

    public float empowerStrenght = 0.5f;
    int empowerDelay = 0;
    bool empowered = false;
    float particleSpawnTimer = 0;

    public void actionEmpower(float empowerAmount = 0.5f)
    {
        empowerDelay = 2; // Empower lasts for 2 turns
        dmgBuffs.Add(empowerAmount);
        dmgBuffTimers.Add(empowerDelay); // Empower lasts for 2 turns
    }

    public async override Task playTurn(GameObject target)
    {
        await base.playTurn(target);
        if (empowerDelay > 0)
        {
            empowerDelay--;
        }

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
        particleSpawnTimer += Time.deltaTime;

        base.Update();
        empowered = (empowerDelay > 0);
        if (empowered && particleSpawnTimer > 0.1f)
        {
            particleSpawnTimer = 0;
            for (int i = 0; i < 4; i++)
            {
                float randomX = Random.Range(-0.6f, 0.6f);
                float randomY = Random.Range(-0.25f, 0.25f);
                Instantiate(powerEffect, this.transform.position + new Vector3(randomX, -0.2f + randomY, 0), Quaternion.identity, this.transform);
            }
        }
    }

}
