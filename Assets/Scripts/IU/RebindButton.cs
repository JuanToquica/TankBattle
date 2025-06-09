using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class RebindButton : MonoBehaviour
{
    //public string actionName;
    //public int bindingIndex;
    //public TextMeshProUGUI displayText;

    //private Button button;
    //private InputAction actionToRebind;

    //private void Awake()
    //{
    //    button = GetComponent<Button>();
    //    button.onClick.AddListener(StartRebind);

    //    actionToRebind = InputManager.Instance.playerInput.FindAction(actionName);
    //    if (actionToRebind == null)
    //    {
    //        Debug.LogError($"No se encontró la acción '{actionName}' en el InputActions.");
    //        return;
    //    }

    //    UpdateBindingDisplay();
    //}

    //void StartRebind()
    //{
    //    displayText.text = "Presiona una tecla...";

    //    actionToRebind.PerformInteractiveRebinding(bindingIndex)
    //        .WithControlsExcluding("Mouse")
    //        .OnComplete(operation =>
    //        {
    //            operation.Dispose();
    //            UpdateBindingDisplay();
    //            SaveRebinds();
    //        })
    //        .Start();
    //}

    //void UpdateBindingDisplay()
    //{
    //    var binding = actionToRebind.bindings[bindingIndex];
    //    displayText.text = InputControlPath.ToHumanReadableString(
    //        binding.effectivePath,
    //        InputControlPath.HumanReadableStringOptions.OmitDevice
    //    );
    //}

    //void SaveRebinds()
    //{
    //    var json = InputManager.Instance.playerInput.SaveBindingOverridesAsJson();
    //    PlayerPrefs.SetString("Bindings", json);
    //}
}
