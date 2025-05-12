using UnityEngine;
[System.Serializable]
public class MenuPanel
{
    [SerializeField] private string panelName;
    [SerializeField] public GameObject panelObject;
    [SerializeField] private Animator animator;
    [SerializeField] private string openAnimationParameter = "Open";
    [SerializeField] private float animationDuration = 1f;

    private bool isOpen = false;

    public float AnimationDuration => animationDuration;

    public void Initialize()
    {
        if (panelObject == null)
        {
            return;
        }

        panelObject.SetActive(false);
        isOpen = false;

        if (animator != null && animator.runtimeAnimatorController == null)
        {
            Debug.LogWarning($"El Animator del panel '{panelName}' no tiene un AnimatorController asignado. Desactivando el Animator para evitar errores.");
            animator = null;
        }
    }

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

    public void Close()
    {
        if (panelObject == null) return;

        if (animator != null && animator.runtimeAnimatorController != null)
        {
            animator.SetBool(openAnimationParameter, false);
        }
        isOpen = false;
    }

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