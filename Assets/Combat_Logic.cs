using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class Combat_Logic : MonoBehaviour
{
    bool playerTurn = true;

    /*public Stats_System playerStats;*/
    public List<GameObject> enemiesToSpawn;
    public List<GameObject> EnemyPositions;

    List<GameObject> enemies = new List<GameObject>();

    public void switchTurn()
    {
        Debug.Log("Switching turns...");
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
                enemyStats.TakeDamage(damageAmount);
                Debug.Log($"Player attacked {targetEnemy.name} for {damageAmount} damage.");
            }
        }
        switchTurn();
    }

    private IEnumerator SwitchTurnCoroutine()
    {
        yield return new WaitForSeconds(2f);
        playerTurn = !playerTurn;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        for (int i = 0; i < enemiesToSpawn.Count; i++)
        {
            enemies.Add( Instantiate(enemiesToSpawn[i], EnemyPositions[i].transform.position, Quaternion.identity, this.transform) );
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (playerTurn)
        {
            /*Debug.Log("Player's turn");*/
        }
        else {
            /*Debug.Log("Enemy's turn");*/

            // All enemies attack player
            foreach (GameObject enemy in enemies)
            {
                Stats_System enemyStats = enemy.GetComponent<Stats_System>();
                if (enemyStats != null)
                {
                    int damageAmount = enemyStats.damage;
                    int randomDmgOffset = Random.Range(-2, 3);
                    damageAmount += randomDmgOffset;
                    // Assuming playerStats is defined and accessible
                    // playerStats.TakeDamage(damageAmount);
                    Debug.Log($"Enemy {enemy.name} attacked player for {damageAmount} damage.");
                }
            }

            switchTurn();
        }
    }
}
