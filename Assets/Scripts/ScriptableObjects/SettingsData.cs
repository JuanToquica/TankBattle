using UnityEngine;

[CreateAssetMenu(fileName = "NewSettingsData", menuName = "ScriptableObject/Settings Data")]
public class SettingsData : ScriptableObject
{
    public float defaultMasterVolume;
    public float defaultMusicVolume;
    public float defaultSFXVolume;
    public float defaultSensitivity;
}
