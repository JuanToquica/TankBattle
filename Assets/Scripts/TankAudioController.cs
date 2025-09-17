using UnityEngine;

public class TankAudioController : MonoBehaviour
{
    [SerializeField] protected AudioSource audioSource;
    [SerializeField] protected AudioSource powerUpSource;
    [SerializeField] protected AudioClip mainTurretSound;
    [SerializeField] protected AudioClip railgunSound;
    [SerializeField] protected AudioClip railgunChargeSound;
    [SerializeField] protected AudioClip machinegunSound;
    [SerializeField] protected AudioClip rocketsSound;
    [SerializeField] protected AudioClip powerUpSound;
    
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
