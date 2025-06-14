using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class VictoryAndDefeatUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI coinsAmount;
    [SerializeField] private Button exitButton;

    private void Start()
    {
        exitButton.onClick.AddListener(OnExitButton);
    }

    private void OnExitButton()
    {
        Time.timeScale = 1;
        SceneManager.LoadScene("MainMenu");
    }

    private void ShowEarnedCoins()
    {
        coinsAmount.text = $"{GameManager.instance.GetCoinsEarned()}";
    }

    public void OnEnable()
    {
        EventSystem.current.firstSelectedGameObject = exitButton.gameObject;
        ShowEarnedCoins();
    }
}
