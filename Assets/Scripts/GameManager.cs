using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    [Header("Prefabs")]
    public GameObject projectilePrefab;
    public GameObject railgunBulletPrefab;
    public GameObject machineGunBulletPrefab;
    public GameObject rocketPrefab;
    public GameObject recoveryPowerUpPrefab;
    public GameObject rechargingPowerUpPrefab;
    public GameObject speedPowerUpPrefab;
    public GameObject weaponPowerUpPrefab;
    public GameObject PowerUpSmokeVfxPrefab;
    public GameObject recoveryPowerUpVfxPrefab;
    public GameObject rechargingPowerUpVfxPrefab;
    public GameObject speedPowerUpVfxPrefab;
    public GameObject weaponPowerUpVfxPrefab;
    public GameObject maingunImpactVfxPrefab;   
    public GameObject railgunImpactVfxPrefab;
    public GameObject machineGunImpactVfxPrefab;
    public GameObject rocketImpactVfxPrefab;
    public GameObject rocketFireVfxPrefab;
    public GameObject rocketSmokeTrailPrefab;
    public GameObject maingunShotVfxPrefab;
    public GameObject railgunShotVfxPrefab;

    [Header ("UI")]
    [SerializeField] private GameObject pauseUI;
    [SerializeField] private GameObject victoryPanel;
    [SerializeField] private GameObject defeatPanel;
    [SerializeField] private GameObject drawPanel;
    [SerializeField] private GameObject coins;
    [SerializeField] private GameObject mainMenuButton;

    [Header ("Gameplay")]
    [SerializeField] private GameObject flag1;
    [SerializeField] private GameObject flag2;
    [SerializeField] private int coinsPerEnemy;
    [SerializeField] private int coinsPerPoint;
    [SerializeField] private int coinsPerGameWon;
    public int playerMaxScore;
    public int enemyMaxScore;
    public float time;
    public bool isTheGamePaused;    
    public int playerScore, enemyScore;
    public bool playerHasTheFlag;
    public int coinsEarned;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        playerScore = 0;
        enemyScore = 0;
        coinsEarned = 5;

        ObjectPoolManager.Instance.CreatePool(projectilePrefab, 10);
        ObjectPoolManager.Instance.CreatePool(railgunBulletPrefab, 10);
        ObjectPoolManager.Instance.CreatePool(machineGunBulletPrefab, 30);
        ObjectPoolManager.Instance.CreatePool(rocketPrefab, 10);
        ObjectPoolManager.Instance.CreatePool(recoveryPowerUpPrefab, 10);
        ObjectPoolManager.Instance.CreatePool(rechargingPowerUpPrefab, 10);
        ObjectPoolManager.Instance.CreatePool(speedPowerUpPrefab, 10);
        ObjectPoolManager.Instance.CreatePool(weaponPowerUpPrefab, 10);
        ObjectPoolManager.Instance.CreatePool(PowerUpSmokeVfxPrefab, 15);
        ObjectPoolManager.Instance.CreatePool(recoveryPowerUpVfxPrefab, 4);
        ObjectPoolManager.Instance.CreatePool(rechargingPowerUpVfxPrefab, 4);
        ObjectPoolManager.Instance.CreatePool(speedPowerUpVfxPrefab, 4);
        ObjectPoolManager.Instance.CreatePool(weaponPowerUpVfxPrefab, 4);
        ObjectPoolManager.Instance.CreatePool(maingunImpactVfxPrefab, 10);
        ObjectPoolManager.Instance.CreatePool(railgunImpactVfxPrefab, 10);
        ObjectPoolManager.Instance.CreatePool(machineGunImpactVfxPrefab, 30);
        ObjectPoolManager.Instance.CreatePool(rocketImpactVfxPrefab, 10);
        ObjectPoolManager.Instance.CreatePool(rocketSmokeTrailPrefab, 10);
        ObjectPoolManager.Instance.CreatePool(maingunShotVfxPrefab, 10);
        ObjectPoolManager.Instance.CreatePool(railgunShotVfxPrefab, 10);
}

    public void OnFlagPickedUp()
    {
        playerHasTheFlag = true;
        flag1.SetActive(false);
        flag2.SetActive(false);
    }

    public void OnFlagDelivered()
    {
        playerHasTheFlag = false;
        playerScore ++;
        flag1.SetActive(true);
        flag2.SetActive(true);
        coinsEarned += coinsPerPoint;
        if (playerScore >= playerMaxScore)
            EndGame();
    }

    public void OnPlayerDeathWithFlag()
    {
        playerHasTheFlag = false;
        enemyScore++;
        flag1.SetActive(true);
        flag2.SetActive(true);
        if (enemyScore >= enemyMaxScore)
            EndGame();
    }

    public int GetCoinsEarned()
    {
        return coinsEarned;
    }

    public void OnEnemyDead()
    {
        coinsEarned += coinsPerEnemy;
    }
    public void PauseAndUnpauseGame()
    {
        if (isTheGamePaused)
        {
            Time.timeScale = 1;
            pauseUI.SetActive(false);
            isTheGamePaused = false;
            InputManager.Instance.playerInput.actions.FindActionMap("Player").Enable();
            InputManager.Instance.playerInput.actions["SelectButton"].Disable();
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        else
        {
            Time.timeScale = 0;
            pauseUI.SetActive(true);
            isTheGamePaused = true;
            InputManager.Instance.playerInput.actions.FindActionMap("Player").Disable();
            InputManager.Instance.playerInput.actions["SelectButton"].Enable();
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    public void EndGame()
    {
        InputManager.Instance.playerInput.actions.FindActionMap("Player").Disable();
        InputManager.Instance.playerInput.actions["SelectButton"].Enable();
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        Time.timeScale = 0;
        if (playerScore > enemyScore)
        {
            coinsEarned += coinsPerGameWon;
            victoryPanel.SetActive(true);
            coins.SetActive(true);
            mainMenuButton.SetActive(true);
        }        
        else if (enemyScore > playerScore)
        {
            defeatPanel.SetActive(true);
            coins.SetActive(true);
            mainMenuButton.SetActive(true);
        }           
        else
        {
            coinsEarned += coinsPerGameWon / 2;
            drawPanel.SetActive(true);
            coins.SetActive(true);
            mainMenuButton.SetActive(true);
        }
        DataManager.Instance.AddCoins(coinsEarned);
        Debug.Log("Monedas añadidas");
    }
}
