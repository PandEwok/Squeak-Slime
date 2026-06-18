using UnityEngine;

public class PlayerVfx : MonoBehaviour
{
    [Header("VFX Prefabs")]
    [SerializeField] private GameObject attackBoostEffect;
    [SerializeField] private GameObject defenseBoostEffect;
    protected float particleSpawnTimer = 0;
    protected float particleSpawnDuration = 0.1f;
    protected int particleQtt = 4;
    protected float particleRandomXRange = 0.6f;
    protected float particleRandomYRange = 0.25f;
    protected float particleVerticalOffset = -0.2f;

    private void Update()
    {
        if (Player.Instance.IsInBattle != false)
        {
            HandleParticles();
        }
    }
    public void HandleParticles()
    {
        particleSpawnTimer += Time.deltaTime;
        HandleEmpoweredParticles();
        HandleDefenseBuffedParticles();
    }
    private void HandleEmpoweredParticles()
    {
        Player.Instance.stats.empowered = (Player.Instance.stats.empowerDelay > 0);
        if (Player.Instance.stats.empowered && particleSpawnTimer > particleSpawnDuration)
        {
            particleSpawnTimer = 0;
            for (int i = 0; i < particleQtt; i++)
            {
                float randomX = Random.Range(-particleRandomXRange, particleRandomXRange);
                float randomY = Random.Range(-particleRandomYRange, particleRandomYRange);
                Instantiate(attackBoostEffect, Player.Instance.transform.position + new Vector3(randomX, -particleVerticalOffset + randomY, 0), Quaternion.identity, Player.Instance.transform);
            }
        }
    }
    private void HandleDefenseBuffedParticles()
    {
        Player.Instance.stats.defenseBuffed = (Player.Instance.stats.defenseBuffDelay > 0);
        if (Player.Instance.stats.defenseBuffed && particleSpawnTimer > particleSpawnDuration)
        {
            particleSpawnTimer = 0;
            for (int i = 0; i < particleQtt; i++)
            {
                float randomX = Random.Range(-particleRandomXRange, particleRandomXRange);
                float randomY = Random.Range(-particleRandomYRange, particleRandomYRange);
                Instantiate(defenseBoostEffect, Player.Instance.transform.position + new Vector3(randomX, -particleVerticalOffset + randomY, 0), Quaternion.identity, Player.Instance.transform);
            }
        }
    } 
}
