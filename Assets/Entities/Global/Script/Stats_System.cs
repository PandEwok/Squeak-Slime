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
    [Header("Prefabs")]
    public GameObject damagePF;
    public GameObject bloodPF;
    public GameObject firePF;
    public GameObject dizzyPF;
    [Header("Base Stats")]
    public int health;
    public int originalHealth;
    public int damage;
    public int defense;
    [Header("Status Booleans")]
    public bool isBleeding = false;
    public bool isOnFire = false;
    public bool isDizzy = false;
    public bool isAbsorbing = false;
    [Header("Status Values")]
    public int bleedDamage = 5;
    public float fireDamage = 18; //Utilise pour calculer une proportion, n'inflige pas 18
    [Header("Status Durations")]
    public int bleedingDuration = 3;
    public int fireDuration = 3;
    public int absorptionDuration = 3;
    public int dizzyDuration = 1;
    //StatusTimers
    protected int bleedingTimer = 0;
    protected int fireTimer = 0;
    protected int absorptionTimer = 0;
    protected int dizzyTimer = 0;
    //StatusIconsReferences
    protected GameObject bleedingInstance;
    protected GameObject fireInstance;
    protected GameObject dizzyInstance;
    protected GameObject player;
    Vector3 originalPos;
    Color originalColor;
    [HideInInspector] public Color absorptionColor;

    protected virtual void Start()
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

    protected IEnumerator DmgShake()
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

    protected IEnumerator DmgShade()
    {
        SpriteRenderer img = GetComponentInChildren<SpriteRenderer>();
        
        Color targetColor = new Color(1f, 0f, 0f, 0.8f);
        float duration = 0.15f;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            if(!isAbsorbing) img.color = Color.Lerp(originalColor, targetColor, elapsed / duration);
            else img.color = Color.Lerp(absorptionColor, targetColor, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        elapsed = 0f;
        while (elapsed < duration)
        {
            if(!isAbsorbing) img.color = Color.Lerp(targetColor, originalColor, elapsed / duration);
            else img.color = Color.Lerp(targetColor, absorptionColor, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        if (!isAbsorbing) img.color = originalColor;
        else img.color = absorptionColor;
    }

    public virtual int TakeDamage(int damageAmount, bool isStatusDamage)
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
        }
        health -= effectiveDamage;
        GameObject newDmgDisplay;

        Vector3 spawnPos = new Vector3(this.transform.position.x, this.transform.position.y + 2);
        float randomXOffset = UnityEngine.Random.Range(-0.5f, 0.5f);
        spawnPos.x += randomXOffset;
        float randomYOffset = UnityEngine.Random.Range(-0.5f, 0.5f);
        spawnPos.y += randomYOffset;


        StartCoroutine(DmgShake());
        StartCoroutine(DmgShade());

        newDmgDisplay = Instantiate(damagePF, spawnPos, Quaternion.identity, GameObject.FindGameObjectWithTag("Canvas").transform);
        newDmgDisplay.GetComponent<TextMeshProUGUI>().SetText(effectiveDamage.ToString());

        Debug.Log($"{gameObject.name} took {effectiveDamage} damage. Remaining health: {health}");

        return effectiveDamage;
    }

    public void Heal(int healAmount)
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
            TakeDamage(bleedDamage, true);
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
            TakeDamage(burnDamage, true);
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
                isDizzy = false;
            }
        }
    }
    public void HandleAbsorptionColor()
    {
        SpriteRenderer img = GetComponentInChildren<SpriteRenderer>();
        isAbsorbing = (absorptionTimer > 0);
        if(isAbsorbing)
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
        isDizzy = (dizzyTimer > 0);
        {
            Dizzyness();
        }
        HandleAbsorptionColor();
    }
    public virtual void MakeBleeding()
    {
        if(!isBleeding)
        {
            isBleeding = true;
            bleedingInstance = Instantiate(bloodPF, this.transform.position + new Vector3(0, 3, 0), Quaternion.identity, this.transform);
        }
        bleedingTimer = bleedingDuration +1;
    }
    public virtual void MakeBurned()
    {
        if (!isOnFire)
        {
            isOnFire = true;
            fireInstance = Instantiate(firePF, this.transform.position + new Vector3(0.5f, 3, 0), Quaternion.identity, this.transform);
        }
        fireTimer = fireDuration+1;
    }
    public virtual void MakeDizzy()
    {
        if (!isDizzy)
        {
            isDizzy = true;
            dizzyInstance = Instantiate(dizzyPF, this.transform.position + new Vector3(-0.5f, 3, 0), Quaternion.identity, this.transform);   
        }
        dizzyTimer = dizzyDuration;
    }
    public void ActivateAbsorption()
    {
        if (!isAbsorbing)
        {
            isAbsorbing = true;
            SpriteRenderer img = GetComponentInChildren<SpriteRenderer>();
            img.color = absorptionColor;
            AudioManager.Instance.PlaySFX("Powerup");
        }
        absorptionTimer = absorptionDuration + 1;
    }
}
