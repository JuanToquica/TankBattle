using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GarageUI : MonoBehaviour
{
    [SerializeField] private GarageTankController tank;
    [SerializeField] private Button backButton;
    [SerializeField] private Button redPaintButton;
    [SerializeField] private Button bluePaintButton;
    [SerializeField] private Button purplePaintButton;
    [SerializeField] private Button yellowPaintButton;
    [SerializeField] private Button greenPaintButton;

    private void Start()
    {
        backButton.onClick.AddListener(OnBackButtonClicked);
        redPaintButton.onClick.AddListener(OnRedPaintButton);
        bluePaintButton.onClick.AddListener(OnBluePaintButton);
        purplePaintButton.onClick.AddListener(OnPurplePaintButton);
        yellowPaintButton.onClick.AddListener(OnYellowPaintButton);
        greenPaintButton.onClick.AddListener(OnGreenPaintButton);
    }

    public void OnBackButtonClicked()
    {
        SceneManager.LoadScene("MainMenu");
    }

    public void OnRedPaintButton()
    {
        tank.ChangeTankMaterial(0);
    }

    public void OnBluePaintButton()
    {
        tank.ChangeTankMaterial(1);
    }

    public void OnPurplePaintButton()
    {
        tank.ChangeTankMaterial(2);
    }

    public void OnYellowPaintButton()
    {
        tank.ChangeTankMaterial(3);
    }

    public void OnGreenPaintButton()
    {
        tank.ChangeTankMaterial(4);
    }

    private void OnEnable()
    {
        EventSystem.current.firstSelectedGameObject = backButton.gameObject;
    }
}
