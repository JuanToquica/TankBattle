using System.Collections;
using UnityEngine;

public enum PowerUps { speed, recovery, recharging, weapon}
public class PowerUpSpawner : MonoBehaviour
{
    [SerializeField] private GameObject speedPowerUpPrefab;
    [SerializeField] private GameObject recoveryPowerUpPrefab;
    [SerializeField] private GameObject rechargingPowerUpPrefab;
    [SerializeField] private GameObject weaponPowerUpPrefab;
    [SerializeField] private Transform[] speedPowerUpSpawnPoints;
    [SerializeField] private Transform[] rechargingPowerUpSpawnPoints;
    [SerializeField] private Transform[] recoveryPowerUpSpawnPoints;
    [SerializeField] private Transform[] weaponPowerUpSpawnPoints;
    [SerializeField] private float minTimeToSpawn;
    [SerializeField] private float maxTimeToSpawn;
    [SerializeField] private float gravity;
    [SerializeField] private float spawnHeight;

    private void Start()
    {
        for (int i = 0; i < speedPowerUpSpawnPoints.Length; i++)
        {
            StartCoroutine(SpawnPowerUp(PowerUps.speed, i));
        }
        for (int i = 0; i < recoveryPowerUpSpawnPoints.Length; i++)
        {
            StartCoroutine(SpawnPowerUp(PowerUps.recovery, i));
        }
        for (int i = 0; i < rechargingPowerUpSpawnPoints.Length; i++)
        {
            StartCoroutine(SpawnPowerUp(PowerUps.recharging, i));
        }
        for (int i = 0; i < weaponPowerUpSpawnPoints.Length; i++)
        {
            StartCoroutine(SpawnPowerUp(PowerUps.weapon, i));
        }
    }

    public void PowerUpCollected(PowerUps powerUpType, int index)
    {
        StartCoroutine(SpawnPowerUp(powerUpType, index));
    }

    private IEnumerator SpawnPowerUp(PowerUps powerUpType, int index)
    {
        yield return new WaitForSeconds(Random.Range(minTimeToSpawn, maxTimeToSpawn));
        switch (powerUpType)
        {
            case PowerUps.speed:
                Vector3 position1 = speedPowerUpSpawnPoints[index].position + new Vector3(0, spawnHeight, 0);
                GameObject speedPowerUp = ObjectPoolManager.Instance.GetPooledObject(speedPowerUpPrefab, position1, Quaternion.Euler(-90, 0, 0));
                SpeedPowerUp script1 = speedPowerUp.GetComponent<SpeedPowerUp>();
                if (script1 != null)
                {
                    script1.powerUpSpawner = this;
                    script1.targetPoint = speedPowerUpSpawnPoints[index];
                    script1.gravity = this.gravity;
                    script1.index = index;
                }                   
                break;
            case PowerUps.recovery:
                Vector3 position2 = recoveryPowerUpSpawnPoints[index].position + new Vector3(0, spawnHeight, 0);
                GameObject recoveryPowerUp = ObjectPoolManager.Instance.GetPooledObject(recoveryPowerUpPrefab, position2, Quaternion.Euler(-90, 0, 0));
                RecoveryPowerUp script2 = recoveryPowerUp.GetComponent<RecoveryPowerUp>();
                if (script2 != null)
                {
                    script2.powerUpSpawner = this;
                    script2.targetPoint = recoveryPowerUpSpawnPoints[index];
                    script2.gravity = this.gravity;
                    script2.index = index;
                }
                break;
            case PowerUps.recharging:
                Vector3 position3 = rechargingPowerUpSpawnPoints[index].position + new Vector3(0, spawnHeight, 0);
                GameObject rechargingPowerUp = ObjectPoolManager.Instance.GetPooledObject(rechargingPowerUpPrefab, position3, Quaternion.Euler(-90, 0, 0));
                RechargingPowerUp script3 = rechargingPowerUp.GetComponent<RechargingPowerUp>();
                if (script3 != null)
                {
                    script3.powerUpSpawner = this;
                    script3.targetPoint = rechargingPowerUpSpawnPoints[index];
                    script3.gravity = this.gravity;
                    script3.index = index;
                }
                break;
            case PowerUps.weapon:
                Vector3 position4 = weaponPowerUpSpawnPoints[index].position + new Vector3(0, spawnHeight, 0);
                GameObject weaponPowerUp = ObjectPoolManager.Instance.GetPooledObject(weaponPowerUpPrefab, position4, Quaternion.Euler(-90, 0, 0));
                WeaponPowerUp script4 = weaponPowerUp.GetComponent<WeaponPowerUp>();
                if (script4 != null)
                {
                    script4.powerUpSpawner = this;
                    script4.targetPoint = weaponPowerUpSpawnPoints[index];
                    script4.gravity = this.gravity;
                    script4.index = index;
                }
                break;
        }
    }
}
