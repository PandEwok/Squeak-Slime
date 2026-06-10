using UnityEngine;
using Random = UnityEngine.Random;
using System.Threading.Tasks;

public class Mutant_AI : Enemy_AI
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

    protected virtual void rage()
    {
        empowered = true;
        actionEmpower(EmpowerType.DAMAGE, 0.5f, 0, permBuffID);
        empowerDuration = 1;
    }
    protected virtual void unRage()
    {
        empowered = false;
        for (int i = 0; i < dmgBuffTimers.Count; i++)
        {
            if (dmgBuffTimers[i] == permBuffID)
            {
                dmgBuffTimers.RemoveAt(i);
                dmgBuffs.RemoveAt(i);
            }
        }
        empowerDuration = 0;
    }

    public override void Update()
    {
        base.Update();

        if (stats != null)
        {
            if (stats.health <= (stats.originalHealth * 0.35f) && !empowered)
            {
                rage();
            }
            if (empowered && (stats.health > (stats.originalHealth * 0.35f)))
            {
                unRage();
            }
        }
    }

    public override void newTurnCount()
    {
        countBuffTimers();
        StartCoroutine(GetComponent<Stats_System>().ApplyStatus());
    }
}
