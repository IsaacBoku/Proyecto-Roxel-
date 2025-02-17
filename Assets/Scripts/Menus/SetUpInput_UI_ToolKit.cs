using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.InputSystem;
using System.Collections;
using UnityEngine.UI;

public class SetUpInput_UI_ToolKit : MonoBehaviour
{
    public InputActionAsset inputActions;
    [SerializeField] protected Selectable _firstSelected;

    protected Selectable _lastSelected;

    [Header("Controls")]
    [SerializeField] protected InputActionReference _navigateReference;

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
    public virtual void OnEnable()
    {
        _navigateReference.action.performed += OnNavigate;

        StartCoroutine(SelectAfterDelay());
    }

    protected virtual IEnumerator SelectAfterDelay()
    {
        yield return null;

        EventSystem.current.SetSelectedGameObject(_firstSelected.gameObject);
    }
    private void OnDisable()
    {
        _navigateReference.action.performed -= OnNavigate;
    }
    protected virtual void OnNavigate(InputAction.CallbackContext context)
    {
        if(EventSystem.current.currentSelectedGameObject == null && _lastSelected != null)
        {
            EventSystem.current.SetSelectedGameObject(_lastSelected.gameObject);
        }
    }
}
