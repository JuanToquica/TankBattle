using UnityEngine;

public class SmokeTrail : MonoBehaviour
{
    [SerializeField] private ParticleSystem smokeTrail;

    public void OnRocketCollision()
    {
        var mainModule = smokeTrail.main;
        mainModule.loop = false;
        smokeTrail.Stop();
    }
}
