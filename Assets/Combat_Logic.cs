using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Random = UnityEngine.Random;

public class Combat_Logic : MonoBehaviour
{
    [SerializeField] private GameObject actionUI;
    bool playerTurn = true;
    bool switchingTurns = false;

    /*public Stats_System playerStats;*/
    public List<GameObject> enemiesToSpawn;
    public List<GameObject> EnemyPositions;
    public GameObject PlayerPosition;

    public List<GameObject> enemies = new List<GameObject>();
    private List<GameObject> enemiesToDestroy = new List<GameObject>();

    public GameObject player;
    private int playerTurnCount = 0;
    //UnityEngine.UI.Button[] UI_Buttons;

    private void Start()
    {

        Player.Instance.LoadPlayer(PlayerPosition.transform.position);
        actionUI = Player.Instance.uiManager.actionMenu;
    }


    public void removeEnemy(GameObject enemy)
    {
        int index = enemies.IndexOf(enemy);

        if (playerTurn)
        {
            if (index != -1)
            {
                EnemyPositions.RemoveAt(index);
                enemies.RemoveAt(index);
            }
            Destroy(enemy);
        }
        else {
            enemy.SetActive(false);
            enemiesToDestroy.Add(enemy);
        }

        if (enemies.Count == 0)
        {
            Debug.Log("All enemies defeated! Victory!");
            EngameUIScript.Instance.Victory();
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

            foreach (GameObject enemy in enemiesToDestroy)
            {
                enemiesToDestroy.Remove(enemy);
                removeEnemy(enemy);
            }
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        player = GameObject.FindWithTag("Player");

        for (int i = 0; i < enemiesToSpawn.Count; i++)
        {
            enemies.Add( Instantiate(enemiesToSpawn[i], EnemyPositions[i].transform.position, Quaternion.identity, this.transform) );
        }
        // UI_Buttons = GameObject.Find("PAction_Bar").GetComponentsInChildren<UnityEngine.UI.Button>();
    }

    // Update is called once per frame
    void Update()
    {
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
