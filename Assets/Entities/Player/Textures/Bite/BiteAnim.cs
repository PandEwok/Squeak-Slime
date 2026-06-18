using UnityEngine;
using System.Collections;
public class BiteAnim : MonoBehaviour
{
    public GameObject up;
    public GameObject down;
    float upOriginalY;
    float downOriginalY;

    [Header("Réglages de l'Animation")]
    [Tooltip("Durée de l'animation en secondes")]
    public float duration = 0.2f;

    float finalY = 0f;
    public void Awake()
    {
        if (up != null) upOriginalY = up.transform.localPosition.y;
        if (down != null) downOriginalY = down.transform.localPosition.y;
    }
    void Start()
    {
        ResetPositions();
        Animation();
    }

    public void Animation()
    {
        StopAllCoroutines();
        StartCoroutine(BiteRoutine());
    }
    private IEnumerator BiteRoutine()
    {
        float elapsedTime = 0f;

        Vector3 upStartPos = new Vector3(up.transform.localPosition.x, upOriginalY, up.transform.localPosition.z);
        Vector3 downStartPos = new Vector3(down.transform.localPosition.x, downOriginalY, down.transform.localPosition.z);

        Vector3 upTargetPos = new Vector3(up.transform.localPosition.x, finalY, up.transform.localPosition.z);
        Vector3 downTargetPos = new Vector3(down.transform.localPosition.x, finalY, down.transform.localPosition.z);

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;

            float percentageComplete = elapsedTime / duration;

            up.transform.localPosition = Vector3.Lerp(upStartPos, upTargetPos, percentageComplete);
            down.transform.localPosition = Vector3.Lerp(downStartPos, downTargetPos, percentageComplete);

            yield return null;
        }

        up.transform.localPosition = upTargetPos;
        down.transform.localPosition = downTargetPos;

        Destroy(gameObject, 0.1f);
    }

    private void ResetPositions()
    {
        if (up != null)
            up.transform.localPosition = new Vector3(up.transform.localPosition.x, upOriginalY, up.transform.localPosition.z);
        if (down != null)
            down.transform.localPosition = new Vector3(down.transform.localPosition.x, downOriginalY, down.transform.localPosition.z);
    }
}
