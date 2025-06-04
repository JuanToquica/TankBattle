using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameManager : MonoBehaviour
{
    [SerializeField] private GameObject pauseUI;
    public static GameManager instance;
    private InputManager inputManager;   
    [SerializeField] private GameObject flag1;
    [SerializeField] private GameObject flag2;
    public int playerMaxScore;
    public int enemyMaxScore;
    public float time;
    public bool isTheGamePaused;    
    public int playerScore, enemyScore;
    public bool playerHasTheFlag;

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
        inputManager = GetComponent<InputManager>();
        inputManager.playerInput.IU.Pause.started += ctx => PauseAndUnpauseGame();
        playerScore = 0;
        enemyScore = 0;        
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
    }

    public void OnPlayerDeathWithFlag()
    {
        playerHasTheFlag = false;
        enemyScore++;
        flag1.SetActive(true);
        flag2.SetActive(true);
    }

    public void PauseAndUnpauseGame()
    {
        if (isTheGamePaused)
        {
            Time.timeScale = 1;
            pauseUI.SetActive(false);
            isTheGamePaused = false;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        else
        {
            Time.timeScale = 0;
            pauseUI.SetActive(true);
            isTheGamePaused = true;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }
}
