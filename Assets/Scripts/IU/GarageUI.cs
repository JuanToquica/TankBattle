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

    [Header ("Text References")]  
    [SerializeField] private TextMeshProUGUI coinsAmount;
    [SerializeField] private TextMeshProUGUI ArmorDamageValue;
    [SerializeField] private TextMeshProUGUI mainGunDamageValue;
    [SerializeField] private TextMeshProUGUI railgunDamageValue;
    [SerializeField] private TextMeshProUGUI machinegunDamageValue;
    [SerializeField] private TextMeshProUGUI rocketDamageValue;

    [Header ("Image References")]
    [SerializeField] private GameObject equippedImage;
    [SerializeField] private GameObject notEnoughCoinsImage;

    [Header ("Other Referernces")]
    [SerializeField] private ChangeTankPaint tank;
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

        upgradeArmorButton.onClick.AddListener(OnUpgradeArmorButton);
        upgradeMainGunDamageButton.onClick.AddListener(OnUpgradeMainGunDamageButton);
        upgradeRailgunDamageButton.onClick.AddListener(OnUpgradeRailgunDamageButton);
        upgradeMachinegunDamageButton.onClick.AddListener(OnUpgradeMachineGunDamageButton);
        upgradeRocketDamageButton.onClick.AddListener(OnUpgradeRocketDamageButton);

        buyButton.gameObject.SetActive(false);
        equippedImage.SetActive(false);
        equipButton.gameObject.SetActive(false);

        UpdateUITexts();
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
    public void OnBackButtonClicked()
    {
        SceneManager.LoadScene("MainMenu");
    }

    public void OnRedPaintButton()
    {
        tank.LoadTankMaterial(0);
        currentColorSelected = 0;
        UpdateUIButtons();
    }

    public void OnBluePaintButton()
    {
        tank.LoadTankMaterial(1);
        currentColorSelected = 1;
        UpdateUIButtons();
    }

    public void OnPurplePaintButton()
    {
        tank.LoadTankMaterial(2);
        currentColorSelected = 2;
        UpdateUIButtons();
    }

    public void OnYellowPaintButton()
    {
        tank.LoadTankMaterial(3);
        currentColorSelected = 3;
        UpdateUIButtons();
    }

    public void OnGreenPaintButton()
    {
        tank.LoadTankMaterial(4);
        currentColorSelected = 4;
        UpdateUIButtons();
    }

    public void OnBuyButton()
    {
        if (DataManager.Instance.TryPurchaseColor(currentColorSelected, materialsData.cost))
            DataManager.Instance.SetSelectedTankColor(currentColorSelected);
        UpdateUIButtons();
    }

    public void OnEquipButton()
    {
        DataManager.Instance.SetSelectedTankColor(currentColorSelected);
        UpdateUIButtons();
    }

    public void OnUpgradeArmorButton()
    {
        DataManager.Instance.UpgradeArmorStrength();
        UpdateUITexts();
    }

    public void OnUpgradeMainGunDamageButton()
    {
        DataManager.Instance.UpgradeMainTurret();
        UpdateUITexts();
    }

    public void OnUpgradeRailgunDamageButton()
    {
        DataManager.Instance.UpgradeRailgun();
        UpdateUITexts();
    }

    public void OnUpgradeMachineGunDamageButton()
    {
        DataManager.Instance.UpgradeMachineGun();
        UpdateUITexts();
    }

    public void OnUpgradeRocketDamageButton()
    {
        DataManager.Instance.UpgradeRocketLauncher();
        UpdateUITexts();
    }

    public void OnOkButton()
    {
        notEnoughCoinsImage.SetActive(false);
    }

    private void OnEnable()
    {
        EventSystem.current.firstSelectedGameObject = backButton.gameObject;
        UpdateUITexts();
    }
}
