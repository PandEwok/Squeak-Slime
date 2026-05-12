using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class Combat_Logic : MonoBehaviour
{
    bool playerTurn = true;
    bool switchingTurns = false;

    /*public Stats_System playerStats;*/
    public List<GameObject> enemiesToSpawn;
    public List<GameObject> EnemyPositions;

    List<GameObject> enemies = new List<GameObject>();

    GameObject player;

    UnityEngine.UI.Button[] UI_Buttons;

    public void switchTurn()
    {
        Debug.Log("Switching turns...");
        switchingTurns = true;
        StartCoroutine(SwitchTurnCoroutine());
    }

    public void playerAttack()
    {
        foreach (UnityEngine.UI.Button button in UI_Buttons)
        {
            button.interactable = false;
        }
        if (enemies.Count > 0)
        {
            GameObject targetEnemy = enemies[0];
            Stats_System enemyStats = targetEnemy.GetComponent<Stats_System>();
            if (enemyStats != null)
            {
                int damageAmount = 20; // Example damage value
                int randomDmgOffset = Random.Range(-2, 3);
                damageAmount += randomDmgOffset;
                enemyStats.takeDamage(damageAmount);
                Debug.Log($"Player attacked {targetEnemy.name} for {damageAmount} damage.");
            }
        }
        switchTurn();
    }

    private IEnumerator SwitchTurnCoroutine()
    {
        yield return new WaitForSeconds(1.5f);
        playerTurn = !playerTurn;
        switchingTurns = false;
        if (playerTurn)
        {
            foreach (UnityEngine.UI.Button button in UI_Buttons)
            {
                button.interactable = true;
            }
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        player = GameObject.FindWithTag("Player");

        for (int i = 0; i < enemiesToSpawn.Count; i++)
        {
            enemies.Add( Instantiate(enemiesToSpawn[i], EnemyPositions[i].transform.position, Quaternion.identity, this.transform) );
        }
        UI_Buttons = GameObject.Find("PAction_Bar").GetComponentsInChildren<UnityEngine.UI.Button>();
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
            /*Debug.Log("Enemy's turn");*/

            switchingTurns = true;
            EnemyTurnSequence();
        }
    }

    public async void EnemyTurnSequence()
    {
        foreach (GameObject enemy in enemies)
        {
            Enemy_AI enemyAI = enemy.GetComponent<Enemy_AI>();
            if (enemyAI != null)
            {
                await enemyAI.playTurn(player); // ATTENDRE la fin du tour de cet ennemi
            }
        }

        switchTurn();
    }

}
