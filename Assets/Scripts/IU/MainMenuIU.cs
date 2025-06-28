using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using System.Collections;

public class MainMenuiu : MonoBehaviour
{
    [SerializeField] private GameObject settingsIU;
    [SerializeField] private GameObject loadPanel;
    [SerializeField] private Button playButton;
    [SerializeField] private Button garageButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Slider loadBar;

    private void Start()
    {
        playButton.onClick.AddListener(OnPlayButtonClicked);
        garageButton.onClick.AddListener(OnGarageButtonClicked);
        settingsButton.onClick.AddListener(OnSettingsButtonClicked);
        StartCoroutine(Deselect());
    }

    IEnumerator Deselect()
    {
        yield return null;
        EventSystem.current.SetSelectedGameObject(null);
    }

    public void OnPlayButtonClicked()
    {
        loadPanel.SetActive(true);
        StartCoroutine(LoadAsync("Battlefield"));
    }

    public void OnGarageButtonClicked()
    {
        loadPanel.SetActive(true);
        StartCoroutine(LoadAsync("Garage"));
    }

    public void OnSettingsButtonClicked()
    {
        settingsIU.SetActive(true);
        gameObject.SetActive(false);
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
        EventSystem.current.firstSelectedGameObject = playButton.gameObject;        
    }
}
