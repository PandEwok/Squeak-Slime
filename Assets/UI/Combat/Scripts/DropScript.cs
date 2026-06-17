using System;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class DropScript : MonoBehaviour
{
    Vector2 basePos;
    Vector2 endPos;
    float timer = 0;
    int step = 0;

    Vector3 BezierQuadratic(Vector3 p0, Vector3 p1, Vector3 p2, float t)
    {
        float u = 1f - t;
        return u * u * p0
             + 2f * u * t * p1
             + t * t * p2;
    }

    void destroyObject()
    {
        Destroy(this.gameObject);
    }

    IEnumerator switchToSecondStep()
    {
        basePos = this.transform.position;
        endPos = GameObject.FindGameObjectWithTag("Player").transform.position;
        timer = 0;
        yield return new WaitForSeconds(0.8f);
        step = 2;
    }

void Start()
    {
        basePos = this.transform.position;
        endPos = basePos + new Vector2(-2, 0);
    }

    void Update()
    {
        if (step != 1)
        { timer += Time.deltaTime; }

        if (step == 0)
        {
            float t = timer / 0.7f;

            transform.position = BezierQuadratic(basePos, basePos + new Vector2(-0.7f, 1.2f), endPos, Mathf.Clamp01(t));

            if (transform.position.ConvertTo<Vector2>() == endPos)
            {
                step = 1;
                StartCoroutine(switchToSecondStep());
            }
        }
        else if (step == 2)
        {
            float t = timer / 0.4f;
            transform.position = Vector2.Lerp(basePos, endPos, Mathf.Clamp01(t));
            if (transform.position.ConvertTo<Vector2>() == endPos)
            {
                destroyObject();
            }
        }
    }
}
