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
    [SerializeField] private int centeringOffset1;
    [SerializeField] private int centeringOffset2;
    [SerializeField] private EnemyAI[] activeEnemies;
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
        activeEnemies = new EnemyAI[10];
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
        enemyHealth.enemyManager = this;
        enemyAttack.projectilesContainer = projectileContainer;
        if (area == 3 || area == 5 || area == 7 || area == 9)
            enemyAI.centeringOffset = centeringOffset1;
        else if(area == 4 || area == 6 || area == 8 || area == 10 || area == 11 || area == 12)
            enemyAI.centeringOffset = centeringOffset2;
        activeEnemies[area-3] = enemyAI;
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

    public IEnumerator DeadEnemy(int area)
    {
        yield return new WaitForSeconds(timeToRespawn / 2);
        SwitchAreas(area);
        yield return new WaitForSeconds(timeToRespawn / 2);
        Transform spawn = spawns[0];
        if (area == 7 || area == 8 || area == 9 || area == 10 || area == 11)
            spawn = spawns[1];
        SpawnEnemy(spawn, area);
        Debug.Log("Enemigo spawneado");
    }

    public void SwitchAreas(int area)
    {
        switch (area){
            case 4:
                Debug.Log("area cambiada");
                activeEnemies[0].enemyArea = 4;
                activeEnemies[0].waypoints.Clear();
                activeEnemies[0].waypoints = new List<Transform>(wayPoints[area - 3]);
                activeEnemies[0].ChangeArea();
                break;
            
        }
    }
}
