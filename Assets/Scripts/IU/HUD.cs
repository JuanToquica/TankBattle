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
    [SerializeField] private CanvasGroup objectiveMessagePanel;
    [SerializeField] private CanvasGroup playerScoresMessagePanel;
    [SerializeField] private CanvasGroup enemyScoresMessagePanel;
    [SerializeField] private float messageDuration;
    [SerializeField] private float panelDisolveSpeed;
    [SerializeField] private PlayerAttack playerAttack;
    private int minutes, seconds;
    private bool speedPowerUpActive;
    private bool rechargingPowerUpActive;
    private float speedPowerUpTimer;
    private float rechargingPowerUpTimer;
    private float rechargingPowerUpDuration;
    private float speedPowerUpDuration;


    private void Start()
    {
        playerScoreBar.fillAmount = 0;
        enemyScoreBar.fillAmount = 0;
        OnSpeedPowerUpDeactivated();
        OnRechargingPowerUpDeactivated();
        weaponPowerUpImage.gameObject.SetActive(false);
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
            if (weaponPowerUpImage.gameObject.activeSelf) weaponPowerUpImage.gameObject.SetActive(false);

        if (rechargingPowerUpActive)
            SetRechargingPowerUpFill();
        if (speedPowerUpActive)
            SetSpeedPowerUpFill();
    }

    private void SetSpeedPowerUpFill()
    {
        speedPowerUpImage.fillAmount = 1 - (speedPowerUpTimer / speedPowerUpDuration);
        speedPowerUpTimer += Time.deltaTime;
    }

    private void SetRechargingPowerUpFill()
    {
        rechargingPowerUpImage.fillAmount = 1 - (rechargingPowerUpTimer / rechargingPowerUpDuration);
        rechargingPowerUpTimer += Time.deltaTime;
    }

    private void SetWeaponPowerUpFill()
    {
        if (!weaponPowerUpImage.gameObject.activeSelf) weaponPowerUpImage.gameObject.SetActive(true);
        if (playerAttack.currentWeapon == Weapons.railGun)
            weaponPowerUpImage.fillAmount = 1 - (playerAttack.shotsFired / playerAttack.weaponsSettings.railgunAmmo);
        if (playerAttack.currentWeapon == Weapons.machineGun)
            weaponPowerUpImage.fillAmount = 1 - (playerAttack.shotsFired / playerAttack.weaponsSettings.machineGunAmmo);
        if (playerAttack.currentWeapon == Weapons.rocket)
            weaponPowerUpImage.fillAmount = 1 - (playerAttack.shotsFired / playerAttack.weaponsSettings.rocketAmmo);
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
        GameManager.instance.time = Mathf.Clamp(GameManager.instance.time - Time.deltaTime, 0, 600);
        seconds = Mathf.FloorToInt(GameManager.instance.time % 60);
        minutes = Mathf.FloorToInt(GameManager.instance.time / 60);
        chronometer.text = string.Format("{0:00}:{1:00}", minutes, seconds);
        if (GameManager.instance.time <= 0)
        {
            GameManager.instance.EndGame();
            gameObject.SetActive(false);
        }     
    }

    public void ShowMessage(int message)
    {
        if (message == 1)
            StartCoroutine(ShowMessageCoroutine(objectiveMessagePanel));
        if (message == 2)
            StartCoroutine(ShowMessageCoroutine(playerScoresMessagePanel));
        if (message == 3)
            StartCoroutine(ShowMessageCoroutine(enemyScoresMessagePanel));
    }

    private IEnumerator ShowMessageCoroutine(CanvasGroup panel)
    {
        panel.gameObject.SetActive(true);
        panel.alpha = 1;
        yield return new WaitForSeconds(messageDuration);

        while (panel.alpha > 0)
        {           
            panel.alpha -= Time.deltaTime * panelDisolveSpeed;
            yield return null;
        }
        panel.alpha = 0;
        panel.gameObject.SetActive(false);
    }

    public void OnRechargingPowerUp(float duration)
    {
        rechargingPowerUpTimer = 0;
        rechargingPowerUpActive = true;
        rechargingPowerUpDuration = duration;
        rechargingPowerUpImage.gameObject.SetActive(true);
    }

    public void OnRechargingPowerUpDeactivated()
    {
        rechargingPowerUpActive = false;
        rechargingPowerUpImage.gameObject.SetActive(false);
    }

    public void OnSpeedPowerUp(float duration)
    {
        speedPowerUpTimer = 0;
        speedPowerUpActive = true;
        speedPowerUpDuration = duration;
        speedPowerUpImage.gameObject.SetActive(true);
    }

    public void OnSpeedPowerUpDeactivated()
    {
        speedPowerUpActive = false;
        speedPowerUpImage.gameObject.SetActive(false);
    }
}
