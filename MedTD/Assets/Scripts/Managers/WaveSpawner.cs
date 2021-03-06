﻿using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class WaveSpawner : MonoBehaviour
{
    public static WaveSpawner instance; // this is a singleton class
    
    public int numberOfWaves = 10;
    public float timeBetweenWaves = 8f; // todo: could be different for each wave
    public float moneyForEarlyStart = 20f;
    public Transform enemyFolder;
    //public Transform spawnPoint;
    public Transform[] spawnPoints;
    //public Transform pathBoard;
    public Transform enemyPrefab1;
    public Transform enemyPrefab2;
    //public Transform enemyPrefab3; // todo: etc
    public Text waveNumberText;
    public Text waveCountdownText;

    //private Transform[] pathTiles;
    private float countdown = 0f;
    private int waveNumber = 1;
    private float earlyStartTime;
    private bool levelStarted = false;
    private bool allWavesFinishedSpawning = false;

    private void Awake()
    {
        // initialize an instance of this singleton for use in other classes
        if (instance != null)
        {
            Debug.Log("More than one WaveSpawner in scene!");
            Destroy(gameObject);
            return;
        }
        else
        {
            instance = this;
        }

        //countdown = 1f;
        countdown = timeBetweenWaves/3;

        //pathTiles = new Transform[pathBoard.childCount];
        //for (int i = 0; i < pathTiles.Length; i++)
        //{
        //    pathTiles[i] = pathBoard.GetChild(i);
        //}
    }

    private void Start()
    {
        earlyStartTime = timeBetweenWaves / 2;
    }

    internal void StartLevel()
    {
        levelStarted = true;
        allWavesFinishedSpawning = false;
    }

    internal bool IsLevelStarted()
    {
        return levelStarted;
    }
    internal bool IsFinishedSpawning()
    {
        return allWavesFinishedSpawning;
    }

    private void Update()
    {
        if (!levelStarted || allWavesFinishedSpawning) return;

        // if this is the last wave
        if (waveNumber > numberOfWaves)
        {
            allWavesFinishedSpawning = true;
            //countdown = timeBetweenWaves;
            waveCountdownText.text = "No more waves.";
        }

        // if time to next wave is less than half, enable "start wave" button; if not, disable it
        UIManager.instance.SetEnabledButtonBottomCenterStartWave(countdown <= (earlyStartTime), "Start wave early");

        if (countdown <= 0f && !Shop.instance.IsCoughing())
        {
            //StartCoroutine(SpawnWave());
            //countdown = timeBetweenWaves;
            NextWave();
        }
        
        // while there are still more waves coming, update the text showing time until next wave
        if (waveNumber <= numberOfWaves)
        {
            if (countdown > 0f)
            {
                countdown -= Time.deltaTime;
                waveCountdownText.text = "Next wave in " + Mathf.Floor(countdown + 1).ToString() + " seconds";
            }
        }
    }

    internal void NextWave()
    {
        StartCoroutine(SpawnWave());
        countdown = timeBetweenWaves;
    }
    internal void EarlyNextWave()
    {
        // give the player money for starting the wave early: the earlier, the more money
        float recMoney = moneyForEarlyStart * (countdown / earlyStartTime);
        Player.AddMoney(recMoney);

        NextWave();
    }

    private IEnumerator SpawnWave()
    {
        UIManager.instance.SetEnabledButtonBottomCenter(false);

        //waveIndex++;

        // update UI element showing the current level number
        waveNumberText.text = "Wave: " + waveNumber + "/" + numberOfWaves;

        // todo: this is just for testing, should be predefined
        //if (waveIndex == 1)
        //{
        //    SpawnEnemy(enemyPrefab1);
        //    //yield return new WaitForSeconds(0.5f);
        //    //SpawnEnemy(enemyPrefab2);
        //}
        //else if (waveIndex == 2)
        //{
        //    SpawnEnemy(enemyPrefab1);
        //    //yield return new WaitForSeconds(0.6f);
        //    //SpawnEnemy(enemyPrefab1);
        //    //yield return new WaitForSeconds(0.7f);
        //    //SpawnEnemy(enemyPrefab2);
        //}
        //else
        //{
            // generate N enemies, where N is the number of the current wave
            for (int i = 0; i < waveNumber; i++)
            //for (int i = 0; i < 1; i++)
            {
                SpawnEnemy(null);
                //SpawnEnemy(enemyPrefab1);
                yield return new WaitForSeconds(0.6f);
            }
        //}

        waveNumber++;
    }

    private void SpawnEnemy(Transform enemyPrefab)
    {
        //Transform enemyPrefab = null;
        System.Random random = new System.Random();

        if (enemyPrefab == null)
        {
            // todo: number of enemies...
            int randomInt = random.Next(1, 3); // generates a number between 1 and number of enemies
            switch (randomInt)
            {
                case 1:
                default:
                    enemyPrefab = enemyPrefab1;
                    break;
                case 2:
                    enemyPrefab = enemyPrefab2;
                    break;
                    //case 3:
                    //    enemyPrefab = enemyPrefab3;
                    //    break;
            }
        }

        // in case only enemyPrefab1 is set and the others are not
        if (enemyPrefab == null) enemyPrefab = enemyPrefab1;

        //if (enemyPrefab != null && pathBoard.childCount > 0)
        if (enemyPrefab != null)
        {
            //int randomTileIndex = random.Next(0, pathBoard.childCount);
            //Transform spawnPoint = pathBoard.GetChild(randomTileIndex);
            if (spawnPoints.Length > 0)
            {
                int randomSpawnPointIndex = random.Next(0, spawnPoints.Length);
                Transform startTile = spawnPoints[randomSpawnPointIndex];
                Transform spawnPoint = startTile.GetChild(0); // the spawn point must be a tile game object with a spawnpoint gameobject
                //if (spawnPoint == null) spawnPoint = startTile;
                Transform enemyTr = Instantiate(enemyPrefab, spawnPoint.position, spawnPoint.rotation);
                enemyTr.SetParent(enemyFolder);
                enemyTr.GetComponent<Enemy>().SetStartTile(startTile, 0f);
            }
        }
    }

    //private void SpawnEnemy()
    //{
    //    Transform enemyTr = Instantiate(enemyPrefab1, spawnPoint.position, spawnPoint.rotation);
    //    enemyTr.SetParent(enemyFolder);
    //}
}
