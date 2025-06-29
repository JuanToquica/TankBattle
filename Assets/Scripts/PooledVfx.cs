using UnityEngine;

public class PoolledVfx : MonoBehaviour
{
    [SerializeField] ParticleSystem _particles;

    void OnParticleSystemStopped()
    {
        ObjectPoolManager.Instance.ReturnPooledObject(gameObject);
    }

    void OnEnable()
    {
        if (_particles != null)
        {
            _particles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            _particles.Play();
        }
    }
}
