using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GarageUI : MonoBehaviour
{
    [SerializeField] private Button backButton;

    private void Start()
    {
        backButton.onClick.AddListener(OnBackButtonClicked);
    }

    public void OnBackButtonClicked()
    {
        SceneManager.LoadScene("MainMenu");
    }

    private void OnEnable()
    {
        EventSystem.current.firstSelectedGameObject = backButton.gameObject;
    }
}
