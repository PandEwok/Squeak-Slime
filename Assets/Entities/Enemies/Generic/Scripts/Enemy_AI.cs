using System.Collections;
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
    void Update()
    {
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
    }

    public async virtual Task playTurn(GameObject target) {
        Debug.Log($"{this.gameObject.name} played their turn, but no action was defined.");
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
