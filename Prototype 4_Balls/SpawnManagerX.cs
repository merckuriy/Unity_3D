using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnManagerX : MonoBehaviour
{
    public GameObject enemyPrefab;
    public GameObject powerupPrefab;

    private float spawnRangeX = 10;
    private float spawnZMin = 20; // set min spawn Z
    private float spawnZMax = 27; // set max spawn Z
    [HideInInspector]
    public int goals;

    public int enemyCount;
    public int waveCount = 1;
    public float increaseEnemiesSpeed = 40.0f;
    public float currentEnemiesSpeed = 40.0f;
    public int maxEnemies = 3;

    public GameObject player; 

    // Update is called once per frame
    void Update()
    {
        enemyCount = GameObject.FindGameObjectsWithTag("Enemy").Length;

        if (enemyCount == 0)
        {
            // increase the complexity in case of all enemies were scored the goal to their gate.
            if (goals == waveCount){
                if(waveCount == maxEnemies) {
                    waveCount = 1;
                    currentEnemiesSpeed += 40.0f;
                } else {
                    waveCount++;
                }
                Debug.Log("waveCount:" + waveCount); 
            }

            SpawnEnemyWave(waveCount);
            goals = 0;
        }

    }

    // Generate random spawn position for powerups and enemy balls
    Vector3 GenerateSpawnPosition ()
    {
        float xPos = Random.Range(-spawnRangeX, spawnRangeX);
        float zPos = Random.Range(spawnZMin, spawnZMax);
        return new Vector3(xPos, 0, zPos);
    }


    void SpawnEnemyWave(int enemiesToSpawn)
    {
        Vector3 powerupSpawnOffset = new Vector3(0, 0, -spawnZMin); // make powerups spawn at player end

        // If no powerups remain, spawn a powerup
        if (GameObject.FindGameObjectsWithTag("Powerup").Length == 0) // check that there are zero powerups
        {
            Instantiate(powerupPrefab, GenerateSpawnPosition() + powerupSpawnOffset, powerupPrefab.transform.rotation);
        }

        // Spawn number of enemy balls based on wave number
        for (int i = 0; i < enemiesToSpawn; i++)
        {
            Instantiate(enemyPrefab, GenerateSpawnPosition(), enemyPrefab.transform.rotation).GetComponent<EnemyX>().speed = currentEnemiesSpeed;
        }

        ResetPlayerPosition(); // put player back at start
    }

    // Move player back to position in front of own goal
    void ResetPlayerPosition ()
    {
        player.transform.position = new Vector3(0, 1, -7);
        player.GetComponent<Rigidbody>().velocity = Vector3.zero;
        player.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;

    }

}
