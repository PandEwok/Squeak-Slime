using UnityEngine;
using Random = UnityEngine.Random;

public class Rodent_AI : Enemy_AI
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public override void playTurn(GameObject target)
    {
        attack(target);
    }
    void attack(GameObject target) {
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
