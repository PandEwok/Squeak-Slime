using TMPro;
using UnityEngine;

public class PowerEffect : MonoBehaviour
{
    Vector2 basePos;
    Vector2 endPos;
    float timer = 0;
    public GameObject child;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        basePos = this.transform.position;
        endPos = basePos + new Vector2(0, 1);
    }

    // Update is called once per frame
    void Update()
    {
        timer += Time.deltaTime;
        float t = timer / 0.5f;

        if (Mathf.Lerp(1, 0, t) > 0)
        {
            if (t > 0)
            {
                child.transform.position = Vector2.Lerp(basePos, endPos, t);

                child.GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, Mathf.Lerp(1, 0, t)); // Fade out over time
            }
        }
        else
        {
            Destroy(this.gameObject);
        }
    }
}
