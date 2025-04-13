using UnityEngine;
[System.Serializable]
public class MenuPanel
{
    [SerializeField] private string panelName; // Nombre del panel (para depuraci�n)
    [SerializeField] private GameObject panelObject; // GameObject del panel
    [SerializeField] private Animator animator; // Animador del panel (si tiene animaciones)
    [SerializeField] private string openAnimationParameter = "Open"; // Nombre del par�metro de animaci�n para abrir
    [SerializeField] private float animationDuration = 1f; // Duraci�n de la animaci�n

    private bool isOpen = false;

    public float AnimationDuration => animationDuration;

    // Inicializar el panel
    public void Initialize()
    {
        if (panelObject != null)
        {
            panelObject.SetActive(false);
            isOpen = false;

            // Validar el Animator al inicializar
            if (animator != null && animator.runtimeAnimatorController == null)
            {
                Debug.LogWarning($"El Animator del panel '{panelName}' no tiene un AnimatorController asignado. Desactivando el Animator para evitar errores.");
                animator = null; // Desactivar el Animator si no tiene controlador
            }
        }
    }

    // Abrir el panel
    public void Open()
    {
        if (panelObject == null) return;

        panelObject.SetActive(true);
        if (animator != null && animator.runtimeAnimatorController != null)
        {
            animator.SetBool(openAnimationParameter, true);
        }
        isOpen = true;
    }

    // Cerrar el panel
    public void Close()
    {
        if (panelObject == null) return;

        if (animator != null && animator.runtimeAnimatorController != null)
        {
            animator.SetBool(openAnimationParameter, false);
        }
        isOpen = false;
    }

    // Cerrar el panel inmediatamente (sin animaci�n)
    public void CloseImmediate()
    {
        if (panelObject == null) return;

        panelObject.SetActive(false);
        if (animator != null && animator.runtimeAnimatorController != null)
        {
            animator.SetBool(openAnimationParameter, false);
        }
        isOpen = false;
    }

    public bool IsOpen => isOpen;
}