using UnityEngine;
using Random = UnityEngine.Random;
using System.Threading.Tasks;
using NUnit.Framework;
using System.Collections.Generic;

public class Boss_AI : Enemy_AI
{
    public List<float> phases = new List<float>(); // percentages of hp for each phase end

    [SerializeField] protected List<GameObject> summons = new List<GameObject>();

    public int GetCurrentPhase()
    {
        for (int i = phases.Count - 1; i >= 0; i--)
        {
            if (stats.health <= (stats.originalHealth * phases[i]))
            {
                return i+1;
            }
        }
        return -1;
    }

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
