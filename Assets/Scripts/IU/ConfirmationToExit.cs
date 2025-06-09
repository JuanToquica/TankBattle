using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

public class ConfirmationToExit : MonoBehaviour
{
    [SerializeField] private GameObject pauseIU;
    [SerializeField] private Button cancelButton;
    [SerializeField] private Button confirmButton;

    private void Start()
    {
        cancelButton.onClick.AddListener(OnCancelButtonClicked);
        confirmButton.onClick.AddListener(OnConfirmButtonClicked);
    }

    public void OnCancelButtonClicked()
    {
        pauseIU.SetActive(true);
        gameObject.SetActive(false);
    }

    public void OnConfirmButtonClicked()
    {
        Time.timeScale = 1;
        SceneManager.LoadScene("MainMenu");
    }

    public void OnEnable()
    {
        EventSystem.current.firstSelectedGameObject = cancelButton.gameObject;
    }
}
