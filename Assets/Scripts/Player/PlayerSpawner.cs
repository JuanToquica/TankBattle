using System.Collections;
using UnityEngine;

public class PlayerSpawner : MonoBehaviour
{
    [SerializeField] private Transform[] spawns;
    [SerializeField] private GameObject player;
    [SerializeField] private CameraController cameraController;
    [SerializeField] private float timeToRespawn;
    [SerializeField] private float timeOfDeath;
    private Transform playerTransform;
    private PlayerHealth playerHealth;
    private PlayerAttack playerAttack;

    private void Start()
    {
        playerTransform = player.GetComponent<Transform>();
        playerHealth = player.GetComponent<PlayerHealth>();
        playerAttack = player.GetComponent<PlayerAttack>();
        SpawnPlayer();
    }

    public void OnPlayerDead()
    {
        StopAllCoroutines();
        StartCoroutine(OnPlayerDeadCorutine());
    }

    private IEnumerator OnPlayerDeadCorutine()
    {       
        Invoke("SpawnPlayer", timeToRespawn);
        yield return new WaitForSeconds(timeOfDeath);
        Debug.Log("Desactivando");
        player.SetActive(false);
        cameraController.OnPlayerDead();
    }


    private void SpawnPlayer()
    {
        foreach (Transform child in player.transform)
        {
            if (child.CompareTag("Explosion") || child.CompareTag("VFX"))
            {
                Destroy(child.gameObject);
            }
        }
        int randomSpawn = Random.Range(0, spawns.Length);
        playerTransform.position = spawns[randomSpawn].position;
        playerTransform.rotation = Quaternion.identity;
        player.SetActive(true);
        playerAttack.enabled = true;
        InputManager.Instance.playerInput.actions.FindActionMap("Player").Enable();
        playerHealth.RegainHealth();
        cameraController.playerAlive = true;
        cameraController.OnPlayerRevived();
    }
}
