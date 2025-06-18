using UnityEngine;

[System.Serializable]
public class UpgradeLevel
{
    public int level;
    public int damageValue;
    public int cost;
}

[CreateAssetMenu(fileName = "NewTurretUpgradeData", menuName = "Turret Upgrade Data")]
public class TurretUpgradeData : ScriptableObject
{
    public UpgradeLevel[] upgradeLevels;
    public int maxLevel => upgradeLevels.Length > 0 ? upgradeLevels.Length : 1;
}
