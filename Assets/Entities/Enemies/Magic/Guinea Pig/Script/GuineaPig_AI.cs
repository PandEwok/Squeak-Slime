using UnityEngine;
using Random = UnityEngine.Random;
using System.Threading.Tasks;

public class GuineaPig_AI : Magic_AI
{

    public async override Task playTurn(GameObject target)
    {
        if (!stats.isDizzy)
        {
            int actionChoiceChance = 0;
            int actionChoice = Random.Range(0, 100);
            Debug.Log($"{this.gameObject.name} action choice: {actionChoice} (empower delay: {empowerDelay})");
            if (empowerDelay <= 0)
            {
                actionChoiceChance = 30;
            }
            if (actionChoice < actionChoiceChance)
            {
                await Task.Run(() =>
                {
                    actionEmpower(EmpowerType.DAMAGE, 0.8f, 1, 0);
                });

                GameObject projInstance = Instantiate(projectilePF, transform.position, Quaternion.identity, transform);
                projInstance.GetComponent<EnemyProjectile>().Init(this.gameObject, target);

                await distanceAttack(target);
                if (target.GetComponent<PlayerStats>() != null)
                {
                    target.GetComponent<PlayerStats>().MakeBurned();
                }
            }
            else
            {
                GameObject projInstance = Instantiate(projectilePF, transform.position, Quaternion.identity, transform);
                projInstance.GetComponent<EnemyProjectile>().Init(this.gameObject, target);

                await distanceAttack(target);
                if (target.GetComponent<PlayerStats>() != null)
                {
                    target.GetComponent<PlayerStats>().MakeBurned();
                }
            }
        }

        await base.playTurn(target);
    }

    public override void Update()
    {
        base.Update();
    }
}
