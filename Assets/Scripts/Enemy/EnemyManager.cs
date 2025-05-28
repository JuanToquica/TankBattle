using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EnemyManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private Transform player;
    [SerializeField] private Transform projectileContainer;

    [Header("Spawns and Waypoints")]
    [SerializeField] private Transform[] spawns;
    [SerializeField] private Transform[] area3Waypoints;
    [SerializeField] private Transform[] area4Waypoints;
    [SerializeField] private Transform[] area5Waypoints;
    [SerializeField] private Transform[] area6Waypoints;
    [SerializeField] private Transform[] area7Waypoints;
    [SerializeField] private Transform[] area8Waypoints;
    [SerializeField] private Transform[] area9Waypoints;
    [SerializeField] private Transform[] area10Waypoints;
    [SerializeField] private Transform[] area11Waypoints;
    [SerializeField] private Transform[] area12Waypoints;

    [Header("Time and Offset")]
    [SerializeField] private float timeToRespawn;
    [SerializeField] private float timeBetweenSpawns;
    [SerializeField] private int centeringOffset1;
    [SerializeField] private int centeringOffset2;

    public Dictionary<int, EnemyAI> activeEnemiesByArea;
    public Queue<int> spawnQueue_Spawn0 = new Queue<int>(); //Areas 3,4,5,6,12
    public Queue<int> spawnQueue_Spawn1 = new Queue<int>(); //Areas 7,8,9,10,11
    public bool spawn0Available;
    public bool spawn1Available;
    private Dictionary<int, Transform[]> wayPointsByArea;

    private void Awake()
    {
        wayPointsByArea = new Dictionary<int, Transform[]>();
        wayPointsByArea.Add(3, area3Waypoints);
        wayPointsByArea.Add(4, area4Waypoints);
        wayPointsByArea.Add(5, area5Waypoints);
        wayPointsByArea.Add(6, area6Waypoints);
        wayPointsByArea.Add(7, area7Waypoints);
        wayPointsByArea.Add(8, area8Waypoints);
        wayPointsByArea.Add(9, area9Waypoints);
        wayPointsByArea.Add(10, area10Waypoints);
        wayPointsByArea.Add(11, area11Waypoints);
        wayPointsByArea.Add(12, area12Waypoints);

        activeEnemiesByArea = new Dictionary<int, EnemyAI>();
    }

    private void Start()
    {
        StartCoroutine(SpawnAllEnemies());
        spawn0Available = true;
        spawn1Available = true;
    }

    private EnemyAI SpawnEnemy(Transform spawn, int area)
    {
        GameObject enemy = Instantiate(enemyPrefab, spawn.position, spawn.rotation);
        EnemyAI enemyAI = enemy.GetComponent<EnemyAI>();
        EnemyHealth enemyHealth = enemy.GetComponent<EnemyHealth>();
        EnemyAttack enemyAttack = enemy.GetComponent<EnemyAttack>();
        enemyAI.player = player;       
        enemyAI.enemyArea = area;
        enemyAI.waypoints = new List<Transform>(wayPointsByArea[area]);
        enemyHealth.player = player;
        enemyHealth.enemyManager = this;
        enemyAttack.projectileContainer = projectileContainer;
        if (area == 3 || area == 6 || area == 8 || area == 11)
            enemyAI.centeringOffset = centeringOffset1;
        else if(area == 4 || area == 5 || area == 7 || area == 9 || area == 10 || area == 12)
            enemyAI.centeringOffset = centeringOffset2;
        activeEnemiesByArea[area] = enemyAI;

        return enemyAI;
    }

    private IEnumerator SpawnAllEnemies()
    {
        SpawnEnemy(spawns[0], 7);
        SpawnEnemy(spawns[1], 12);
        yield return new WaitForSeconds(timeBetweenSpawns);
        SpawnEnemy(spawns[0], 5);
        SpawnEnemy(spawns[1], 10);
        yield return new WaitForSeconds(timeBetweenSpawns);
        SpawnEnemy(spawns[0], 6);
        SpawnEnemy(spawns[1], 11);
        yield return new WaitForSeconds(timeBetweenSpawns);
        SpawnEnemy(spawns[0], 4);
        SpawnEnemy(spawns[1], 9);
        yield return new WaitForSeconds(timeBetweenSpawns);
        SpawnEnemy(spawns[0], 3);
        SpawnEnemy(spawns[1], 8);
    }

    public void DeadEnemy(int deadEnemyArea)
    {
        if (deadEnemyArea >= 3 && deadEnemyArea <= 7)
        {
            spawnQueue_Spawn0.Enqueue(deadEnemyArea);
            if (spawn0Available)
            {
                StartCoroutine(ProcessSpawnQueue(0, spawnQueue_Spawn0, spawns[0]));
            }
        }
        else if (deadEnemyArea >= 8 && deadEnemyArea <= 12)
        {
            spawnQueue_Spawn1.Enqueue(deadEnemyArea);
            if (spawn1Available)
            {
                StartCoroutine(ProcessSpawnQueue(1, spawnQueue_Spawn1, spawns[1]));
            }
        }
    }

    private IEnumerator ProcessSpawnQueue(int spawnIndex, Queue<int> queue, Transform spawnPoint)
    {
        Debug.Log("inicio corutina");
        if (spawnIndex == 0) spawn0Available = false;
        else if (spawnIndex == 1) spawn1Available = false;

        while (queue.Count > 0)
        {
            int deadEnemyArea = queue.Dequeue(); // Toma la siguiente área de la cola

            if (activeEnemiesByArea.ContainsKey(deadEnemyArea))
                activeEnemiesByArea.Remove(deadEnemyArea);
            yield return new WaitForSeconds(timeToRespawn / 2);
                
            int newSpawnArea = 0;

            if (deadEnemyArea == 3)
            {
                newSpawnArea = 3;
            }
            else if (deadEnemyArea == 8)
            {
                newSpawnArea = 8;
            }
            if (deadEnemyArea == 4 || deadEnemyArea == 5)
            {
                newSpawnArea = ShiftEnemiesInAreaGroup(deadEnemyArea, new int[] { 5, 4, 3});
            }
            else if (deadEnemyArea == 6 || deadEnemyArea == 7)
            {
                newSpawnArea = ShiftEnemiesInAreaGroup(deadEnemyArea, new int[] { 7, 6, 3 });
            }
            else if (deadEnemyArea == 9 || deadEnemyArea == 10)
            {
                newSpawnArea = ShiftEnemiesInAreaGroup(deadEnemyArea, new int[] { 10, 9, 8 });
            }
            else if (deadEnemyArea == 11)
            {
                newSpawnArea = ShiftEnemiesInAreaGroup(deadEnemyArea, new int[] { 11, 8 });
            }
            else if (deadEnemyArea == 12)
            {
                newSpawnArea = ShiftEnemiesInAreaGroup(deadEnemyArea, new int[] { 12,  8 });
            }

            if (newSpawnArea != 0) //Spawnear enemigo en el area vacia
            {
                yield return new WaitForSeconds(timeToRespawn / 2);
                SpawnEnemy(spawnPoint, newSpawnArea);
            }
        }

        if (spawnIndex == 0) spawn0Available = true;
        else if (spawnIndex == 1) spawn1Available = true;
    }

    private int ShiftEnemiesInAreaGroup(int deadArea, int[] areaOrder)
    {
        int newSpawnArea = deadArea;
        int targetArea = 0;
        for (int i = 0; i < areaOrder.Length - 1; i++)
        {
            int sourceArea = i + 1;
            if (areaOrder[targetArea] <= deadArea)
            {
                if (activeEnemiesByArea.TryGetValue(areaOrder[sourceArea], out EnemyAI enemyToMove) && enemyToMove != null)
                {
                    Debug.Log($"Moving enemy from area {areaOrder[sourceArea]} to {areaOrder[targetArea]}");
                    enemyToMove.enemyArea = areaOrder[targetArea];
                    enemyToMove.waypoints.Clear();
                    enemyToMove.waypoints = new List<Transform>(wayPointsByArea[areaOrder[targetArea]]);
                    enemyToMove.ChangeArea();
                    activeEnemiesByArea.Remove(areaOrder[sourceArea]);
                    activeEnemiesByArea[areaOrder[targetArea]] = enemyToMove;
                    newSpawnArea = areaOrder[sourceArea];
                    targetArea++;
                }
            }
        }
        return newSpawnArea;
    }
}
