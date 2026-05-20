using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Stats_System : MonoBehaviour
{
    public int originalHealth;
    public int damage;
    public int defense;
    public GameObject damagePF;
    Vector3 originalPos;
    Color originalColor;
    public int health;
    [HideInInspector] public bool blocking = false;
    [HideInInspector] public bool defending = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        originalPos = transform.localPosition;
        //originalColor = this.gameObject.GetComponent<SpriteRenderer>().color;
        originalColor = gameObject.GetComponentInChildren<SpriteRenderer>().color;
        health = originalHealth;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private IEnumerator dmgShake()
    {
        float shakeDuration = 0.4f;
        float shakeMagnitude = 0.08f;
        float elapsed = 0.0f;
        while (elapsed < shakeDuration)
        {
            float x = UnityEngine.Random.Range(-1f, 1f) * shakeMagnitude;
            float y = UnityEngine.Random.Range(-1f, 1f) * shakeMagnitude;
            transform.localPosition = new Vector3(originalPos.x + x, originalPos.y + y, originalPos.z);
            elapsed += Time.deltaTime;
            yield return null;
        }
        transform.localPosition = originalPos;
    }

    private IEnumerator dmgShade()
    {
        SpriteRenderer img = GetComponentInChildren<SpriteRenderer>();
        
        Color targetColor = new Color(1f, 0f, 0f, 0.8f);
        float duration = 0.15f;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            img.color = Color.Lerp(originalColor, targetColor, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        elapsed = 0f;
        while (elapsed < duration)
        {
            img.color = Color.Lerp(targetColor, originalColor, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        img.color = originalColor;
    }

    public void takeDamage(int damageAmount)
    {
        int effectiveDamage = Mathf.Max(damageAmount - defense, 0);
        if(blocking)
        {
            effectiveDamage /= 2;
        }
        if (defending)
        {
            effectiveDamage /= 2;
        }
        health -= effectiveDamage;

        GameObject newDmgDisplay;

        Vector3 spawnPos = new Vector3(this.transform.position.x, this.transform.position.y + 2);
        float randomXOffset = UnityEngine.Random.Range(-0.5f, 0.5f);
        spawnPos.x += randomXOffset;
        float randomYOffset = UnityEngine.Random.Range(-0.5f, 0.5f);
        spawnPos.y += randomYOffset;


        StartCoroutine(dmgShake());
        StartCoroutine(dmgShade());

        newDmgDisplay = Instantiate(damagePF, spawnPos, Quaternion.identity, GameObject.FindGameObjectWithTag("Canvas").transform);
        newDmgDisplay.GetComponent<TextMeshProUGUI>().SetText(effectiveDamage.ToString());

        Debug.Log($"{gameObject.name} took {effectiveDamage} damage. Remaining health: {health}");

        if (health <= 0)
        {
            /*Die();*/
        }
    }

    public void heal(int healAmount)
    {
        health += healAmount;
        health = Mathf.Min(health, originalHealth);
        Debug.Log($"{gameObject.name} healed for {healAmount}. Current health: {health}");
    }
}
