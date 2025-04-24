using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using DG.Tweening;
using UnityEngine.Events;

public class MenuEventSystemHadler : MonoBehaviour, ISelectHandler, IDeselectHandler
{
    [Header("Reference")]
    public List<Selectable> Selectables = new List<Selectable>();
    [SerializeField] protected Selectable _firstSelected;

    [Header("Animations")]
    [SerializeField] protected float _selectedAnimationScale = 1.1f;
    [SerializeField] protected float _scaleDuration = 0.25f;
    [SerializeField] protected List<GameObject> _animationExclusions = new List<GameObject>();

    [Header("Sounds")]
    [SerializeField] protected UnityEvent SoundEvent;

    protected Dictionary<Selectable, Vector3> _scales = new Dictionary<Selectable, Vector3>();
    protected Selectable _lastSelected;
    protected Tween _scaleUpTween;
    protected Tween _scaleDownTween;

    public void Awake()
    {
        foreach (var selectable in Selectables)
        {
            AddSelectionListeners(selectable);
            _scales.Add(selectable, selectable.transform.localScale);
        }
    }

    public virtual void OnEnable()
    {
        for (int i = 0; i < Selectables.Count; i++)
        {
            Selectables[i].transform.localScale = _scales[Selectables[i]];
        }

        StartCoroutine(SelectAfterDelay());
    }

    protected virtual IEnumerator SelectAfterDelay()
    {
        yield return null;
        if (_firstSelected != null)
        {
            EventSystem.current.SetSelectedGameObject(_firstSelected.gameObject);
            Debug.Log("Elemento inicial seleccionado: " + _firstSelected.name);
        }
    }

    public virtual void OnDisable()
    {
        _scaleUpTween?.Kill(true);
        _scaleDownTween?.Kill(true);
        Debug.Log("MenuEventSystemHandler desactivado");
    }

    protected virtual void AddSelectionListeners(Selectable selectable)
    {
        EventTrigger trigger = selectable.gameObject.GetComponent<EventTrigger>();
        if (trigger == null)
        {
            trigger = selectable.gameObject.AddComponent<EventTrigger>();
        }

        EventTrigger.Entry SelectEntry = new EventTrigger.Entry
        {
            eventID = EventTriggerType.Select
        };
        SelectEntry.callback.AddListener(OnSelect);
        trigger.triggers.Add(SelectEntry);

        EventTrigger.Entry DeselectEntry = new EventTrigger.Entry
        {
            eventID = EventTriggerType.Deselect
        };
        DeselectEntry.callback.AddListener(OnDeselect);
        trigger.triggers.Add(DeselectEntry);

        EventTrigger.Entry PointerEnter = new EventTrigger.Entry
        {
            eventID = EventTriggerType.PointerEnter
        };
        PointerEnter.callback.AddListener(OnPointerEnter);
        trigger.triggers.Add(PointerEnter);

        EventTrigger.Entry PointerExit = new EventTrigger.Entry
        {
            eventID = EventTriggerType.PointerExit
        };
        PointerExit.callback.AddListener(OnPointerExit);
        trigger.triggers.Add(PointerExit);
    }

    public void OnSelect(BaseEventData eventData)
    {
        SoundEvent?.Invoke();
        _lastSelected = eventData.selectedObject.GetComponent<Selectable>();
        if (_lastSelected != null)
        {
            Debug.Log("Elemento seleccionado: " + _lastSelected.name);
        }

        if (_animationExclusions.Contains(eventData.selectedObject))
        {
            return;
        }

        Vector3 newScale = eventData.selectedObject.transform.localScale * _selectedAnimationScale;
        _scaleUpTween = eventData.selectedObject.transform.DOScale(newScale, _scaleDuration);
    }

    public void OnDeselect(BaseEventData eventData)
    {
        if (_animationExclusions.Contains(eventData.selectedObject))
        {
            return;
        }

        Selectable sel = eventData.selectedObject.GetComponent<Selectable>();
        if (sel != null)
        {
            _scaleDownTween = eventData.selectedObject.transform.DOScale(_scales[sel], _scaleDuration);
            Debug.Log("Elemento deseleccionado: " + sel.name);
        }
    }

    public void OnPointerEnter(BaseEventData eventData)
    {
        PointerEventData pointerEventData = eventData as PointerEventData;
        if (pointerEventData != null)
        {
            Selectable sel = pointerEventData.pointerEnter.GetComponentInParent<Selectable>();
            if (sel == null)
            {
                sel = pointerEventData.pointerEnter.GetComponentInChildren<Selectable>();
            }

            if (sel != null)
            {
                pointerEventData.selectedObject = sel.gameObject;
                EventSystem.current.SetSelectedGameObject(sel.gameObject);
                Debug.Log("Ratón entró en: " + sel.name);
            }
        }
    }

    public void OnPointerExit(BaseEventData eventData)
    {
        PointerEventData pointerEventData = eventData as PointerEventData;
        if (pointerEventData != null)
        {
            pointerEventData.selectedObject = null;
            Debug.Log("Ratón salió de un elemento");
        }
    }
}