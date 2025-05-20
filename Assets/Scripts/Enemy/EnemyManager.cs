using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EnemyManager : MonoBehaviour
{
    [SerializeField] private Transform[] spawns;
    [SerializeField] private Transform[] area1Waypoints;
    [SerializeField] private Transform[] area2Waypoints;
    [SerializeField] private Transform[] area3Waypoints;
    [SerializeField] private Transform[] area4Waypoints;
    [SerializeField] private Transform[] area5Waypoints;
    [SerializeField] private Transform[] area6Waypoints;
    [SerializeField] private Transform[] area7Waypoints;
    [SerializeField] private Transform[] area8Waypoints;
    [SerializeField] private Transform[] area9Waypoints;
    [SerializeField] private Transform[] area10Waypoints;
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private Transform player;
    [SerializeField] private Transform projectileContainer;
    [SerializeField] private float timeToRespawn;
    [SerializeField] private float timeBetweenSpawns;
    private Transform[][] wayPoints;

    private void Start()
    {
        wayPoints = new Transform[][]
        {
            area1Waypoints, area2Waypoints, 
            area3Waypoints, area4Waypoints, 
            area5Waypoints, area6Waypoints, 
            area7Waypoints, area8Waypoints, 
            area9Waypoints, area10Waypoints
        };
        StartCoroutine(SpawnAllEnemies());
        //SpawnEnemy(spawns[0], 12);
    }

    
    private void SpawnEnemy(Transform spawn, int area)
    {
        GameObject enemy = Instantiate(enemyPrefab, spawn.position, spawn.rotation);
        EnemyAI enemyAI = enemy.GetComponent<EnemyAI>();
        EnemyHealth enemyHealth = enemy.GetComponent<EnemyHealth>();
        EnemyAttack enemyAttack = enemy.GetComponent<EnemyAttack>();
        enemyAI.player = player;       
        enemyAI.enemyArea = area;
        enemyAI.waypoints = new List<Transform>(wayPoints[area - 3]); //Las areas de enemigos inician en el indice 3
        enemyHealth.player = player;
        enemyAttack.projectilesContainer = projectileContainer;
    }

    private IEnumerator SpawnAllEnemies()
    {
        SpawnEnemy(spawns[0], 12);
        SpawnEnemy(spawns[1], 11);
        for (int i = 4; i > 0; i--)
        {
            yield return new WaitForSeconds(timeBetweenSpawns);
            SpawnEnemy(spawns[0], i + 2);
            SpawnEnemy(spawns[1], i + 6);           
        }
    }
}
