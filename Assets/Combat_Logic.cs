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
        yield return new WaitForSeconds(2f);
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
        }
        else if (!switchingTurns) {
            /*Debug.Log("Enemy's turn");*/

            

            // All enemies attack player
            foreach (GameObject enemy in enemies)
            {
                Enemy_AI enemyAI = enemy.GetComponent<Enemy_AI>();
                if (enemyAI != null)
                {
                    enemyAI.playTurn(enemy);
                }
            }

            switchTurn();
        }
    }
}
