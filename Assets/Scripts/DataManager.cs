using UnityEngine;

public class DataManager : MonoBehaviour
{
    public static DataManager Instance;
    private PlayerData _currentPlayerData;

    [SerializeField] private TurretUpgradeData armorStrengthUpgradeData;
    [SerializeField] private TurretUpgradeData mainGunUpgradeData;
    [SerializeField] private TurretUpgradeData railgunUpgradeData;
    [SerializeField] private TurretUpgradeData machineGunUpgradeData;
    [SerializeField] private TurretUpgradeData rocketUpgradeData;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadGameData();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void LoadGameData()
    {
        _currentPlayerData = SaveManager.LoadPlayerData();

        if (_currentPlayerData == null)
        {
            Debug.Log("No se encontró un archivo de guardado. Creando nueva PlayerData por defecto.");
            ResetPlayerData();
        }
        if (_currentPlayerData.colorsPurchased == null || _currentPlayerData.colorsPurchased.Length != 5)
        {
            _currentPlayerData.colorsPurchased = new bool[5];
            _currentPlayerData.colorsPurchased[0] = true;
        }
    }

    public void SaveCurrentPlayerData()
    {
        SaveManager.SavePlayerData(_currentPlayerData);
    }

    [ContextMenu("Reset Player Data")]
    private void ResetPlayerData()
    {
        _currentPlayerData = new PlayerData();
        SaveCurrentPlayerData();
    }

    public void AddCoins(int amount)
    {
        _currentPlayerData.coins += amount;
    }

    public int GetMaxLevel() { return mainGunUpgradeData.maxLevel; }
    public int GetArmorStrengthLevel() { return _currentPlayerData.armorStrengthLevel; }
    public int GetMainTurretLevel() { return _currentPlayerData.mainGunDamageLevel; }
    public int GetRailgunLevel() { return _currentPlayerData.railgunDamageLevel; }
    public int GetMachineGunLevel() { return _currentPlayerData.machineGunDamageLevel; }
    public int GetRocketLevel() { return _currentPlayerData.rocketDamageLevel; }

    public float GetCoinsAmount() { return _currentPlayerData.coins; }
    public float GetArmorStrengthDamage() { return GetUpgradeDamage(armorStrengthUpgradeData, _currentPlayerData.armorStrengthLevel); }
    public float GetMainTurretDamage() { return GetUpgradeDamage(mainGunUpgradeData, _currentPlayerData.mainGunDamageLevel); }
    public float GetRailgunDamage() { return GetUpgradeDamage(railgunUpgradeData, _currentPlayerData.railgunDamageLevel); }
    public float GetMachineGunDamage() { return GetUpgradeDamage(machineGunUpgradeData, _currentPlayerData.machineGunDamageLevel); }
    public float GetRocketDamage() { return GetUpgradeDamage(rocketUpgradeData, _currentPlayerData.rocketDamageLevel); }

    private float GetUpgradeDamage(TurretUpgradeData upgradeData, int currentLevel)
    {
        return upgradeData.upgradeLevels[currentLevel].damageValue;
    }

    public float GetArmorStrengthCost() { return GetUpgradeCost(armorStrengthUpgradeData, _currentPlayerData.armorStrengthLevel); }
    public float GetMainTurretCost() { return GetUpgradeCost(mainGunUpgradeData, _currentPlayerData.mainGunDamageLevel); }
    public float GetRailgunCost() { return GetUpgradeCost(railgunUpgradeData, _currentPlayerData.railgunDamageLevel); }
    public float GetMachineGunCost() { return GetUpgradeCost(machineGunUpgradeData, _currentPlayerData.machineGunDamageLevel); }
    public float GetRocketCost() { return GetUpgradeCost(rocketUpgradeData, _currentPlayerData.rocketDamageLevel); }

    private float GetUpgradeCost(TurretUpgradeData upgradeData, int currentLevel)
    {
        return upgradeData.upgradeLevels[currentLevel].cost;
    }

    public bool IsColorPurchased(int colorIndex)
    {
        return _currentPlayerData.colorsPurchased[colorIndex];
    }
    public int GetCurrentColorSelected()
    {
        return _currentPlayerData.selectedTankColorIndex;
    }

    public void SetSelectedTankColor(int colorIndex)
    {
        if (colorIndex >= 0 && colorIndex < _currentPlayerData.colorsPurchased.Length && _currentPlayerData.colorsPurchased[colorIndex])
        {
            _currentPlayerData.selectedTankColorIndex = colorIndex;
            SaveCurrentPlayerData();
            Debug.Log($"Color de tanque seleccionado: {colorIndex}");
        }
        else
        {
            Debug.LogWarning($"No puedes seleccionar el color {colorIndex}. No ha sido comprado o es inválido.");
        }
    }

    public bool TryPurchaseColor(int colorIndex, int cost)
    {
        if (colorIndex < 0 || colorIndex >= _currentPlayerData.colorsPurchased.Length) return false;
        if (_currentPlayerData.colorsPurchased[colorIndex]) return true; //Ya estaba comprado el color

        if (_currentPlayerData.coins >= cost)
        {
            _currentPlayerData.coins -= cost;
            _currentPlayerData.colorsPurchased[colorIndex] = true;
            SaveCurrentPlayerData();
            Debug.Log($"Color {colorIndex} comprado. Monedas restantes: {_currentPlayerData.coins}");
            return true;
        }
        Debug.Log("No hay suficientes monedas para comprar el color.");
        return false;
    }

    public bool UpgradeArmorStrength() { return TryUpgradeTurret(ref _currentPlayerData.armorStrengthLevel, armorStrengthUpgradeData); }
    public bool UpgradeMainTurret() { return TryUpgradeTurret(ref _currentPlayerData.mainGunDamageLevel, mainGunUpgradeData); }
    public bool UpgradeRailgun() { return TryUpgradeTurret(ref _currentPlayerData.railgunDamageLevel, railgunUpgradeData); }
    public bool UpgradeMachineGun() { return TryUpgradeTurret(ref _currentPlayerData.machineGunDamageLevel, machineGunUpgradeData); }
    public bool UpgradeRocketLauncher() { return TryUpgradeTurret(ref _currentPlayerData.rocketDamageLevel, rocketUpgradeData); }

    public bool TryUpgradeTurret(ref int turretLevel, TurretUpgradeData upgradeData)
    {
        int nextLevel = turretLevel + 1;
        if (nextLevel >= upgradeData.maxLevel)
        {
            Debug.Log("NivelMaximoAlcanzado.");
            return true;
        }

        UpgradeLevel nextLevelData = null;
        foreach (var levelData in upgradeData.upgradeLevels)
        {
            if (levelData.level == nextLevel)
            {
                nextLevelData = levelData;
                break;
            }
        }

        if (nextLevelData != null && _currentPlayerData.coins >= nextLevelData.cost)
        {
            _currentPlayerData.coins -= nextLevelData.cost;
            turretLevel = nextLevel;
            SaveCurrentPlayerData();
            Debug.Log($"Mejora exitosa a nivel {nextLevel}. Monedas restantes: {_currentPlayerData.coins}");
            return true;
        }
        Debug.Log("No hay suficientes monedas");
        return false;
    }
}
