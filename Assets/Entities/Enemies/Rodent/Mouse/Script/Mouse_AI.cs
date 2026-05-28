using UnityEngine;
using Random = UnityEngine.Random;
using System.Threading.Tasks;

public class Mouse_AI : Rodent_AI
{
    public async override Task playTurn(GameObject target)
    {
        await closeAttack(target);

        await base.playTurn(target);
    }

    public override void Update()
    {
        base.Update();
    }
}
