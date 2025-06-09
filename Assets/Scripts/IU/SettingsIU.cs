using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SettingsIU : MonoBehaviour
{
    [SerializeField] private GameObject mainMenuUI;
    [SerializeField] private Button backButton;
    [SerializeField] private Slider masterVolume;
    [SerializeField] private Slider musicVolume;
    [SerializeField] private Slider SFXVolume;
    [SerializeField] private Slider sensitivity;
    [SerializeField] private Button resetVolumeSettings;
    [SerializeField] private Button resetControlsSettings;

    private void Start()
    {
        backButton.onClick.AddListener(OnBackButtonClicked);
        resetVolumeSettings.onClick.AddListener(OnResetVolumeClicked);
        resetControlsSettings.onClick.AddListener(OnResetControlsClicked);
        LoadSliderSettins();
    }

    private void LoadSliderSettins()
    {
        masterVolume.value = PlayerPrefs.GetFloat("MasterVolume");
        musicVolume.value = PlayerPrefs.GetFloat("MusicVolume");
        SFXVolume.value = PlayerPrefs.GetFloat("SFXVolume");
        sensitivity.value = PlayerPrefs.GetFloat("Sensitivity");
    }

    public void OnBackButtonClicked()
    {
        mainMenuUI.SetActive(true);
        gameObject.SetActive(false);
    }

    public void OnMasterVolumeChanged()
    {
        PlayerPrefs.SetFloat("MasterVolume", masterVolume.value);
    }

    public void OnMusicVolumeChanged()
    {
        PlayerPrefs.SetFloat("MusicVolume", musicVolume.value);
    }

    public void OnSFXVolumeChanged()
    {
        PlayerPrefs.SetFloat("SFXVolume", SFXVolume.value);
    }

    public void OnSensitivityChanged()
    {
        PlayerPrefs.SetFloat("Sensitivity", sensitivity.value);
    }


    public void OnResetVolumeClicked()
    {
        if (PlayerPrefs.HasKey("MasterVolume"))
            PlayerPrefs.DeleteKey("MasterVolume");
        if (PlayerPrefs.HasKey("MusicVolume"))
            PlayerPrefs.DeleteKey("MusicVolume");
        if (PlayerPrefs.HasKey("SFXVolume"))
            PlayerPrefs.DeleteKey("SFXVolume");
        LoadSliderSettins();
    }

    public void OnResetControlsClicked()
    {
        if (PlayerPrefs.HasKey("Sensitivity"))
            PlayerPrefs.DeleteKey("Sensitivity");
        LoadSliderSettins();
        InputManager.Instance.ResetActionMaps();
    }


    private void OnEnable()
    {
        EventSystem.current.firstSelectedGameObject = backButton.gameObject;
        LoadSliderSettins();
    }
}
