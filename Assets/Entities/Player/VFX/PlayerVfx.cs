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
        if (Player.Instance.hasWon == false)
        {
            HandleParticles(Player.Instance);
        }
    }
    public void HandleParticles(Player player)
    {
        particleSpawnTimer += Time.deltaTime;
        HandleEmpoweredParticles(player);
        HandleDefenseBuffedParticles(player);
    }
    private void HandleEmpoweredParticles(Player player)
    {
        var playerStats = player.stats;
        playerStats.empowered = (playerStats.empowerDelay > 0);
        if (playerStats.empowered && particleSpawnTimer > particleSpawnDuration)
        {
            particleSpawnTimer = 0;
            for (int i = 0; i < particleQtt; i++)
            {
                float randomX = Random.Range(-particleRandomXRange, particleRandomXRange);
                float randomY = Random.Range(-particleRandomYRange, particleRandomYRange);
                Instantiate(attackBoostEffect, player.transform.position + new Vector3(randomX, -particleVerticalOffset + randomY, 0), Quaternion.identity, player.transform);
            }
        }
    }
    private void HandleDefenseBuffedParticles(Player player)
    {
        var playerStats = player.stats;
        playerStats.defenseBuffed = (playerStats.defenseBuffDelay > 0);
        if (playerStats.defenseBuffed && particleSpawnTimer > particleSpawnDuration)
        {
            particleSpawnTimer = 0;
            for (int i = 0; i < particleQtt; i++)
            {
                float randomX = Random.Range(-particleRandomXRange, particleRandomXRange);
                float randomY = Random.Range(-particleRandomYRange, particleRandomYRange);
                Instantiate(defenseBoostEffect, player.transform.position + new Vector3(randomX, -particleVerticalOffset + randomY, 0), Quaternion.identity, player.transform);
            }
        }
    } 
}
