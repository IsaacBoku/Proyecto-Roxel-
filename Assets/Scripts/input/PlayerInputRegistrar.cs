using UnityEngine;

public class PlayerInputRegistrar : MonoBehaviour
{
    private PlayerInputHadler inputHandler;

    private void Awake()
    {
        inputHandler = GetComponent<PlayerInputHadler>();
        if (inputHandler == null)
        {
            Debug.LogWarning("No se encontró PlayerInputHadler en este GameObject: " + gameObject.name);
            return;
        }

        if (Controller_Menus.Instance != null)
        {
            Controller_Menus.Instance.RegisterInputHandler(inputHandler);
        }
        else
        {
            Debug.LogWarning("No se encontró una instancia de MenuSystems en la escena.");
        }
    }

    private void OnDestroy()
    {
        if (Controller_Menus.Instance != null)
        {
            Controller_Menus.Instance.UnregisterInputHandler();
        }
    }
}
