using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class ControlsSettings : MonoBehaviour
{
    [System.Serializable]
    public class KeyBinding
    {
        public string actionName;
        public InputActionReference actionReference; 
        public Button rebindButton; 
        public TextMeshProUGUI keyText; 
        [HideInInspector] public string bindingPath;
    }

    [SerializeField] private List<KeyBinding> keyBindings; 
    private bool isRebinding = false; 
    private KeyBinding currentBinding;
    private bool actionsInitialized = false; 

    private void Start()
    {
        SetupRebindButtons();
    }

    private void Update()
    {
        if (isRebinding)
        {
            if (Keyboard.current.anyKey.wasPressedThisFrame)
            {
                StartInteractiveRebinding();
            }
        }
    }

    private void SetupRebindButtons()
    {
        foreach (var binding in keyBindings)
        {
            if (binding.rebindButton != null)
            {
                binding.rebindButton.onClick.AddListener(() => StartRebinding(binding));
            }
            UpdateKeyText(binding);
        }
    }

    private void StartRebinding(KeyBinding binding)
    {
        if (isRebinding) return;

        InitializeActions();

        if (!actionsInitialized)
        {
            Debug.LogWarning("No se puede reasignar teclas porque las acciones no están inicializadas. Asegúrate de que PlayerInputHadler esté presente en la escena.");
            return;
        }

        isRebinding = true;
        currentBinding = binding;
        binding.keyText.text = "Presiona una tecla...";
        AudioManager.instance.PlaySFX("ButtonClick");
    }

    private void StartInteractiveRebinding()
    {
        if (currentBinding == null || currentBinding.actionReference == null) return;

        var action = currentBinding.actionReference.action;
        action.Disable();

        var rebindOperation = action.PerformInteractiveRebinding()
            .WithControlsExcluding("Mouse") 
            .OnMatchWaitForAnother(0.1f) 
            .OnComplete(operation =>
            {
                currentBinding.bindingPath = action.bindings[0].effectivePath; 
                UpdateKeyText(currentBinding);
                SaveKeyBindings();
                isRebinding = false;
                currentBinding = null;
                action.Enable(); 
                AudioManager.instance.PlaySFX("ButtonClick");
                operation.Dispose();
            })
            .OnCancel(operation =>
            {
                isRebinding = false;
                currentBinding = null;
                UpdateKeyText(currentBinding);
                action.Enable();
                operation.Dispose();
            })
            .Start();
    }

    private void UpdateKeyText(KeyBinding binding)
    {
        if (binding.keyText != null && binding.actionReference != null)
        {
            var action = binding.actionReference.action;
            if (action.bindings.Count > 0)
            {
                string displayString = InputControlPath.ToHumanReadableString(
                    action.bindings[0].effectivePath,
                    InputControlPath.HumanReadableStringOptions.OmitDevice);
                binding.keyText.text = displayString;
            }
            else
            {
                binding.keyText.text = "Sin asignar";
            }
        }
    }

    private void SaveKeyBindings()
    {
        foreach (var binding in keyBindings)
        {
            if (binding.actionReference != null)
            {
                PlayerPrefs.SetString("KeyBinding_" + binding.actionName, binding.bindingPath);
            }
        }
        PlayerPrefs.Save();
        Debug.Log("Asignaciones de teclas guardadas.");
    }

    private void LoadKeyBindings()
    {

        if (!actionsInitialized) return;

        foreach (var binding in keyBindings)
        {
            if (binding.actionReference == null) continue;

            var action = binding.actionReference.action;
            string savedBinding = PlayerPrefs.GetString("KeyBinding_" + binding.actionName, "");

            if (!string.IsNullOrEmpty(savedBinding))
            {
                binding.bindingPath = savedBinding;
                action.ApplyBindingOverride(new InputBinding { overridePath = savedBinding });
            }
            else if (action.bindings.Count > 0)
            {
                binding.bindingPath = action.bindings[0].effectivePath;
            }

            UpdateKeyText(binding);
        }
        Debug.Log("Asignaciones de teclas cargadas.");
    }
    public void InitializeActions()
    {
        var playerInputHadler = FindAnyObjectByType<PlayerInputHadler>();
        if (playerInputHadler == null)
        {
            Debug.Log("PlayerInputHadler no encontrado. Las acciones del Input System no están inicializadas.");
            actionsInitialized = false;
            return;
        }

        foreach (var binding in keyBindings)
        {
            if (binding.actionReference != null && binding.actionReference.action != null)
            {
                binding.actionReference.action.Enable();
            }
        }

        actionsInitialized = true;
        LoadKeyBindings(); 
        Debug.Log("Acciones del Input System inicializadas.");
    }

    public void ApplyKeyBindings(PlayerInputHadler inputHandler)
    {
        Debug.Log("Bindings aplicados al Input System.");
    }
}
