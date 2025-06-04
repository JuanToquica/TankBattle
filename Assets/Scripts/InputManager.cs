using UnityEngine;

public class InputManager : MonoBehaviour
{
    public PlayerInput playerInput;
    [SerializeField] private PlayerController player;
    [SerializeField] private PlayerAttack playerAttack;
    private void Awake()
    {
        playerInput = new PlayerInput();

        playerInput.Player.CenterTurret.started += ctx => player.ActivateTurretCenteringAndChangeTurretControlToKeys();
        playerInput.Player.Fire.started += ctx => playerAttack.Fire();
        playerInput.Player.FireKeyOnly.started += ctx => playerAttack.Fire();
        playerInput.Player.SwitchTurretControlToMouse.started += ctx => player.SwitchTurretControlToMouse();
    }


    private void OnEnable() => playerInput.Enable();
    private void OnDisable() => playerInput.Disable();
}
