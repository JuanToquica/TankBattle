using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GarageUI : MonoBehaviour
{
    [Header ("Button References")]
    [SerializeField] private Button backButton;
    [SerializeField] private Button redPaintButton;
    [SerializeField] private Button bluePaintButton;
    [SerializeField] private Button purplePaintButton;
    [SerializeField] private Button yellowPaintButton;
    [SerializeField] private Button greenPaintButton;
    [SerializeField] private Button equipButton;
    [SerializeField] private Button buyButton;   
    [SerializeField] private Button upgradeArmorButton;  
    [SerializeField] private Button upgradeMainGunDamageButton;
    [SerializeField] private Button upgradeRailgunDamageButton;
    [SerializeField] private Button upgradeMachinegunDamageButton;
    [SerializeField] private Button upgradeRocketDamageButton;
    [SerializeField] private Button okButton;
    [SerializeField] private Button mainTurretButton;
    [SerializeField] private Button railgunTurretButton;
    [SerializeField] private Button machineGunTurretButton;
    [SerializeField] private Button rocketsButton;

    [Header ("Text References")]  
    [SerializeField] private TextMeshProUGUI coinsAmount;
    [SerializeField] private TextMeshProUGUI ArmorDamageValue;
    [SerializeField] private TextMeshProUGUI mainGunDamageValue;
    [SerializeField] private TextMeshProUGUI railgunDamageValue;
    [SerializeField] private TextMeshProUGUI machinegunDamageValue;
    [SerializeField] private TextMeshProUGUI rocketDamageValue;

    [Header ("Image References")]
    [SerializeField] private GameObject[] colorPadlocks;
    [SerializeField] private Image[] colorBackgrounds;
    [SerializeField] private GameObject equippedImage;
    [SerializeField] private GameObject notEnoughCoinsImage;
    [SerializeField] private Image armorBar;
    [SerializeField] private Image mainGunBar;
    [SerializeField] private Image railgunBar;
    [SerializeField] private Image machineGunBar;
    [SerializeField] private Image rocketBar;
    
    [Header ("Other Referernces")]
    [SerializeField] private GarageTankMaterialHandler tank;
    [SerializeField] private MaterialsData materialsData;
    public int currentColorSelected;

    private void Start()
    {
        backButton.onClick.AddListener(OnBackButtonClicked);
        redPaintButton.onClick.AddListener(OnRedPaintButton);
        bluePaintButton.onClick.AddListener(OnBluePaintButton);
        purplePaintButton.onClick.AddListener(OnPurplePaintButton);
        yellowPaintButton.onClick.AddListener(OnYellowPaintButton);
        greenPaintButton.onClick.AddListener(OnGreenPaintButton);
        buyButton.onClick.AddListener(OnBuyButton);
        equipButton.onClick.AddListener(OnEquipButton);
        okButton.onClick.AddListener(OnOkButton);
        mainTurretButton.onClick.AddListener(OnMainTurretButton);
        railgunTurretButton.onClick.AddListener(OnRailgunButton);
        machineGunTurretButton.onClick.AddListener(OnMachineGunButton);
        rocketsButton.onClick.AddListener(OnRocketsButton);

        upgradeArmorButton.onClick.AddListener(OnUpgradeArmorButton);
        upgradeMainGunDamageButton.onClick.AddListener(OnUpgradeMainGunDamageButton);
        upgradeRailgunDamageButton.onClick.AddListener(OnUpgradeRailgunDamageButton);
        upgradeMachinegunDamageButton.onClick.AddListener(OnUpgradeMachineGunDamageButton);
        upgradeRocketDamageButton.onClick.AddListener(OnUpgradeRocketDamageButton);

        HideColorButtons();

        UpdateUITexts();
        UpdateUIBarsAndUpdateButtons();
    }

    private void UpdateUITexts()
    {
        coinsAmount.text = $"{DataManager.Instance.GetCoinsAmount()}";
        ArmorDamageValue.text = $"{DataManager.Instance.GetArmorStrengthDamage()}";
        mainGunDamageValue.text = $"{DataManager.Instance.GetMainTurretDamage()}";
        railgunDamageValue.text = $"{DataManager.Instance.GetRailgunDamage()}";
        machinegunDamageValue.text = $"{DataManager.Instance.GetMachineGunDamage()}";
        rocketDamageValue.text = $"{DataManager.Instance.GetRocketDamage()}";
    }

    private void UpdateUIButtons()
    {
        if (DataManager.Instance.IsColorPurchased(currentColorSelected))
        {
            buyButton.gameObject.SetActive(false);
            if (DataManager.Instance.GetCurrentColorSelected() == currentColorSelected)
            {
                equippedImage.SetActive(true);
                equipButton.gameObject.SetActive(false);
            }
            else
            {
                equippedImage.SetActive(false);
                equipButton.gameObject.SetActive(true);
            }

        }
        else
        {
            buyButton.gameObject.SetActive(true);
            equippedImage.SetActive(false);
            equipButton.gameObject.SetActive(false);
        }
    }

    private void UpdateUIBarsAndUpdateButtons()
    {
        int armorLevel = DataManager.Instance.GetArmorStrengthLevel() + 1;
        int mainGunLevel = DataManager.Instance.GetMainTurretLevel() + 1;
        int railgunLevel = DataManager.Instance.GetRailgunLevel() + 1;
        int machineGunLevel = DataManager.Instance.GetMachineGunLevel() + 1;
        int rocketLevel = DataManager.Instance.GetRocketLevel() + 1;
        float maxLevel = (float)DataManager.Instance.GetMaxLevel();

        armorBar.fillAmount = armorLevel / maxLevel;
        mainGunBar.fillAmount = mainGunLevel / maxLevel;
        railgunBar.fillAmount = railgunLevel / maxLevel;
        machineGunBar.fillAmount = machineGunLevel / maxLevel;
        rocketBar.fillAmount = rocketLevel / maxLevel;

        if (armorLevel == maxLevel) upgradeArmorButton.gameObject.SetActive(false);
        if (mainGunLevel == maxLevel) upgradeMainGunDamageButton.gameObject.SetActive(false);
        if (railgunLevel == maxLevel) upgradeRailgunDamageButton.gameObject.SetActive(false);
        if (machineGunLevel == maxLevel) upgradeMachinegunDamageButton.gameObject.SetActive(false);
        if (rocketLevel == maxLevel) upgradeRocketDamageButton.gameObject.SetActive(false);

        HideColorButtons();
    }

    private void HideColorButtons()
    {
        buyButton.gameObject.SetActive(false);
        equippedImage.SetActive(false);
        equipButton.gameObject.SetActive(false);
    }

    private void UpdateColorBackgrounds()
    {
        int currentColor = DataManager.Instance.GetCurrentColorSelected();
        for (int i = 0; i < 5; i++)
        {
            if (i == currentColor)
                colorBackgrounds[i].enabled = true;
            else
                colorBackgrounds[i].enabled = false;
        }
    }

    private void UpdateColorPadlocks()
    {
        for (int i = 0; i < 5; i++)
        {
            if (DataManager.Instance.IsColorPurchased(i))
                colorPadlocks[i].SetActive(false);
            else
                colorPadlocks[i].SetActive(true);              
        }
    }

    public void OnBackButtonClicked()
    {
        SceneManager.LoadScene("MainMenu");
    }

    public void OnRedPaintButton() => OnPaintButton(0);
    public void OnBluePaintButton() => OnPaintButton(1);
    public void OnPurplePaintButton() => OnPaintButton(2);
    public void OnYellowPaintButton() => OnPaintButton(3);
    public void OnGreenPaintButton() => OnPaintButton(4);

    private void OnPaintButton(int index)
    {
        tank.LoadTankMaterial(index);
        currentColorSelected = index;
        UpdateUIButtons();
        EventSystem.current.SetSelectedGameObject(null);
    }

    public void OnBuyButton()
    {
        if (DataManager.Instance.TryPurchaseColor(currentColorSelected, materialsData.cost))
            DataManager.Instance.SetSelectedTankColor(currentColorSelected);
        else
            notEnoughCoinsImage.SetActive(true);
        UpdateUIButtons();
        UpdateUITexts();
        UpdateColorBackgrounds();
        UpdateColorPadlocks();
    }

    public void OnEquipButton()
    {
        DataManager.Instance.SetSelectedTankColor(currentColorSelected);
        UpdateUIButtons();
        UpdateColorBackgrounds();
    }

    public void OnUpgradeArmorButton()
    {
        if (!DataManager.Instance.UpgradeArmorStrength())
            notEnoughCoinsImage.SetActive(true);
        UpdateUITexts();
        UpdateUIBarsAndUpdateButtons();
        EventSystem.current.SetSelectedGameObject(null);
    }

    public void OnUpgradeMainGunDamageButton()
    {
        if (!DataManager.Instance.UpgradeMainTurret())
            notEnoughCoinsImage.SetActive(true);
        UpdateUITexts();
        UpdateUIBarsAndUpdateButtons();
        EventSystem.current.SetSelectedGameObject(null);
    }

    public void OnUpgradeRailgunDamageButton()
    {
        if (!DataManager.Instance.UpgradeRailgun())
            notEnoughCoinsImage.SetActive(true);
        UpdateUITexts();
        UpdateUIBarsAndUpdateButtons();
        EventSystem.current.SetSelectedGameObject(null);
    }

    public void OnUpgradeMachineGunDamageButton()
    {
        if (!DataManager.Instance.UpgradeMachineGun())
            notEnoughCoinsImage.SetActive(true);
        UpdateUITexts();
        UpdateUIBarsAndUpdateButtons();
        EventSystem.current.SetSelectedGameObject(null);
    }

    public void OnUpgradeRocketDamageButton()
    {
        if (!DataManager.Instance.UpgradeRocketLauncher())
            notEnoughCoinsImage.SetActive(true);
        UpdateUITexts();
        UpdateUIBarsAndUpdateButtons();
        EventSystem.current.SetSelectedGameObject(null);
    }

    public void OnOkButton()
    {
        notEnoughCoinsImage.SetActive(false);
        HideColorButtons();
    }

    public void OnMainTurretButton()
    {
        tank.ChangeTurret(0);
    }

    public void OnRailgunButton()
    {
        tank.ChangeTurret(1);
    }

    public void OnMachineGunButton()
    {
        tank.ChangeTurret(2);
    }

    public void OnRocketsButton()
    {
        tank.ChangeTurret(3);
    }

    private void OnEnable()
    {
        EventSystem.current.firstSelectedGameObject = backButton.gameObject;
        UpdateUITexts();
        UpdateUIBarsAndUpdateButtons();
        UpdateColorBackgrounds();
        UpdateColorPadlocks();
    }
}
