using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.InputSystem;

public class SetUpInput_UI_ToolKit : MonoBehaviour
{
    public InputActionAsset inputActions;

    private void Awake()
    {
        var eventSystem = FindObjectOfType<EventSystem>();
        if (eventSystem == null)
        {
            GameObject esObj = new GameObject("EventSystem");
            eventSystem = esObj.AddComponent<EventSystem>();
        }

        var uiInputModule = eventSystem.GetComponent<InputSystemUIInputModule>();
        if (uiInputModule == null)
        {
            uiInputModule = eventSystem.gameObject.AddComponent<InputSystemUIInputModule>();
        }

        if (inputActions != null)
        {
            uiInputModule.actionsAsset = inputActions;
            uiInputModule.move = InputActionReference.Create(inputActions.FindAction("UI/Navigate"));
            uiInputModule.submit = InputActionReference.Create(inputActions.FindAction("UI/Submit"));
            uiInputModule.cancel = InputActionReference.Create(inputActions.FindAction("UI/Cancel"));
            uiInputModule.point = InputActionReference.Create(inputActions.FindAction("UI/Point"));
            uiInputModule.leftClick = InputActionReference.Create(inputActions.FindAction("UI/Click"));
        }
    }
}
