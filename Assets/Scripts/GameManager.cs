using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    public static event System.Action OnStartGame;

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
    public GameObject backToMainTurretVfxPrefab;
    public GameObject DeathVfxPrefab;
    public GameObject FlagVfxPrefab;
    public GameObject DamageTextPrefab;

    [Header("UI")]
    [SerializeField] private HUD hud;
    [SerializeField] private GameObject pauseUI;
    [SerializeField] private GameObject victoryPanel;
    [SerializeField] private GameObject defeatPanel;
    [SerializeField] private GameObject drawPanel;
    [SerializeField] private GameObject coins;
    [SerializeField] private GameObject mainMenuButton;
    [SerializeField] private GameObject flagImage;

    [Header ("Gameplay")]
    [SerializeField] private GameObject flag1;
    [SerializeField] private GameObject flag2;
    [SerializeField] private CameraController cameraController;
    [SerializeField] private float timeToStartGame;
    [SerializeField] private int coinsPerEnemy;
    [SerializeField] private int coinsPerPoint;
    [SerializeField] private int coinsPerGameWon;
    public int playerMaxScore;
    public int enemyMaxScore;
    public float gameTime;
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
        GlobalAudio.Instance.StopMusic();
        playerScore = 0;
        enemyScore = 0;
        coinsEarned = 5;
        flag1.SetActive(false);
        flag2.SetActive(false);
        flagImage.SetActive(false);
        cameraController.sensitivity = PlayerPrefs.GetFloat("Sensitivity", 0.3f);
        InputManager.Instance.playerInput.actions["Pause"].Enable();

        ObjectPoolManager.Instance.CreatePool(projectilePrefab, 10);
        ObjectPoolManager.Instance.CreatePool(railgunBulletPrefab, 10);
        ObjectPoolManager.Instance.CreatePool(machineGunBulletPrefab, 30);
        ObjectPoolManager.Instance.CreatePool(rocketPrefab, 10);
        ObjectPoolManager.Instance.CreatePool(recoveryPowerUpPrefab, 7);
        ObjectPoolManager.Instance.CreatePool(rechargingPowerUpPrefab, 7);
        ObjectPoolManager.Instance.CreatePool(speedPowerUpPrefab, 7);
        ObjectPoolManager.Instance.CreatePool(weaponPowerUpPrefab, 7);
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
        ObjectPoolManager.Instance.CreatePool(backToMainTurretVfxPrefab, 7);
        ObjectPoolManager.Instance.CreatePool(DeathVfxPrefab, 7);
        ObjectPoolManager.Instance.CreatePool(DamageTextPrefab, 15);
        ObjectPoolManager.Instance.CreatePool(FlagVfxPrefab, 2);
        Invoke("StartGame", timeToStartGame);
    }

    private void StartGame()
    {
        hud.ShowMessage(1);
        OnStartGame?.Invoke();
        flag1.SetActive(true);
        flag2.SetActive(true);
    }

    public void OnFlagPickedUp()
    {
        playerHasTheFlag = true;
        GlobalAudio.Instance.PlayFlagTakenSound();
        flagImage.SetActive(true);
        flag1.SetActive(false);
        flag2.SetActive(false);
    }

    public void OnFlagDelivered()
    {
        GlobalAudio.Instance.PlayPointSound();
        playerHasTheFlag = false;
        playerScore ++;
        flagImage.SetActive(false);
        flag1.SetActive(true);
        flag2.SetActive(true);
        coinsEarned += coinsPerPoint;
        if (playerScore >= playerMaxScore)
            EndGame();
        else
            hud.ShowMessage(2);
    }

    public void OnPlayerDeathWithFlag()
    {
        playerHasTheFlag = false;
        enemyScore++;
        flagImage.SetActive(false);
        flag1.SetActive(true);
        flag2.SetActive(true);
        if (enemyScore >= enemyMaxScore)
            EndGame();
        else
            hud.ShowMessage(3);
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
        GlobalAudio.Instance.PlayButtonSound();
        if (isTheGamePaused)
        {
            Time.timeScale = 1;
            pauseUI.SetActive(false);
            isTheGamePaused = false;
            InputManager.Instance.playerInput.actions.FindActionMap("Player").Enable();
            InputManager.Instance.playerInput.actions["SelectButton"].Disable();
            //Cursor.lockState = CursorLockMode.Locked;
            //Cursor.visible = false;
            cameraController.sensitivity = PlayerPrefs.GetFloat("Sensitivity");
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
