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
        public string actionName; // Nombre de la acción (por ejemplo, "Jump", "Pause")
        public InputActionReference actionReference; // Referencia a la acción del Input System
        public Button rebindButton; // Botón para reasignar la tecla
        public TextMeshProUGUI keyText; // Texto que muestra la tecla asignada
        [HideInInspector] public string bindingPath; // Ruta del binding (por ejemplo, "/Keyboard/space")
    }

    [SerializeField] private List<KeyBinding> keyBindings; // Lista de asignaciones de teclas
    private bool isRebinding = false; // Estado para detectar si estamos reasignando
    private KeyBinding currentBinding; // Acción que estamos reasignando
    private bool actionsInitialized = false; // Para saber si las acciones ya fueron inicializadas

    private void Start()
    {
        LoadKeyBindings(); // Cargar las asignaciones guardadas
        InitializeActions();
        SetupRebindButtons(); // Configurar los botones de reasignación
    }

    private void Update()
    {
        if (isRebinding)
        {
            // Detectar cualquier tecla presionada para iniciar la reasignación
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

        // Asegurarse de que las acciones estén inicializadas antes de reasignar
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
        action.Disable(); // Desactivar la acción mientras reasignamos

        var rebindOperation = action.PerformInteractiveRebinding()
            .WithControlsExcluding("Mouse") // Excluir el ratón para evitar bindings no deseados
            .OnMatchWaitForAnother(0.1f) // Esperar un breve momento para confirmar la tecla
            .OnComplete(operation =>
            {
                currentBinding.bindingPath = action.bindings[0].effectivePath; // Guardar la nueva ruta del binding
                UpdateKeyText(currentBinding);
                SaveKeyBindings();
                isRebinding = false;
                currentBinding = null;
                action.Enable(); // Reactivar la acción
                AudioManager.instance.PlaySFX("ButtonClick");
                operation.Dispose(); // Liberar la operación
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

        if (!actionsInitialized) return; // No cargar si las acciones no están inicializadas

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

        // Asegurarse de que todas las acciones estén inicializadas
        foreach (var binding in keyBindings)
        {
            if (binding.actionReference != null && binding.actionReference.action != null)
            {
                binding.actionReference.action.Enable();
            }
        }

        actionsInitialized = true;
        LoadKeyBindings(); // Cargar los bindings ahora que las acciones están inicializadas
        Debug.Log("Acciones del Input System inicializadas.");
    }
    // Método para aplicar las asignaciones al PlayerInputHadler (si es necesario)
    public void ApplyKeyBindings(PlayerInputHadler inputHandler)
    {
        // No es necesario aplicar manualmente, ya que las acciones ya están vinculadas al Input System
        Debug.Log("Bindings aplicados al Input System.");
    }
}
