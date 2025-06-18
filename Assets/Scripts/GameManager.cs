using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    [SerializeField] private GameObject pauseUI;
    [SerializeField] private GameObject victoryPanel;
    [SerializeField] private GameObject defeatPanel;
    [SerializeField] private GameObject drawPanel;
    [SerializeField] private GameObject coins;
    [SerializeField] private GameObject mainMenuButton;
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
