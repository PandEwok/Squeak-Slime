using System;
using Unity.VisualScripting;
using UnityEngine;

public class EnemyProjectile : MonoBehaviour
{
    protected GameObject parent;
    protected GameObject target;
    [HideInInspector] public Vector2 parentPos;
    [HideInInspector] public Vector2 targetPos;

    [HideInInspector] public float timer = 0;

    [HideInInspector] public bool sentBack = false;

    Vector3 BezierQuadratic(Vector3 p0, Vector3 p1, Vector3 p2, float t)
    {
        float u = 1f - t;
        return u * u * p0
             + 2f * u * t * p1
             + t * t * p2;
    }

    Vector3 BezierQuadraticTangent(Vector3 p0, Vector3 p1, Vector3 p2, float t)
    {
        return 2f * (1f - t) * (p1 - p0)
             + 2f * t * (p2 - p1);
    }




    public void Init(GameObject parent_p, GameObject target_p)
    {
        parent = parent_p;
        target = target_p;
        parentPos = parent_p.transform.position + new Vector3(0, 2.5f, 0);
        targetPos = target_p.transform.position + new Vector3(0, 1.5f, 0);
    }


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    protected virtual void ProjectileEffect()
    {

    }

    // Update is called once per frame
    protected virtual void Update()
    {
        timer += Time.deltaTime;
        float t = timer / 0.85f;

        transform.position = BezierQuadratic(parentPos, parentPos + new Vector2(-3, 5), targetPos, t);

        Vector3 tan = BezierQuadraticTangent(parentPos, parentPos + new Vector2(-3, 5), targetPos, t).normalized;
        float angle = Mathf.Atan2(tan.y, tan.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);

        float errorRange = 0.1f;
        if ( ( (transform.position.x <= targetPos.x + errorRange) && (transform.position.x >= targetPos.x - errorRange) ) &&
             ( (transform.position.y <= targetPos.y + errorRange) && (transform.position.y >= targetPos.y - errorRange) ) )
        {
            ProjectileEffect();
            Destroy(this.gameObject);
        }
    }
}
