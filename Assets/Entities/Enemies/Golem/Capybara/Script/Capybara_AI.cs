using UnityEngine;
using Random = UnityEngine.Random;
using System.Threading.Tasks;

public class Capybara_AI : Golem_AI
{
    public async override Task playTurn(GameObject target)
    {
        if (!stats.isDizzy)
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
