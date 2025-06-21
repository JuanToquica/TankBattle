using UnityEngine;


[CreateAssetMenu(fileName = "NewWeaponsSettings", menuName = "Weapons Settings")]
public class WeaponsSettings : ScriptableObject
{
    [Header("References")]
    public GameObject projectilePrefab;
    public GameObject railgunBulletPrefab;
    public GameObject bulletPrefab;
    public GameObject rocketPrefab;
    public GameObject shotVfx;
    public GameObject railgunVfx;

    [Header("Fire Specifications")]
    public float[] rangeOfTurrets;
    public Vector2 amountOfShotsInOneRound;
    public float aimAngle;
    public int defaultAmountOfRaycast;
    public int railGunAmountOfRaycast;
    public float projectileSpeed;
    public float railgunBulletSpeed;
    public float bulletSpeed;
    public float rocketSpeed;
    public float bulletRange;
    public float railgunDelay;
    public float railgunAmmo;
    public float machineGunAmmo;
    public float rocketAmmo;
    public float timeBetweenShoots;
    public float machineGunAngularSpeed;
}

