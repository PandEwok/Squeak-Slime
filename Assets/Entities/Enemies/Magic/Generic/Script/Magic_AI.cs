using UnityEngine;
using Random = UnityEngine.Random;
using System.Threading.Tasks;

public class Magic_AI : Enemy_AI
{
    public async override Task playTurn(GameObject target)
    {
        await base.playTurn(target);
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
        }
    }

    public override void Update()
    {
        base.Update();
    }
}
