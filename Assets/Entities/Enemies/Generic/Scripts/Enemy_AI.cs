using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
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

    public List<float> dmgBuffs = new List<float>();
    public List<float> defBuffs = new List<float>();
    protected List<int> dmgBuffTimers = new List<int>();
    protected List<int> defBuffTimers = new List<int>();

    public GameObject powerEffect;
    public float empowerStrenght = 0.5f;
    protected int empowerDelay = 0;
    protected int empowerDuration = 0;
    protected bool empowered = false;
    protected float particleSpawnTimer = 0;
    GameObject player;
    Player playerCombat;

    bool selected = false;

    protected List<GameObject> enemies;
    protected int ownIndex;

    float delta = 0;

    public GameObject projectilePF;

    protected Stats_System stats;

    protected int permBuffID = 131313;

    [SerializeField] public Tooth teethType;

    [SerializeField] protected GameObject dropPF;

    public enum attackDirection
    {
        NONE,
        TOP,
        FRONT,
    }
    public attackDirection directionalResistance = attackDirection.NONE;
    public float dResistanceAmount = 0f;

    void setArrow(bool value) {
        transform.Find("SelectArrow").gameObject.SetActive(value);
    }

    private void Awake()
    {
        setArrow(false);
    }

    public void select()
    {
        selected = true;
        setArrow(true);
    }

    public void deselect()
    {
        selected = false;
        setArrow(false);
    }

    public bool isSelected()
    {
        return selected;
    }


    public enum EmpowerType
    {
        DAMAGE,
        DEFENSE
    }

    public void addBuff(EmpowerType type, float amount, int duration)
    {
        if (type == EmpowerType.DEFENSE)
        {
            defBuffs.Add(amount);
            defBuffTimers.Add(duration);
        }
        else if (type == EmpowerType.DAMAGE)
        {
            dmgBuffs.Add(amount);
            dmgBuffTimers.Add(duration);
        }
    }


    public void actionEmpower(EmpowerType type, float empowerAmount = 0.5f, int delay = 2, int duration = 2)
    {
        empowerDelay = delay + 1; // Empower lasts for 2 turns
        empowerDuration = duration + 1;
        Debug.Log($"type is {type}");

        addBuff(type, empowerAmount, duration);
    }



    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        basePos = getPos();
        player = GameObject.FindGameObjectWithTag("Player");
        playerCombat = player.GetComponent<Player>();

        enemies = GameObject.Find("CombatLogic").GetComponent<Combat_Logic>().enemies;
        ownIndex = enemies.IndexOf(this.gameObject);

        stats = GetComponent<Stats_System>();
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

    protected virtual int dropTeeth()
    {
        int teethDropped = Random.Range(1, 4);

        PlayerInventory inv = player.GetComponent<PlayerInventory>();
        if (inv != null)
        {
            inv.AddTooth(teethType, teethDropped);
            Instantiate(dropPF, transform.position, Quaternion.identity, GameObject.FindGameObjectWithTag("Canvas").transform);
        }

        return teethDropped;
    }


    // Update is called once per frame
    public virtual void Update()
    {
        delta = Time.deltaTime;

        ownIndex = enemies.IndexOf(this.gameObject);

        particleSpawnTimer += Time.deltaTime;
        securityTimer += Time.deltaTime;
        pos = getPos();


        if (gameObject.GetComponent<Stats_System>().health <= 0)
        {
            if (player != null) { dropTeeth(); }
            if (this.gameObject == null)
            {
                Debug.Log("enemy null");
            }
            if (GameObject.FindGameObjectWithTag("CombatLogic") == null)
            {
                Debug.Log("logic null");
            }
            GameObject.FindGameObjectWithTag("CombatLogic").GetComponent<Combat_Logic>().removeEnemy(this.gameObject);
        }


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

        empowered = (empowerDuration > 0);
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

    protected void countBuffTimers()
    {
        for (int i = 0; i < dmgBuffTimers.Count; i++)
        {
            if (dmgBuffTimers[i] != permBuffID)
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
        for (int i = 0; i < defBuffTimers.Count; i++)
        {
            if (defBuffTimers[i] != permBuffID)
            {
                if (defBuffTimers[i] <= 0)
                {
                    defBuffTimers.RemoveAt(i);
                    defBuffs.RemoveAt(i);
                    i--;
                }
                else
                {
                    defBuffTimers[i]--;
                }
            }
        }
    }

    public virtual void newTurnCount()
    {
        empowerDelay = Mathf.Max(0, empowerDelay - 1);
        empowerDuration = Mathf.Max(0, empowerDuration - 1);
        countBuffTimers();
        StartCoroutine(GetComponent<Stats_System>().ApplyStatus());
    }

    public async virtual Task playTurn(GameObject target)
    {
        if (!stats.isDizzy)
        {
            newTurnCount();
        }
        else
        {
            StartCoroutine(GetComponent<Stats_System>().ApplyStatus());
        }
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
        bool hasFailedQTE = false;

        securityTimer = 0;

        playerCombat.uiManager.ShowQTE(true, false);
        await Task.Run(() =>
        {
            while (Vector2.Distance(pos, moveTarget) > 0.001f)
            {
                if (Pointer.current.press.wasPressedThisFrame)
                {
                    hasFailedQTE = true;
                    
                }

                if (securityTimer > 10.0f) break;
            }
        });
        ///

        if (player.GetComponent<Player>() != null)
        {
            if(!hasFailedQTE)
            { 
                playerCombat.inventory.defenseAction.Execute(playerCombat, 0.4f); 
            }
            else
            {
                playerCombat.uiManager.DisplayGrade(GradeScript.Grade.Missed, true);
                playerCombat.uiManager.ShowQTE(false);
            }
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

    public async Task distanceAttack(GameObject target)
    {
        bool hasFailedQTE = false;
        float elapsedTime = 0;

        while (elapsedTime <= 0.2f)
        {
            if (Pointer.current.press.wasPressedThisFrame)
            {
                hasFailedQTE = true;
                break; //attention animation
            }

            await Task.Yield();
            elapsedTime += Time.deltaTime;
        }

        if (player.GetComponent<Player>() != null)
        {
            if (!hasFailedQTE)
            {
                playerCombat.inventory.defenseAction.Execute(playerCombat, 0.4f);
            }
            else
            {
                playerCombat.uiManager.DisplayGrade(GradeScript.Grade.Missed, true);
            }
            await Task.Delay((int)secToMili(0.4f));

            await Task.Delay((int)secToMili(0.1f));
        }
        else
        {
            await Task.Delay((int)secToMili(0.3f));
        }

        await Task.Delay((int)secToMili(0.2f));
        attack(target);
    }


    protected float secToMili(float seconds) {
        return seconds * 1000;
    }
}
