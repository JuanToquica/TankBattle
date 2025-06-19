using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class VictoryAndDefeatUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI coinsAmount;
    [SerializeField] private Button exitButton;
    [SerializeField] private GameObject loadPanel;
    [SerializeField] private Slider loadBar;

    private void Start()
    {
        exitButton.onClick.AddListener(OnExitButton);
    }

    private void ShowEarnedCoins()
    {
        coinsAmount.text = $"{GameManager.instance.GetCoinsEarned()}";
    }

    private void OnExitButton()
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
        EventSystem.current.firstSelectedGameObject = exitButton.gameObject;
        ShowEarnedCoins();
    }
}
