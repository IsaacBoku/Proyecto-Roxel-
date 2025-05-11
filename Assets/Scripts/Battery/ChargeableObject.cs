using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChargeableObject : InteractableBase
{
    public enum TargetType
    {
        Door,
        Laser
    }

    [System.Serializable]
    public class TargetEntry
    {
        public TargetType type;
        public GameObject targetObject;
        [HideInInspector] public IActivable activable;
        public Transform[] waypoints;
        [HideInInspector] public LineRenderer lineRenderer;
        [SerializeField] public int sortingOrder = 10;
        [SerializeField] public Transform lineEndPoint;
    }

    [SerializeField]
    private List<TargetEntry> targets = new List<TargetEntry>();

    [SerializeField]
    private float requiredEnergy = 50f;

    [SerializeField]
    private float chargeDuration = 2f;

    [SerializeField]
    private Material mat;
    [SerializeField] private Transform lineStartPoint; // Nuevo: Punto de origen de las líneas
    [SerializeField] private Color inactiveLineColor = Color.gray; // Nuevo: Color cuando no está activo
    [SerializeField] private Color chargingLineColor = Color.yellow; // Nuevo: Color durante la carga
    [SerializeField] private Color activeLineColor = Color.green; // Nuevo: Color cuando está activo
    [SerializeField] private string lineRendererLayer = "LineRenderer"; // Nuevo: Layer para el LineRenderer
    [SerializeField] private int lineRendererSortingOrder = 10; // Nuevo: Sorting Order para 2D

    public bool isCharging = false;

    [SerializeField]
    private Slider slider_Energy;

    protected override void Start()
    {
        base.Start();
        foreach (var target in targets)
        {
            if (target.targetObject == null)
            {
                Debug.LogWarning($"ChargeableObject '{gameObject.name}': Un objetivo en la lista 'Targets' no tiene GameObject asignado.");
                continue;
            }

            switch (target.type)
            {
                case TargetType.Door:
                    target.activable = target.targetObject.GetComponent<Door_Mechanic>();
                    break;
                case TargetType.Laser:
                    target.activable = target.targetObject.GetComponent<Laser_Mechanic>();
                    break;
            }

            if (target.activable == null)
            {
                Debug.LogWarning($"ChargeableObject '{gameObject.name}': El objetivo '{target.targetObject.name}' no tiene un componente {target.type} válido.");
                continue;
            }

            target.activable.Toggle(isActive);
            target.activable.SetIgnoreTrigger(true);

            SetupLineRenderer(target);
        }

        /*if (mat != null)
        {
            mat.SetFloat("_Progress", isActive ? 1f : 0f);
        }*/

    }

    protected override void Update()
    {
        base.Update();

        foreach (var target in targets)
        {
            if (target.lineRenderer != null)
            {
                UpdateLineRenderer(target);
            }
        }
    }
    public override void Interact()
    {
        Debug.Log($"ChargeableObject '{gameObject.name}': Necesitas una batería para interactuar.");
    }

    public void StartCharging(BatteryController battery)
    {
        if (isCharging || isActive)
        {
            Debug.Log($"ChargeableObject '{gameObject.name}': Ya está cargando o activo.");
            return;
        }

        if (battery == null)
        {
            Debug.Log($"ChargeableObject '{gameObject.name}': No se puede interactuar sin una batería.");
            //return;
        }

        StartCoroutine(ChargeProgressively(battery));
    }

    private IEnumerator ChargeProgressively(BatteryController battery)
    {
        isCharging = true;
        float elapsedTime = 0f;
        float energyToConsume = requiredEnergy;
        float energyPerSecond = energyToConsume / chargeDuration;
        SpriteRenderer sr = GetComponent<SpriteRenderer>();

        // Activar objetivos y actualizar líneas
        foreach (var target in targets)
        {
            if (target.activable != null)
            {
                target.activable.Toggle(true);
            }
            if (target.lineRenderer != null)
            {
                target.lineRenderer.startColor = chargingLineColor;
                target.lineRenderer.endColor = chargingLineColor;
            }
        }


        UpdateVisuals(true);

        // Ensure the slider is initialized
        if (slider_Energy != null)
        {
            slider_Energy.value = 0f; // Start at 0
            slider_Energy.maxValue = 1f; // Set max value to 1 for progress (0 to 1)
        }

        while (elapsedTime < chargeDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / chargeDuration;
            float energyThisFrame = energyPerSecond * Time.deltaTime;

            if (battery.energyAmounts < energyThisFrame)
            {
                Debug.Log($"{gameObject.name} no puede activarse: Energía insuficiente ({battery.energyAmounts}/{energyThisFrame} requerida en este frame).");
                isCharging = false;
                foreach (var target in targets)
                {
                    if (target.activable != null)
                    {
                        target.activable.Toggle(false);
                    }
                    if (target.lineRenderer != null)
                    {
                        target.lineRenderer.startColor = inactiveLineColor;
                        target.lineRenderer.endColor = inactiveLineColor;
                    }
                }
                UpdateVisuals(false);

                // Reset slider on failure
                if (slider_Energy != null)
                {
                    slider_Energy.value = 0f;
                }
                yield break;
            }

            battery.energyAmounts -= energyThisFrame;
            battery.energyAmounts = Mathf.Clamp(battery.energyAmounts, 0f, battery.maxEnergy);
            //Debug.Log($"Consumiendo energía progresivamente: {energyThisFrame}. Energía restante: {battery.energyAmounts}");

            // Update slider to reflect charging progress
            if (slider_Energy != null)
            {
                slider_Energy.value = t; // Update slider value (0 to 1)
            }
            if (mat != null)
            {
                mat.SetFloat("_Progress", t);
            }

            foreach (var target in targets)
            {
                if (target.lineRenderer != null)
                {
                    Color lerpedColor = Color.Lerp(inactiveLineColor, chargingLineColor, t);
                    target.lineRenderer.startColor = lerpedColor;
                    target.lineRenderer.endColor = lerpedColor;
                }
            }

            yield return null;
        }

        isActive = true;
        isCharging = false;
        if (mat != null)
        {
            mat.SetFloat("_Progress", 1f);
        }
        // Set slider to max when charging completes
        if (slider_Energy != null)
        {
            slider_Energy.value = 1f;
        }

        foreach (var target in targets)
        {
            if (target.lineRenderer != null)
            {
                target.lineRenderer.startColor = activeLineColor;
                target.lineRenderer.endColor = activeLineColor;
            }
        }
        Debug.Log($"{gameObject.name} ha terminado de cargarse.");
    }

    public void Deactivate()
    {
        if (isActive)
        {
            isActive = false;
            foreach (var target in targets)
            {
                if (target.activable != null)
                {
                    target.activable.Toggle(false);
                }
                if (target.lineRenderer != null)
                {
                    target.lineRenderer.startColor = inactiveLineColor;
                    target.lineRenderer.endColor = inactiveLineColor;
                }
            }
            UpdateVisuals(false);
            if (mat != null)
            {
                mat.SetFloat("_Progress", 0f);
            }
            Debug.Log($"{gameObject.name} ha sido desactivado.");
        }
    }
    private void SetupLineRenderer(TargetEntry target)
    {
        GameObject lineObj = new GameObject($"LineTo_{target.targetObject.name}");
        lineObj.transform.SetParent(transform);
        lineObj.layer = LayerMask.NameToLayer(lineRendererLayer);
        target.lineRenderer = lineObj.AddComponent<LineRenderer>();
        target.lineRenderer.startWidth = 0.1f;
        target.lineRenderer.endWidth = 0.1f;
        target.lineRenderer.material = mat != null ? mat : new Material(Shader.Find("Sprites/Default"));
        target.lineRenderer.startColor = inactiveLineColor;
        target.lineRenderer.endColor = inactiveLineColor;
        target.lineRenderer.sortingOrder = target.sortingOrder;
        target.lineRenderer.sortingLayerName = lineRendererLayer;

        // Añadir sprite en el punto final
        if (target.lineEndPoint != null)
        {
            SpriteRenderer sr = target.lineEndPoint.gameObject.GetComponent<SpriteRenderer>();
            if (sr == null)
            {
                sr = target.lineEndPoint.gameObject.AddComponent<SpriteRenderer>();
            }
            sr.sprite = Resources.Load<Sprite>("ConnectorSprite");
            if (sr.sprite == null)
            {
                Debug.LogWarning($"ChargeableObject '{gameObject.name}': No se encontró 'ConnectorSprite' en la carpeta Resources.");
            }
            sr.sortingLayerName = lineRendererLayer;
            sr.sortingOrder = target.sortingOrder + 1; // Delante de la línea
        }

        UpdateLineRenderer(target);
    }
    private List<Vector3> SmoothLinePositions(List<Vector3> inputPositions)
    {
        List<Vector3> smoothed = new List<Vector3>();
        int segmentsPerPoint = 10;
        for (int i = 0; i < inputPositions.Count - 1; i++)
        {
            for (int j = 0; j < segmentsPerPoint; j++)
            {
                float t = j / (float)segmentsPerPoint;
                Vector3 pos = Vector3.Lerp(inputPositions[i], inputPositions[i + 1], t);
                smoothed.Add(pos);
            }
        }
        smoothed.Add(inputPositions[inputPositions.Count - 1]);
        return smoothed;
    }
    private void UpdateLineRenderer(TargetEntry target)
    {
        List<Vector3> positions = new List<Vector3>();
        Vector3 startPos = lineStartPoint != null ? lineStartPoint.position : transform.position;
        startPos.z = -1f + 0.01f * Vector3.Distance(startPos, Camera.main.transform.position);
        positions.Add(startPos);

        if (target.waypoints != null)
        {
            foreach (var waypoint in target.waypoints)
            {
                if (waypoint != null)
                {
                    Vector3 pos = waypoint.position;
                    pos.z = -1f + 0.01f * Vector3.Distance(pos, Camera.main.transform.position);
                    positions.Add(pos);
                }
            }
        }

        Vector3 targetPos = target.lineEndPoint != null ? target.lineEndPoint.position : target.targetObject.transform.position;
        targetPos.z = -1f + 0.01f * Vector3.Distance(targetPos, Camera.main.transform.position);
        positions.Add(targetPos);

        // Aplicar suavizado a las posiciones
        target.lineRenderer.positionCount = SmoothLinePositions(positions).Count;
        target.lineRenderer.SetPositions(SmoothLinePositions(positions).ToArray());

        target.lineRenderer.enabled = true;
        Color targetColor = isActive ? activeLineColor : (isCharging ? chargingLineColor : inactiveLineColor);
        target.lineRenderer.startColor = targetColor;
        target.lineRenderer.endColor = targetColor;

        // Animación de pulso en el punto final
        if (target.lineEndPoint != null && (isCharging || isActive))
        {
            float scale = 1f + Mathf.Sin(Time.time * 5f) * 0.1f;
            target.lineEndPoint.localScale = new Vector3(scale, scale, 1f);
        }
        else if (target.lineEndPoint != null)
        {
            target.lineEndPoint.localScale = Vector3.one; // Escala normal cuando no está activo
        }
    }

}
