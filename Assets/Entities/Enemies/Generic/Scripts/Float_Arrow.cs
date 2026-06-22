using UnityEngine;

public class Float_Arrow : MonoBehaviour
{
    Vector3 startPos;
    Vector3 endPos;

    float timer = 0f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        startPos = transform.position;
        endPos = startPos + new Vector3(0, 0.6f, 0);
    }

    // Update is called once per frame
    void Update()
    {
        timer += Time.deltaTime;
        float t = Mathf.PingPong(timer, 1f);
        this.gameObject.transform.position = Vector3.Lerp(startPos, endPos, t);
    }
}
