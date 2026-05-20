using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class Enemy_AI : MonoBehaviour
{
    float timer = 0;
    bool isMoving = false;
    Vector2 moveTarget;
    Vector2 moveStart;
    Vector2 basePos;
    Vector2 pos;
    float securityTimer = 0;

    protected List<float> dmgBuffs = new List<float>();
    protected List<float> defBuffs = new List<float>();
    protected List<int> dmgBuffTimers = new List<int>();

    public GameObject powerEffect;
    public float empowerStrenght = 0.5f;
    protected int empowerDelay = 0;
    protected bool empowered = false;
    protected float particleSpawnTimer = 0;
    GameObject player;
    playerScript playerCombat;


    public void actionEmpower(float empowerAmount = 0.5f, int delay = 2)
    {
        empowerDelay = delay + 1; // Empower lasts for 2 turns
        dmgBuffs.Add(empowerAmount);
        dmgBuffTimers.Add(empowerDelay);
    }



    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        basePos = getPos();
    }

    Vector2 getPos()
    {
        return this.gameObject.transform.position;
    }
    Vector2 setPos(Vector2 newPos)
    {
        this.gameObject.transform.position = newPos;
        return newPos;
    }

    // Update is called once per frame
    public virtual void Update()
    {
        particleSpawnTimer += Time.deltaTime;
        securityTimer += Time.deltaTime;
        pos = getPos();


        if (isMoving && moveTarget != null && moveStart != null) {
            //Debug.Log($"moving to {moveTarget}");
            timer += Time.deltaTime;
            float t = timer / 0.5f;
            setPos(Vector2.Lerp(moveStart, moveTarget, t));
            if (Vector2.Distance(getPos(), moveTarget) < 0.001f) {
                isMoving = false;
                setPos(moveTarget);
            }
        }

        empowered = (empowerDelay > 0);
        //Debug.Log($"{this.gameObject.name} empower delay: {empowerDelay}, empowered: {empowered}");
        if (empowered && particleSpawnTimer > 0.1f)
        {
            particleSpawnTimer = 0;
            for (int i = 0; i < 4; i++)
            {
                float randomX = Random.Range(-0.6f, 0.6f);
                float randomY = Random.Range(-0.25f, 0.25f);
                Instantiate(powerEffect, this.transform.position + new Vector3(randomX, -0.2f + randomY, 0), Quaternion.identity, this.transform);
            }
        }
    }

    public void newTurnCount()
    {
        empowerDelay = Mathf.Max(0, empowerDelay - 1);
        for (int i = 0; i < dmgBuffTimers.Count; i++)
        {
            if (dmgBuffTimers[i] <= 0)
            {
                dmgBuffTimers.RemoveAt(i);
                dmgBuffs.RemoveAt(i);
                i--;
            }
            else
            {
                dmgBuffTimers[i]--;
            }
        }
    }

    public async virtual Task playTurn(GameObject target) {
        
        /*if (empowerDelay > 0)
        {
            empowerDelay--;
        }*/
        newTurnCount();
    }

    public virtual void attack(GameObject target)
    {
        Debug.Log($"{this.gameObject.name} tried to attack {target.name}, but no attack was defined.");
    }

    public async Task closeAttack(GameObject target)
    {
        ///
        Vector2 targetPos = target.transform.position + new Vector3(2.5f, 0, 0);
        
        timer = 0;
        moveTarget = targetPos;
        moveStart = getPos();
        isMoving = true;

        securityTimer = 0;
        await Task.Run(() =>
        {
            while (Vector2.Distance(pos, moveTarget) > 0.001f)
            {
                if (securityTimer > 10.0f) break;
            }
        });
        ///
        player = GameObject.FindGameObjectWithTag("Player");
        playerCombat = player.GetComponent<playerScript>();

        if (player.GetComponent<playerScript>() != null)
        {
            Coroutine qteCouroutine = playerCombat.StartCoroutine(playerCombat.TriggerDefenseQTE(0.4f));
            await Task.Delay((int)secToMili(0.4f));

            await Task.Delay((int)secToMili(0.1f));
        }
        else
        {
            await Task.Delay((int)secToMili(0.3f));
        }

        await Task.Delay((int)secToMili(0.3f));
        attack(target);
        await Task.Delay((int)secToMili(0.3f));

        ///
        moveStart = getPos();
        moveTarget = basePos;
        timer = 0;
        isMoving = true;

        securityTimer = 0;
        await Task.Run(() =>
        {
            while (Vector2.Distance(pos, moveTarget) > 0.001f)
            {
                if (securityTimer > 10.0f) break;
            }
        });
        setPos(basePos);
        ///
    }

    float secToMili(float seconds) {
        return seconds * 1000;
    }
}
