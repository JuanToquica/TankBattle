using UnityEngine;

public class TankAudioController : MonoBehaviour
{
    [SerializeField] private AudioData audioData;
    [SerializeField] protected AudioSource audioSource;
    [SerializeField] protected AudioSource powerUpSource;

    public void PlayMainTurretSound()
    {
        audioSource.PlayOneShot(audioData.mainTurretSound);
    }

    public void PlayRailgunSound()
    {
        audioSource.PlayOneShot(audioData.railgunSound);
    }

    public void PlayRailgunChargeSound()
    {
        audioSource.PlayOneShot(audioData.railgunChargeSound);
    }

    public void PlayMachineGunSound()
    {
        audioSource.PlayOneShot(audioData.machinegunSound);
    }
    public void PlayRocketsSound()
    {
        audioSource.PlayOneShot(audioData.rocketsSound);
    }

    public void PlayPowerUpSound()
    {
        powerUpSource.PlayOneShot(audioData.powerUpSound);
    }

    public void PlayDeathSound()
    {
        powerUpSource.PlayOneShot(audioData.deathSound);
    }
}
