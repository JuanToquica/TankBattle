using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SettingsIU : MonoBehaviour
{
    [SerializeField] private SettingsData settingsData;
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
        GlobalAudio.Instance.PlayButtonSound();
        mainMenuUI.SetActive(true);
        InputManager.Instance.playerInput.actions["Pause"].Enable();
        gameObject.SetActive(false);
    }

    public void OnMasterVolumeChanged()
    {
        PlayerPrefs.SetFloat("MasterVolume", masterVolume.value);
        GlobalAudio.Instance.SetMasterVolume(masterVolume.value);
    }

    public void OnMusicVolumeChanged()
    {
        PlayerPrefs.SetFloat("MusicVolume", musicVolume.value);
        GlobalAudio.Instance.SetMusicVolume(musicVolume.value);
    }

    public void OnSFXVolumeChanged()
    {
        PlayerPrefs.SetFloat("SFXVolume", SFXVolume.value);
        GlobalAudio.Instance.SetSFXVolume(SFXVolume.value);
    }

    public void OnSensitivityChanged()
    {
        PlayerPrefs.SetFloat("Sensitivity", sensitivity.value);
    }


    public void OnResetVolumeClicked()
    {
        GlobalAudio.Instance.PlayButtonSound();
        PlayerPrefs.SetFloat("MasterVolume", settingsData.defaultMasterVolume);
        PlayerPrefs.SetFloat("MusicVolume", settingsData.defaultMusicVolume);
        PlayerPrefs.SetFloat("SFXVolume", settingsData.defaultSFXVolume);
        LoadSliderSettins();
        EventSystem.current.SetSelectedGameObject(null);
    }

    public void OnResetControlsClicked()
    {
        GlobalAudio.Instance.PlayButtonSound();
        PlayerPrefs.SetFloat("Sensitivity", settingsData.defaultSensitivity);
        LoadSliderSettins();
        InputManager.Instance.ResetActionMaps();
        EventSystem.current.SetSelectedGameObject(null);
    }


    private void OnEnable()
    {
        EventSystem.current.firstSelectedGameObject = masterVolume.gameObject;
        LoadSliderSettins();
    }
}
