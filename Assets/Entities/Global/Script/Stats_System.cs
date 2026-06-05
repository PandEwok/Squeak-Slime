using System;
using System.Collections;
using TMPro;
using Unity.Mathematics;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEditor.ShaderGraph;
using UnityEngine;
using UnityEngine.UI;

public class Stats_System : MonoBehaviour
{
    
    public int originalHealth;
    public int damage;
    public int defense;
    public GameObject damagePF;
    public GameObject bloodPF;
    public GameObject firePF;
    public GameObject dizzyPF;
    Vector3 originalPos;
    Color originalColor;
    [HideInInspector] public Color absorptionColor;
    public int health;
    [HideInInspector] public bool blocking = false;
    [HideInInspector] public bool defending = false;
    public bool isBleeding = false;
    public bool isOnFire = false;
    public bool isDizzy = false;
    public bool hasAbsorption = false;
    public int bleedDamage = 5;
    public float fireDamage = 18; //Utilise pour calculer une proportion, n'inflige pas 18
    public int bleedingDuration = 3;
    public int fireDuration = 3;
    public int absorptionDuration = 3;
    public int dizzyDuration = 1;
    public int bleedingTimer = 0;
    public int fireTimer = 0;
    public int absorptionTimer = 0;
    public int dizzyTimer = 0;
    private GameObject bleedingInstance;
    private GameObject fireInstance;
    private GameObject dizzyInstance;
    private GameObject player;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        originalPos = transform.localPosition;
        //originalColor = this.gameObject.GetComponent<SpriteRenderer>().color;
        originalColor = gameObject.GetComponentInChildren<SpriteRenderer>().color;
        absorptionColor = new Color(1f, 0, 0, 1);
        health = originalHealth;
        if(this.CompareTag("Player"))
        {
            player = this.gameObject;
        }
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
            if(!hasAbsorption) img.color = Color.Lerp(originalColor, targetColor, elapsed / duration);
            else img.color = Color.Lerp(absorptionColor, targetColor, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        elapsed = 0f;
        while (elapsed < duration)
        {
            if(!hasAbsorption) img.color = Color.Lerp(targetColor, originalColor, elapsed / duration);
            else img.color = Color.Lerp(targetColor, absorptionColor, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        if (!hasAbsorption) img.color = originalColor;
        else img.color = absorptionColor;
    }

    public int takeDamage(int damageAmount, bool isStatusDamage)
    {
        int effectiveDamage = 0;
        if (isStatusDamage)
        {
            effectiveDamage = damageAmount;
        }
        else
        {
            effectiveDamage = Mathf.Max(damageAmount - defense, 0);
            Enemy_AI enemyAI = this.gameObject.GetComponent<Enemy_AI>();
            if (enemyAI != null)
            {
                foreach (float buff in enemyAI.defBuffs)
                {
                    float buffReduction = damageAmount * buff;
                    effectiveDamage -= Mathf.RoundToInt(buffReduction);
                }
                effectiveDamage = Mathf.Max(effectiveDamage, 0);
            }
            //this.GetComponent<Enemy_AI>()?.defBuffs.ForEach(buff => effectiveDamage -= Mathf.RoundToInt(damageAmount * buff));

            if (blocking)
            {
                effectiveDamage /= 2;
                AudioManager.Instance.PlaySFX("Parade");
            }
            if (defending)
            {
                effectiveDamage /= 2;
            }
        }
        health -= effectiveDamage;
        if(this.CompareTag("Player"))
        {
            AudioManager.Instance.PlaySFX("Slime_Damage");
        }
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

        if (health <= 0 && this.CompareTag("Player"))
        {
            Debug.Log("Player has died. Game Over.");
            player.GetComponent<PlayerScript>().GameOver();
        }
        return effectiveDamage;
    }

    public void heal(int healAmount)
    {
        health += healAmount;
        health = Mathf.Min(health, originalHealth);
        Debug.Log($"{gameObject.name} healed for {healAmount}. Current health: {health}");
        AudioManager.Instance.PlaySFX("Heal");
    }
    public void Bleed()
    {
        isBleeding = (bleedingTimer > 0);
        if(isBleeding)
        {
            Debug.Log($"{gameObject.name} is bleeding.");
            takeDamage(bleedDamage, true);
            AudioManager.Instance.PlaySFX("Bleed");
            bleedingTimer--;
            if(bleedingTimer <= 0 && bleedingInstance != null)
            {
                Destroy(bleedingInstance);
            }
        }
    }

    public void Burn()
    {
        isOnFire = (fireTimer > 0);
        if (isOnFire)
        {
            Debug.Log($"{gameObject.name} is on fire.");
            int burnDamage = Mathf.RoundToInt(originalHealth / fireDamage);
            takeDamage(burnDamage, true);
            AudioManager.Instance.PlaySFX("Burn");
            fireTimer--;
            if (fireTimer <= 0 && fireInstance != null)
            {
                Destroy(fireInstance);
            }
        }
    }

    public void Dizzyness()
    {
        isDizzy = (dizzyTimer > 0);
        if (isDizzy)
        {
            Debug.Log($"{gameObject.name} is dizzy.");
            dizzyTimer--;
            if (dizzyTimer <= 0 && dizzyInstance != null)
            {
                Destroy(dizzyInstance);
            }
        }
    }
    public void HandleAbsorptionColor()
    {
        SpriteRenderer img = GetComponentInChildren<SpriteRenderer>();
        hasAbsorption = (absorptionTimer > 0);
        if(hasAbsorption)
        {
            Debug.Log($"{gameObject.name} has absorption.");
            img.color = absorptionColor;
            absorptionTimer--;
        }
        else
        {
            img.color = originalColor;
        }
    }

    public IEnumerator ApplyStatus()
    {
        isBleeding = (bleedingTimer > 0);
        if (isBleeding)
        {
            Bleed();
            yield return new WaitForSeconds(1f);
        }
        isOnFire = (fireTimer > 0);
        if (isOnFire)
        {
            Burn();
        }
        if (isDizzy)
        {
            Dizzyness();
        }
        HandleAbsorptionColor();
    }
    public void MakeBleeding()
    {
        if(!isBleeding)
        {
            isBleeding = true;
            if(CompareTag("Player"))
            { 
                bleedingInstance = Instantiate(bloodPF, this.transform.position + new Vector3(-0.75f, 3, 0), Quaternion.identity, this.transform);
            }
            else
            {
                bleedingInstance = Instantiate(bloodPF, this.transform.position + new Vector3(0, 3, 0), Quaternion.identity, this.transform);
            }

        }
        bleedingTimer = bleedingDuration +1;
    }
    public void MakeBurned()
    {
        if (!isOnFire)
        {
            isOnFire = true;
            if(CompareTag("Player"))
            {
                fireInstance = Instantiate(firePF, this.transform.position + new Vector3(0, 3, 0), Quaternion.identity, this.transform);
            }
            else
            {
                fireInstance = Instantiate(firePF, this.transform.position + new Vector3(0.5f, 3, 0), Quaternion.identity, this.transform);
            }
        }
        fireTimer = fireDuration+1;
    }
    public void MakeDizzy()
    {
        if (!isDizzy)
        {
            isDizzy = true;
            if (CompareTag("Player"))
            {
                dizzyInstance = Instantiate(dizzyPF, this.transform.position + new Vector3(-0.75f-0.5f, 3, 0), Quaternion.identity, this.transform);
            }
            else
            {
                dizzyInstance = Instantiate(dizzyPF, this.transform.position + new Vector3(-0.5f, 3, 0), Quaternion.identity, this.transform);
            }
        }
        dizzyTimer = dizzyDuration + 1;
    }
    public void ActivateAbsorption()
    {
        if (!hasAbsorption)
        {
            hasAbsorption = true;
            SpriteRenderer img = GetComponentInChildren<SpriteRenderer>();
            img.color = absorptionColor;
            AudioManager.Instance.PlaySFX("Powerup");
        }
        absorptionTimer = absorptionDuration + 1;
    }
}
