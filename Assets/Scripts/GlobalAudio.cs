using UnityEngine;

public class GlobalAudio : MonoBehaviour
{
    public static GlobalAudio Instance;

    [SerializeField] private AudioClip buttonSound;
    [SerializeField] private AudioClip flagTakenSound;
    [SerializeField] private AudioClip pointSound;

    private AudioSource audioSource;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            audioSource = GetComponent<AudioSource>();
        }
        else Destroy(gameObject);
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
}
