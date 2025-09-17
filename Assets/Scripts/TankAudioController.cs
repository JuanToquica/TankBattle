using UnityEngine;

public class TankAudioController : MonoBehaviour
{
    [SerializeField] private AudioData audioData;
    [SerializeField] protected AudioSource audioSource;
    [SerializeField] protected AudioSource powerUpSource;
    private AudioClip mainTurretSound;
    private AudioClip railgunSound;
    private AudioClip railgunChargeSound;
    private AudioClip machinegunSound;
    private AudioClip rocketsSound;
    private AudioClip powerUpSound;

    private void Awake()
    {
        mainTurretSound = audioData.mainTurretSound;
        railgunSound = audioData.railgunSound;
        railgunChargeSound = audioData.railgunChargeSound;
        machinegunSound = audioData.machinegunSound;
        rocketsSound = audioData.rocketsSound;
        powerUpSound = audioData.powerUpSound;
    }

    public void PlayMainTurretSound()
    {
        audioSource.PlayOneShot(mainTurretSound);
    }

    public void PlayRailgunSound()
    {
        audioSource.PlayOneShot(railgunSound);
    }

    public void PlayRailgunChargeSound()
    {
        audioSource.PlayOneShot(railgunChargeSound);
    }

    public void PlayMachineGunSound()
    {
        audioSource.PlayOneShot(machinegunSound);
    }
    public void PlayRocketsSound()
    {
        audioSource.PlayOneShot(rocketsSound);
    }

    public void PlayPowerUpSound()
    {
        powerUpSource.PlayOneShot(powerUpSound);
    }
}
