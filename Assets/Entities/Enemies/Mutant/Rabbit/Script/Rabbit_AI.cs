using UnityEngine;
using Random = UnityEngine.Random;
using System.Threading.Tasks;

public class Rabbit_AI : Mutant_AI
{
    public async override Task playTurn(GameObject target)
    {
        if (!stats.isDizzy)
        {
            await closeAttack(target);
            if (empowered)
            {
                await closeAttack(target);
            }
            if (target.GetComponent<PlayerStats>() != null)
            {
                target.GetComponent<PlayerStats>().MakeBleeding();
            }
        }
        await base.playTurn(target);
    }

    public override void Update()
    {
        base.Update();
    }

    protected override void rage()
    {
        empowered = true;
        empowerDuration = 1;
    }

    protected override void unRage()
    {
        empowered = false;
        empowerDuration = 0;
    }
}
