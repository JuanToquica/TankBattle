using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameManager : MonoBehaviour
{
    [SerializeField] private GameObject pauseUI;
    public static GameManager instance; 
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
}
