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
    private PlayerAttack playerAttack;

    private void OnEnable()
    {
        PlayerHealth.OnPlayerDead += OnPlayerDead;
    }

    private void OnDisable()
    {
        PlayerHealth.OnPlayerDead -= OnPlayerDead;
    }

    private void Start()
    {
        playerTransform = player.GetComponent<Transform>();
        playerAttack = player.GetComponent<PlayerAttack>();
        SpawnPlayer(1);
    }

    public void OnPlayerDead()
    {
        StopAllCoroutines();
        StartCoroutine(OnPlayerDeadCorutine());
    }

    private IEnumerator OnPlayerDeadCorutine()
    {       
        yield return new WaitForSeconds(timeOfDeath);
        Debug.Log("Desactivando");
        player.SetActive(false);
        cameraController.OnPlayerDead();
        yield return new WaitForSeconds(timeToRespawn - timeOfDeath);
        SpawnPlayer(Random.Range(0, spawns.Length));
    }


    private void SpawnPlayer(int randomSpawn)
    {
        foreach (Transform child in player.transform)
        {
            if (child.CompareTag("Explosion") || child.CompareTag("VFX"))
            {
                Destroy(child.gameObject);
            }
        }
        playerTransform.position = spawns[randomSpawn].position;
        playerTransform.rotation = Quaternion.identity;
        player.SetActive(true);
        playerAttack.enabled = true;
        InputManager.Instance.playerInput.actions.FindActionMap("Player").Enable();
        cameraController.playerAlive = true;
        cameraController.OnPlayerRevived();
    }
}
