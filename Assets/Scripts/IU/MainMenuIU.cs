using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainMenuiu : MonoBehaviour
{
    [SerializeField] private GameObject SettingsIU;
    [SerializeField] private Button playButton;
    [SerializeField] private Button garageButton;
    [SerializeField] private Button settingsButton;

    private void Start()
    {
        playButton.onClick.AddListener(OnPlayButtonClicked);
        garageButton.onClick.AddListener(OnGarageButtonClicked);
        settingsButton.onClick.AddListener(OnSettingsButtonClicked);
    }

    public void OnPlayButtonClicked()
    {
        SceneManager.LoadScene("Battlefield");
    }

    public void OnGarageButtonClicked()
    {
        SceneManager.LoadScene("Garage");
    }

    public void OnSettingsButtonClicked()
    {
        SettingsIU.SetActive(false);
    }

}
