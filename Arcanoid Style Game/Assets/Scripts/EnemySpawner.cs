//using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour {

    public GameObject enemy;
    private LevelManager levelManager;
    public int enemyMax = 3;
    private float timeInterval = 2f;

    //[HideInInspector]
    public bool spawning = false;

    Vector2[] spawnPoints = new Vector2[] {
    new Vector2(0.5f, 14.25f),
    new Vector2(1.5f, 14.25f),
    new Vector2(2.5f, 14.25f),
    new Vector2(3.5f, 14.25f),
    new Vector2(4.5f, 14.25f),
    new Vector2(5.5f, 14.25f),
    new Vector2(6.5f, 14.25f),
    new Vector2(7.5f, 14.25f),
    new Vector2(8.5f, 14.25f),
    };

    void Start()
    {
        levelManager = GameObject.FindObjectOfType<LevelManager>();
    }

    // Update is called once per frame
    void Update ()
    {
		/*if (levelManager.levelStart == true && spawn == true)
        {
            int randomInt = Random.Range(0, spawnPoints.Length - 1);
            GameObject.Instantiate(enemy, spawnPoints[randomInt], transform.rotation);

            spawn = false;
        }*/
       
	}

    
    public IEnumerator SpawnEnemies()
    {
        spawning = true;

        do
        {
            yield return new WaitForSeconds(timeInterval);

            if (FindObjectsOfType<Enemy>().Length < enemyMax)
            {
                int randomInt = Random.Range(0, spawnPoints.Length - 1);
                GameObject.Instantiate(enemy, spawnPoints[randomInt], transform.rotation);
            }

        } while (FindObjectsOfType<Enemy>().Length < enemyMax);

        spawning = false;
        //Debug.Log("Finished Spawning");
    }
}
