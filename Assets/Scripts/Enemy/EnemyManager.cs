using UnityEngine;

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
    }

    [ContextMenu ("Crear enemigo")]
    private void SpawnEnemy(Transform spawn)
    {
        GameObject enemy = Instantiate(enemyPrefab, spawn.position, spawn.rotation);
        EnemyAI enemyAI = enemy.GetComponent<EnemyAI>();
        EnemyHealth enemyHealth = enemy.GetComponent<EnemyHealth>();
        enemyAI.player = player;    
        enemyAI.projectilesContainer = projectileContainer;
        enemyHealth.player = player;
    }
}
