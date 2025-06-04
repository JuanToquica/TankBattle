using UnityEngine;

public class PlayerSpawner : MonoBehaviour
{
    [SerializeField] private Transform[] spawns;
    [SerializeField] private GameObject player;
    [SerializeField] private float timeToRespawn;
    private Transform playerTransform;
    private PlayerHealth playerHealth;

    private void Start()
    {
        playerTransform = player.GetComponent<Transform>();
        playerHealth = player.GetComponent<PlayerHealth>();
        SpawnPlayer();
    }

    public void OnPlayerDead()
    {
        player.SetActive(false);
        Invoke("SpawnPlayer", timeToRespawn);
    }

    private void SpawnPlayer()
    {
        int randomSpawn = Random.Range(0, spawns.Length);
        playerTransform.position = spawns[randomSpawn].position;
        playerTransform.rotation = Quaternion.identity;
        player.SetActive(true);
        playerHealth.RegainHealth();
    }
}
