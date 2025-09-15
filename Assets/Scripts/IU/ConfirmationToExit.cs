using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using System.Collections;
public class ConfirmationToExit : MonoBehaviour
{
    [SerializeField] private GameObject pauseIU;
    [SerializeField] private GameObject loadPanel;
    [SerializeField] private Button cancelButton;
    [SerializeField] private Button confirmButton;
    [SerializeField] private Slider loadBar;

    private void Start()
    {
        cancelButton.onClick.AddListener(OnCancelButtonClicked);
        confirmButton.onClick.AddListener(OnConfirmButtonClicked);
    }

    public void OnCancelButtonClicked()
    {
        pauseIU.SetActive(true);
        InputManager.Instance.playerInput.actions["Pause"].Enable();
        gameObject.SetActive(false);
    }

    public void OnConfirmButtonClicked()
    {       
        Time.timeScale = 1;
        loadPanel.SetActive(true);
        StartCoroutine(LoadAsync("MainMenu"));
    }

    private IEnumerator LoadAsync(string scene)
    {
        AsyncOperation asyncOperation = SceneManager.LoadSceneAsync(scene);
        while (!asyncOperation.isDone)
        {
            Debug.Log(asyncOperation.progress);
            loadBar.value = asyncOperation.progress / 0.9f;
            yield return null;
        }
    }

    public void OnEnable()
    {
        EventSystem.current.firstSelectedGameObject = cancelButton.gameObject;
    }
}
