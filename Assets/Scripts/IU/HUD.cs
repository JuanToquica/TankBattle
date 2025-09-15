using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Threading;

public class HUD : MonoBehaviour
{    
    [SerializeField] private TextMeshProUGUI chronometer;
    [SerializeField] private TextMeshProUGUI playerScoreText;
    [SerializeField] private TextMeshProUGUI enemyScoreText;
    [SerializeField] private Image playerScoreBar;
    [SerializeField] private Image enemyScoreBar;
    [SerializeField] private Image speedPowerUpImage;
    [SerializeField] private Image rechargingPowerUpImage;
    [SerializeField] private Image weaponPowerUpImage;
    [SerializeField] private CanvasGroup messagePanel;
    [SerializeField] private GameObject[] messageGameobjects;
    [SerializeField] private float messageDuration;
    [SerializeField] private float panelDisolveSpeed;
    [SerializeField] private PlayerAttack playerAttack;
    private int minutes, seconds;
    public bool speedPowerUpActive;
    public bool rechargingPowerUpActive;
    public float speedPowerUpTimer;
    public float rechargingPowerUpTimer;
    private float rechargingPowerUpDuration;
    private float speedPowerUpDuration;
    private GameObject weaponPowerUpParent;
    private GameObject rechargingPowerUpParent;
    private GameObject speedPowerUpParent;


    private void Start()
    {
        playerScoreBar.fillAmount = 0;
        enemyScoreBar.fillAmount = 0;
        weaponPowerUpParent = weaponPowerUpImage.transform.parent.gameObject;
        rechargingPowerUpParent = rechargingPowerUpImage.transform.parent.gameObject;
        speedPowerUpParent = speedPowerUpImage.transform.parent.gameObject;
        OnSpeedPowerUpDeactivated();
        OnRechargingPowerUpDeactivated();
        weaponPowerUpImage.transform.parent.gameObject.SetActive(false);
    }

    private void Update()
    {
        playerScoreText.text = string.Format("{0}", GameManager.instance.playerScore);
        enemyScoreText.text = string.Format("{0}", GameManager.instance.enemyScore);
        Chronometer();
        SetScoreBars();

        if (playerAttack.currentWeapon != Weapons.mainTurret)
            SetWeaponPowerUpFill();
        else
            if (weaponPowerUpParent.activeSelf) weaponPowerUpParent.SetActive(false);

        if (rechargingPowerUpActive)
            SetRechargingPowerUpFill();
        if (speedPowerUpActive)
            SetSpeedPowerUpFill();
    }

    private void SetScoreBars()
    {
        float targetPlayerFillAmount = (float)GameManager.instance.playerScore / (float)GameManager.instance.playerMaxScore;
        float targetEnemyFillAmount = (float)GameManager.instance.enemyScore / (float)GameManager.instance.enemyMaxScore;

        if (Mathf.Abs(playerScoreBar.fillAmount - targetPlayerFillAmount) < 0.001f)
            playerScoreBar.fillAmount = targetPlayerFillAmount;
        else
            playerScoreBar.fillAmount = Mathf.Lerp(playerScoreBar.fillAmount, targetPlayerFillAmount, Time.deltaTime * 15);

        if (Mathf.Abs(enemyScoreBar.fillAmount - targetEnemyFillAmount) < 0.001f)
            enemyScoreBar.fillAmount = targetEnemyFillAmount;
        else
            enemyScoreBar.fillAmount = Mathf.Lerp(enemyScoreBar.fillAmount, targetEnemyFillAmount, Time.deltaTime * 15);
    }

    private void Chronometer()
    {
        GameManager.instance.gameTime = Mathf.Clamp(GameManager.instance.gameTime - Time.deltaTime, 0, 600);
        seconds = Mathf.FloorToInt(GameManager.instance.gameTime % 60);
        minutes = Mathf.FloorToInt(GameManager.instance.gameTime / 60);
        chronometer.text = string.Format("{0:00}:{1:00}", minutes, seconds);
        if (GameManager.instance.gameTime <= 0)
        {
            GameManager.instance.EndGame();
            gameObject.SetActive(false);
        }     
    }

    public void ShowMessage(int message)
    {
        if (message == 1)
            StartCoroutine(ShowMessageCoroutine(message));
        if (message == 2)
            StartCoroutine(ShowMessageCoroutine(message));
        if (message == 3)
            StartCoroutine(ShowMessageCoroutine(message));
    }

    private IEnumerator ShowMessageCoroutine(int message)
    {
        messagePanel.gameObject.SetActive(true);
        for (int i = 0; i<3; i++) //Activar solo el mensaje necesitado
        {
            if (i == message - 1)
            {
                messageGameobjects[i].SetActive(true);
                continue;
            }             
            messageGameobjects[i].SetActive(false);

        }
        messagePanel.alpha = 1;
        yield return new WaitForSeconds(messageDuration);

        while (messagePanel.alpha > 0)
        {
            messagePanel.alpha -= Time.deltaTime * panelDisolveSpeed;
            yield return null;
        }
        messagePanel.alpha = 0;
        messagePanel.gameObject.SetActive(false);
    }

    private void SetSpeedPowerUpFill()
    {
        speedPowerUpImage.fillAmount = 1 - (speedPowerUpTimer / speedPowerUpDuration);
        speedPowerUpTimer -= Time.deltaTime;
    }

    private void SetRechargingPowerUpFill()
    {
        rechargingPowerUpImage.fillAmount = 1 - (rechargingPowerUpTimer / rechargingPowerUpDuration);
        rechargingPowerUpTimer -= Time.deltaTime;
    }

    private void SetWeaponPowerUpFill()
    {
        if (!weaponPowerUpParent.activeSelf) weaponPowerUpParent.SetActive(true);
        if (playerAttack.currentWeapon == Weapons.railGun)
            weaponPowerUpImage.fillAmount = (playerAttack.shotsFired / playerAttack.weaponsSettings.railgunAmmo);
        if (playerAttack.currentWeapon == Weapons.machineGun)
            weaponPowerUpImage.fillAmount = (playerAttack.shotsFired / playerAttack.weaponsSettings.machineGunAmmo);
        if (playerAttack.currentWeapon == Weapons.rocket)
            weaponPowerUpImage.fillAmount = (playerAttack.shotsFired / playerAttack.weaponsSettings.rocketAmmo);
    }

    public void OnRechargingPowerUp(float duration)
    {      
        rechargingPowerUpActive = true;
        rechargingPowerUpDuration = duration;
        rechargingPowerUpTimer = rechargingPowerUpDuration;
        rechargingPowerUpParent.SetActive(true);
    }   

    public void OnSpeedPowerUp(float duration)
    {       
        speedPowerUpActive = true;
        speedPowerUpDuration = duration;
        speedPowerUpTimer = speedPowerUpDuration;
        speedPowerUpParent.SetActive(true);
    }

    public void OnRechargingPowerUpDeactivated()
    {
        rechargingPowerUpActive = false;
        rechargingPowerUpParent.SetActive(false);
    }

    public void OnSpeedPowerUpDeactivated()
    {
        speedPowerUpActive = false;
        speedPowerUpParent.SetActive(false);
    }
}
