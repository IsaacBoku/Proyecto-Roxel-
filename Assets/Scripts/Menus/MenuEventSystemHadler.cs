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

    protected Dictionary<Selectable, Vector3> _scales = new Dictionary<Selectable, Vector3>();
    protected Selectable _lastSelected;
    protected Tween _scaleUpTween;
    protected Tween _scaleDownTween;
    private bool isRestoringSelection = false;

    public void Awake()
    {
        foreach (var selectable in Selectables)
        {
            AddSelectionListeners(selectable);
            _scales.Add(selectable, selectable.transform.localScale);
        }
    }

    private void Update()
    {
        MaintainUISelection();
    }

    private void MaintainUISelection()
    {
        if (EventSystem.current.currentSelectedGameObject == null && Selectables.Count > 0)
        {
            Selectable targetSelectable = _lastSelected != null && Selectables.Contains(_lastSelected) ? _lastSelected : _firstSelected;
            if (targetSelectable == null && Selectables.Count > 0)
            {
                targetSelectable = Selectables[0];
            }

            if (targetSelectable != null)
            {
                isRestoringSelection = true;
                EventSystem.current.SetSelectedGameObject(targetSelectable.gameObject);
                isRestoringSelection = false;
                Debug.Log("Selección restaurada a: " + targetSelectable.name);
            }
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
        yield return new WaitForEndOfFrame();
        if (_firstSelected != null)
        {
            isRestoringSelection = true;
            EventSystem.current.SetSelectedGameObject(_firstSelected.gameObject);
            _lastSelected = _firstSelected;
            isRestoringSelection = false;
            Debug.Log("Elemento inicial seleccionado: " + _firstSelected.name);
        }
        else if (Selectables.Count > 0)
        {
            isRestoringSelection = true;
            EventSystem.current.SetSelectedGameObject(Selectables[0].gameObject);
            _lastSelected = Selectables[0];
            isRestoringSelection = false;
            Debug.Log("Elemento inicial seleccionado (fallback): " + Selectables[0].name);
        }
        else
        {
            Debug.LogWarning("No hay elementos seleccionables para seleccionar inicialmente");
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
        _lastSelected = eventData.selectedObject.GetComponent<Selectable>();
        if (_lastSelected != null)
        {
            Debug.Log("Elemento seleccionado: " + _lastSelected.name + ", Restauración automática: " + isRestoringSelection);
        }

        // Reproducir sonido solo si no es una restauración automática
        if (!isRestoringSelection)
        {
            AudioManager.instance.PlaySFX("ButtonHover");
        }

        if (_animationExclusions.Contains(eventData.selectedObject) || isRestoringSelection)
        {
            return; // No aplicar animación si es una restauración automática
        }

        // Cancelar animaciones previas
        _scaleUpTween?.Kill(true);
        _scaleDownTween?.Kill(true);

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
            // Cancelar animaciones previas
            _scaleDownTween?.Kill(true);
            _scaleUpTween?.Kill(true);

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
            Debug.Log("Ratón salió de un elemento, manteniendo selección actual");
        }
    }
}