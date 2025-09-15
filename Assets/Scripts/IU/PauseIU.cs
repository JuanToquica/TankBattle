using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseIU : MonoBehaviour
{
    [SerializeField] private GameObject SettingsIU;
    [SerializeField] private GameObject ConfirmationIU;
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button exitButton;

    private void Start()
    {
        resumeButton.onClick.AddListener(OnResumeButtonClicked);
        settingsButton.onClick.AddListener(OnSettingsButtonClicked);
        exitButton.onClick.AddListener(OnExitButtonClicked);
    }

    public void OnResumeButtonClicked()
    {
        GameManager.instance.PauseAndUnpauseGame();
    }

    public void OnSettingsButtonClicked()
    {
        InputManager.Instance.playerInput.actions["Pause"].Disable();
        SettingsIU.SetActive(true);
    }

    public void OnExitButtonClicked()
    {
        InputManager.Instance.playerInput.actions["Pause"].Disable();
        ConfirmationIU.SetActive(true);
        gameObject.SetActive(false);
    }

    public void OnEnable()
    {
        EventSystem.current.firstSelectedGameObject = resumeButton.gameObject;
    }
}
