using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using System.Collections;
using Unity.VisualScripting;

public class InputManager : MonoBehaviour
{
    public static InputManager Instance;
    public PlayerInput playerInput;
    public PlayerController player;
    public GarageTankController garageTankController;
    public PlayerAttack playerAttack;
    public Vector2 moveInput;
    public float mouseInput;
    public float turretInput;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            playerInput = GetComponent<PlayerInput>();
            SceneManager.sceneLoaded += OnSceneLoaded;
            LoadActionMaps();          
        }
        else
        {
            Destroy(gameObject);
        }
    }


    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        string currentScene = scene.name;

        if (currentScene == "Battlefield")
        {
            playerInput.actions.FindActionMap("Player").Enable();
            playerInput.actions.FindAction("Pause").Enable();
            playerInput.actions.FindActionMap("GaragePlayer").Disable();
        }
        else if (currentScene == "Garage")
        {
            playerInput.actions.FindActionMap("Player").Disable();
            playerInput.actions.FindActionMap("GaragePlayer").Enable();
            playerInput.actions.FindAction("Pause").Disable();
        }
        else
        {
            playerInput.actions.FindActionMap("Player").Disable();
            playerInput.actions.FindAction("Pause").Disable();
            playerInput.actions.FindActionMap("GaragePlayer").Disable();
        }
    }

    public void RegisterPlayerController(PlayerController controller)
    {
        player = controller;
    }

    public void RegisterPlayerAttack(PlayerAttack playerAttack)
    {
        this.playerAttack = playerAttack;
    }

    public void RegisterGarageTankController(GarageTankController controller)
    {
        garageTankController = controller;
    }

    private void LoadActionMaps()
    {
        foreach (var map in playerInput.actions.actionMaps)
        {
            string json = PlayerPrefs.GetString(map.name);
            if (!string.IsNullOrEmpty(json))
            {
                map.LoadBindingOverridesFromJson(json);
            }
        }
    }

    public void ResetActionMaps()
    {
        foreach (var map in playerInput.actions.actionMaps)
        {
            if (PlayerPrefs.HasKey(map.name))
            {
                PlayerPrefs.DeleteKey(map.name);
            }
            map.RemoveAllBindingOverrides();
        }       
    }

    public void OnMove(InputAction.CallbackContext ctx)
    {
        moveInput = ctx.ReadValue<Vector2>();
    }

    public void OnMoveTurretWithKeys(InputAction.CallbackContext ctx)
    {
        turretInput = ctx.ReadValue<float>();
    }

    public void OnMoveTurretWithMouse(InputAction.CallbackContext ctx)
    {
        mouseInput = ctx.ReadValue<float>();
    }

    public void OnCenterTurret(InputAction.CallbackContext ctx)
    {
        if (ctx.started && player != null)
            player.ActivateTurretCenteringAndChangeTurretControlToKeys();
    }
    public void OnCenterTurretInGarage(InputAction.CallbackContext ctx)
    {
        if (ctx.started && garageTankController != null)
            garageTankController.StartTurretCentering();
    }

    public void OnShoot1(InputAction.CallbackContext ctx)
    {
        if (ctx.started && playerAttack != null)
            playerAttack.Fire();
    }
        
    public void OnShoot2(InputAction.CallbackContext ctx)
    {
        if (ctx.started && playerAttack != null)
            playerAttack.Fire();
    }
        
    public void OnSwitchTurretControlToMouse(InputAction.CallbackContext ctx)
    {
        if (ctx.started && player != null)
            player.SwitchTurretControlToMouse();
    }
        
    public void OnPause(InputAction.CallbackContext ctx)
    {
        if (ctx.started)
            GameManager.instance.PauseAndUnpauseGame();
    }

    public void OnSelectButton()
    {
        if (EventSystem.current.currentSelectedGameObject == null)
        {
            StartCoroutine(SelectFirstNextFrame());
        }
    }
   

    private IEnumerator SelectFirstNextFrame()
    {
        yield return null; //Para que no pase al siguiente boton, ya que se usa el mismo bind para el move hacia abajo
        EventSystem.current.SetSelectedGameObject(EventSystem.current.firstSelectedGameObject);
    }

    private void OnDestroy() => SceneManager.sceneLoaded -= OnSceneLoaded;
    private void OnEnable() => playerInput.enabled = true;
    private void OnDisable()
    {
        if (playerInput != null)
            playerInput.enabled = false;
    }
}
