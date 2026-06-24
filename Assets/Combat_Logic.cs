using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Random = UnityEngine.Random;

public class Combat_Logic : MonoBehaviour
{
    [Header("Audio Settings")]
    [Tooltip("The exact name of your main menu track in the AudioManager database.")]
    public string firstBiomeMusic = "ForestBattle";
    public string secondBiomeMusic = "CastleBattle";
    public string thirdBiomeMusic = "LabBattle";
    public string bossMusic = "BossBattle";

    public string musicTrackName = "ForestBattle";

    [SerializeField] private GameObject actionUI;
    bool playerTurn = true;
    bool switchingTurns = false;

    /*public Stats_System playerStats;*/
    public List<GameObject> enemiesToSpawn;
    public List<GameObject> enemyPositions;
    public List<GameObject> enemyBiomeList;
    public List<int> enemySpawnChance;
    public GameObject PlayerPosition;

    public List<GameObject> enemies = new List<GameObject>();
    private List<GameObject> enemiesToDestroy = new List<GameObject>();

    public GameObject boss;
    public GameObject bossPosition;

    public GameObject player;
    private int playerTurnCount = 0;
    private float entranceTimer = 0;
    bool enemiesHaveEntered = false;

    //UnityEngine.UI.Button[] UI_Buttons;
    bool displayFireDebuger = false;

    private IEnumerator EnemyEntrance()
    {
        float offsetX = 13f; // Distance from which enemies will enter
        for (int i = 0; i < enemies.Count; i++)
        {
            GameObject enemy = enemies[i];
            Vector3 startPos = enemyPositions[i].transform.position + new Vector3(offsetX, 0, 0);
            enemy.transform.position = startPos;
        }
        for (int i = 0; i < enemies.Count; i++)
        {
            GameObject enemy = enemies[i];
            Vector3 targetPos = enemyPositions[i].transform.position;
            Vector3 startPos = targetPos + new Vector3(offsetX, 0, 0);
            enemy.transform.position = startPos;
            entranceTimer = 0f;

            while (enemy.transform.position != targetPos) {
                entranceTimer += Time.deltaTime;
                float t = entranceTimer / 0.4f;
                enemy.transform.position = Vector3.Lerp(startPos, targetPos, t);
                yield return null;
            }
        }
    }

    private void Start()
    {

        Player.Instance.LoadPlayer(PlayerPosition.transform.position);
        actionUI = Player.Instance.uiManager.actionMenu;
        switch(Player.Instance.currentBiome)
        {
            case Player.BiomeType.FOREST:
                musicTrackName = firstBiomeMusic;
                break;
            case Player.BiomeType.CASTLE_EXTERIOR:
                musicTrackName = secondBiomeMusic;
                break;
            case Player.BiomeType.RAT_LABORATORY:
                musicTrackName = thirdBiomeMusic;
                break;
            //ajouter manquant boss
            default:
                break;
        }
        if (AudioManager.Instance != null && !string.IsNullOrEmpty(musicTrackName))
        {
            AudioManager.Instance.PlayMusic(musicTrackName);
        }
    }


    public void removeEnemy(GameObject enemy)
    {
        int index = enemies.IndexOf(enemy);

        if (playerTurn)
        {
            if (index != -1)
            {
                enemyPositions.RemoveAt(index);
                enemies.RemoveAt(index);
            }
            Destroy(enemy);
        }
        else {
            enemy.SetActive(false);
            if (!enemiesToDestroy.Contains(enemy))
            {
                enemiesToDestroy.Add(enemy);
            }
        }

        if (enemies.Count <= 0)
        {
            Debug.Log("All enemies defeated! Victory!");
            EndgameUIScript.Instance.Victory();
        }
    }

    public void switchTurn()
    {
        Debug.Log("Switching turns...");
        switchingTurns = true;
        StartCoroutine(SwitchTurnCoroutine());
    }

    public void playerAttack()
    {
        if (enemies.Count > 0)
        {
            GameObject targetEnemy = enemies[0];
            Stats_System enemyStats = targetEnemy.GetComponent<Stats_System>();
            if (enemyStats != null)
            {
                int damageAmount = 20; // Example damage value
                int randomDmgOffset = Random.Range(-2, 3);
                damageAmount += randomDmgOffset;
                enemyStats.TakeDamage(damageAmount, false);
                Debug.Log($"Player attacked {targetEnemy.name} for {damageAmount} damage.");
            }
        }
        switchTurn();
    }

    private IEnumerator SwitchTurnCoroutine()
    {
        yield return new WaitForSeconds(1f);
        playerTurn = !playerTurn;
        switchingTurns = false;
        if (playerTurn)
        {
            actionUI.GetComponent<ActionBarScript>().FinalizeAttack();
            playerTurnCount++;
            Debug.Log($"Nombre de tours joueur: {playerTurnCount}");
            if (playerTurnCount == 2)
            { 
                Player.Instance.stats.HandleHealingEveryTwoTurn(); 
                playerTurnCount = 0;
            }

            for (int i = enemiesToDestroy.Count - 1; i >= 0; i--)
            {
                GameObject enemy = enemiesToDestroy[i];
                removeEnemy(enemy);
                enemiesToDestroy.RemoveAt(i);

                displayFireDebuger = true;
            }
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        player = GameObject.FindWithTag("Player");

        if (enemiesToSpawn.Count < enemyPositions.Count)
        {
            if (enemyBiomeList.Count > 0)
            {
                enemiesToSpawn = new List<GameObject>();
                for (int i = 0; i < enemyPositions.Count; i++)
                {
                    if (boss != null && enemyPositions[i] == bossPosition)
                    {
                        enemiesToSpawn.Add(boss);
                    }
                    else
                    {
                        int randomIndex = Random.Range(0, 100);
                        for (int j = 0; j < enemySpawnChance.Count; j++)
                        {
                            int chances = enemySpawnChance[j];
                            for (int k = j - 1; k >= 0; k--)
                            {
                                chances += enemySpawnChance[k];
                            }
                            if (randomIndex < chances)
                            {
                                randomIndex = j;
                                break;
                            }
                        }
                        GameObject randomEnemyPrefab = enemyBiomeList[randomIndex];
                        enemiesToSpawn.Add(randomEnemyPrefab);
                    }
                }
            }
        }

        for (int i = 0; i < enemiesToSpawn.Count; i++)
        {
            enemies.Add( Instantiate(enemiesToSpawn[i], enemyPositions[i].transform.position, Quaternion.identity, this.transform) );
        }
        // UI_Buttons = GameObject.Find("PAction_Bar").GetComponentsInChildren<UnityEngine.UI.Button>();
    }

    // Update is called once per frame
    void Update()
    {
        //entranceTimer += Time.deltaTime;

        if (!enemiesHaveEntered)
        {
            StartCoroutine(EnemyEntrance());
            enemiesHaveEntered = true;
        }

        if (playerTurn && !switchingTurns)
        {
            /*Debug.Log("Player's turn");*/

            // Wait for player input to attack or use an ability, then call playerAttack() or similar methods
            // switchTurn() will be called at the end of the player's action to switch to the enemy's turn
        }
        else if (!playerTurn && !switchingTurns) {
            /* Enemy turn */

            switchingTurns = true;
            EnemyTurnSequence();
        }
        if (displayFireDebuger)
        {
            displayFireDebuger = false;
            Debug.Log($"Nombre d'ennemis en vie: {enemies.Count}");
        }
        }

    public IEnumerator waitDelay(float seconds)
    {
        yield return new WaitForSeconds(seconds);
    }

    public async void EnemyTurnSequence()
    {
        foreach (GameObject enemy in enemies)
        {
            Enemy_AI enemyAI = enemy.GetComponent<Enemy_AI>();
            if (enemyAI != null)
            {
                await enemyAI.playTurn(player); // ATTENDRE la fin du tour de cet ennemi
                if(player.GetComponent<Stats_System>().health <= 0)
                {
                    Debug.Log("Player has been defeated!");
                    //EngameUIScript.Instance.GameOver();
                    return;
                }
                if (enemy != enemies[enemies.Count - 1])
                {
                    await Task.Delay((int)(1f * 1000));
                }
            }
        }

        switchTurn();
    }

}
