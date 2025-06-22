using UnityEngine;

public class PlayerSpawner : MonoBehaviour
{
    [SerializeField] private Transform[] spawns;
    [SerializeField] private GameObject player;
    [SerializeField] private CameraController cameraController;
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
        cameraController.OnPlayerDead();
        foreach (Transform child in player.transform) //Destruir vfx de powerups que no hayan terminado
        {
            if (child.CompareTag("VFX"))
            {
                ObjectPoolManager.Instance.ReturnPooledObject(child.gameObject);
            }
        }
    }

    private void SpawnPlayer()
    {
        int randomSpawn = Random.Range(0, spawns.Length);
        playerTransform.position = spawns[randomSpawn].position;
        playerTransform.rotation = Quaternion.identity;
        player.SetActive(true);
        playerHealth.RegainHealth();
        cameraController.playerAlive = true;
        cameraController.OnPlayerRevived();
    }
}
