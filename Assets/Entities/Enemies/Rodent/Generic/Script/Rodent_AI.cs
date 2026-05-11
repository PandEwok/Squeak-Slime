using UnityEngine;
using Random = UnityEngine.Random;
using System.Threading.Tasks;

public class Rodent_AI : Enemy_AI
{

    public async override Task playTurn(GameObject target)
    {
        await closeAttack(target);
    }
    public override void attack(GameObject target) {
        
        Stats_System targetStats = target.GetComponent<Stats_System>();

        if (targetStats != null)
        {
            int damageAmount = GetComponent<Stats_System>().damage;
            int randomDmgOffset = Random.Range(-2, 3);
            damageAmount += randomDmgOffset;
            targetStats.takeDamage(damageAmount);
            Debug.Log($"{this.gameObject.name} attacked {target.name} for {damageAmount} damage.");
        }
    }
}
