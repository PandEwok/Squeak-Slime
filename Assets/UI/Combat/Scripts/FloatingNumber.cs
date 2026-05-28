using System;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class FloatingNumber : MonoBehaviour
{
    Vector2 basePos;
    Vector2 endPos;
    float timer = 0;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        basePos = this.transform.position;
        endPos = basePos + new Vector2(0,1);
    }

    // Update is called once per frame
    void Update()
    {
        timer += Time.deltaTime;
        float t = timer / 1.0f; // Assuming 1 second duration

        if (Mathf.Lerp(1, 0, t) > 0)
        {
            if (t > 0)
            {
                this.transform.position = Vector2.Lerp(basePos, endPos, t);

                TextMeshProUGUI text = this.GetComponent<TextMeshProUGUI>();
                Color color = text.color;
                color.a = Mathf.Lerp(1, 0, t); // Fade out over time
                text.color = color;
            }
        }
        else
        {
            Destroy(this.gameObject);
        }
    }
}
