using UnityEngine;
using Random = UnityEngine.Random;
using System.Threading.Tasks;

public class Rat_AI : Mutant_AI
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

    public override void attack(GameObject target)
    {

        Stats_System targetStats = target.GetComponent<Stats_System>();

        if (targetStats != null)
        {
            int damageAmount = GetComponent<Stats_System>().damage;
            int randomDmgOffset = Random.Range(-1, 2);
            damageAmount += randomDmgOffset;
            float finalDamage = damageAmount;
            foreach (float buff in dmgBuffs)
            {
                finalDamage += (damageAmount * buff);
            }
            damageAmount = Mathf.RoundToInt(finalDamage);
            targetStats.TakeDamage(damageAmount, false);
            Debug.Log($"{this.gameObject.name} attacked {target.name} for {damageAmount} damage.");

            stats.Heal(damageAmount / 2);
        }
    }
}
