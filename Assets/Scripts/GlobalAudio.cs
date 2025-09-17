using System.Collections;
using UnityEngine;
using UnityEngine.Audio;

public class GlobalAudio : MonoBehaviour
{
    public static GlobalAudio Instance;

    [SerializeField] private AudioData audioData;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioMixer audioMixer;
    private AudioClip buttonSound;
    private AudioClip flagTakenSound;
    private AudioClip pointSound;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;            
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        buttonSound = audioData.buttonSound;
        flagTakenSound = audioData.flagTakenSound;
        pointSound = audioData.pointSound;
    }

    private void Start()
    {
        SetMasterVolume(PlayerPrefs.GetFloat("MasterVolume"));
        SetMusicVolume(PlayerPrefs.GetFloat("MusicVolume"));
        SetSFXVolume(PlayerPrefs.GetFloat("SFXVolume"));
    }

    public void PlayButtonSound()
    {
        audioSource.PlayOneShot(buttonSound);
    }

    public void PlayFlagTakenSound()
    {
        audioSource.PlayOneShot(flagTakenSound);
    }

    public void PlayPointSound()
    {
        audioSource.PlayOneShot(pointSound);
    }


    public void SetMasterVolume(float value)
    {
        float v = Mathf.Clamp(value, 0.0001f, 1f);
        audioMixer.SetFloat("MasterVolume", Mathf.Log10(v) * 20); //Se usa el logaritmo para convertir a decibelios
    }

    public void SetMusicVolume(float value)
    {
        float v = Mathf.Clamp(value, 0.0001f, 1f);
        audioMixer.SetFloat("MusicVolume", Mathf.Log10(v) * 20);
    }

    public void SetSFXVolume(float value)
    {
        float v = Mathf.Clamp(value, 0.0001f, 1f);
        audioMixer.SetFloat("SFXVolume", Mathf.Log10(v) * 20);
    }

    public void PlayMusic()
    {
        if (!musicSource.isPlaying)
            musicSource.Play();
    }

    public void StopMusic()
    {
        StartCoroutine(MusicFadeOut());
    }

    public IEnumerator MusicFadeOut()
    {
        float startVolume = musicSource.volume;

        while (musicSource.volume > 0)
        {
            musicSource.volume -= startVolume * Time.deltaTime / 2;
            yield return null;
        }

        musicSource.Stop();
        musicSource.volume = startVolume; 
    }
}
