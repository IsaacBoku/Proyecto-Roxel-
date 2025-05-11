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

    [SerializeField] private List<TargetEntry> targets = new List<TargetEntry>();
    [SerializeField] private float requiredEnergy = 50f;
    [SerializeField] private float chargeDuration = 2f;
    [SerializeField] private Material lineMaterial;
    [SerializeField] private Transform lineStartPoint;
    [SerializeField] private Color inactiveLineColor = Color.gray;
    [SerializeField] private Color chargingLineColor = Color.yellow;
    [SerializeField] private Color activeLineColor = Color.green;
    [SerializeField] private string lineRendererLayer = "LineRenderer";
    [SerializeField] private int lineRendererSortingOrder = 10;
    [SerializeField] private Slider energySlider;

    public bool IsCharging { get; private set; }

    protected override void Start()
    {
        base.Start();
        InitializeTargets();
    }

    protected override void Update()
    {
        base.Update();
        UpdateAllLineRenderers();
    }

    public override void Interact()
    {
        Debug.Log($"ChargeableObject '{gameObject.name}': Necesitas una batería para interactuar.");
    }

    public void StartCharging(BatteryController battery)
    {
        if (IsCharging || isActive)
        {
            Debug.Log($"ChargeableObject '{gameObject.name}': Ya está cargando o activo.");
            return;
        }

        if (battery == null)
        {
            Debug.Log($"ChargeableObject '{gameObject.name}': No se puede interactuar sin una batería.");
            return;
        }

        StartCoroutine(ChargeProgressively(battery));
    }

    public void Deactivate()
    {
        if (!isActive) return;

        isActive = false;
        foreach (var target in targets)
        {
            ToggleTargetActive(target, false);
            SetLineColor(target, inactiveLineColor);
        }
        UpdateVisuals(false);
        Debug.Log($"{gameObject.name} ha sido desactivado.");
    }

    private void InitializeTargets()
    {
        foreach (var target in targets)
        {
            if (!ValidateTarget(target)) continue;

            AssignActivableComponent(target);
            InitializeTarget(target);
            SetupLineRenderer(target);
        }
    }

    private bool ValidateTarget(TargetEntry target)
    {
        if (target.targetObject == null)
        {
            Debug.LogWarning($"ChargeableObject '{gameObject.name}': Un objetivo en la lista 'Targets' no tiene GameObject asignado.");
            return false;
        }
        return true;
    }

    private void AssignActivableComponent(TargetEntry target)
    {
        target.activable = target.type switch
        {
            TargetType.Door => target.targetObject.GetComponent<Door_Mechanic>(),
            TargetType.Laser => target.targetObject.GetComponent<Laser_Mechanic>(),
            _ => null
        };

        if (target.activable == null)
        {
            Debug.LogWarning($"ChargeableObject '{gameObject.name}': El objetivo '{target.targetObject.name}' no tiene un componente {target.type} válido.");
        }
    }

    private void InitializeTarget(TargetEntry target)
    {
        if (target.activable == null) return;

        target.activable.Toggle(isActive);
        target.activable.SetIgnoreTrigger(true);
    }

    private void SetupLineRenderer(TargetEntry target)
    {
        GameObject lineObj = new GameObject($"LineTo_{target.targetObject.name}");
        lineObj.transform.SetParent(transform);
        lineObj.layer = LayerMask.NameToLayer(lineRendererLayer);

        target.lineRenderer = lineObj.AddComponent<LineRenderer>();
        ConfigureLineRenderer(target.lineRenderer, target.sortingOrder);
        SetLineColor(target, inactiveLineColor);

        SetupEndPointSprite(target);
        UpdateLineRenderer(target);
    }

    private void ConfigureLineRenderer(LineRenderer lineRenderer, int sortingOrder)
    {
        lineRenderer.startWidth = 0.1f;
        lineRenderer.endWidth = 0.1f;
        lineRenderer.material = lineMaterial != null ? lineMaterial : new Material(Shader.Find("Sprites/Default"));
        lineRenderer.sortingOrder = sortingOrder;
        lineRenderer.sortingLayerName = lineRendererLayer;
    }

    private void SetupEndPointSprite(TargetEntry target)
    {
        if (target.lineEndPoint == null) return;

        SpriteRenderer sr = target.lineEndPoint.GetComponent<SpriteRenderer>();
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
        sr.sortingOrder = target.sortingOrder + 1;
    }

    private void UpdateAllLineRenderers()
    {
        foreach (var target in targets)
        {
            if (target.lineRenderer != null)
            {
                UpdateLineRenderer(target);
            }
        }
    }

    private void UpdateLineRenderer(TargetEntry target)
    {
        List<Vector3> positions = BuildLinePositions(target);
        List<Vector3> smoothedPositions = SmoothLinePositions(positions);

        target.lineRenderer.positionCount = smoothedPositions.Count;
        target.lineRenderer.SetPositions(smoothedPositions.ToArray());
        target.lineRenderer.enabled = true;

        Color targetColor = GetLineColor();
        target.lineRenderer.startColor = targetColor;
        target.lineRenderer.endColor = targetColor;

        AnimateEndPoint(target);
    }

    private List<Vector3> BuildLinePositions(TargetEntry target)
    {
        List<Vector3> positions = new List<Vector3>();
        Vector3 startPos = lineStartPoint != null ? lineStartPoint.position : transform.position;
        startPos.z = CalculateDepth(startPos);
        positions.Add(startPos);

        if (target.waypoints != null)
        {
            foreach (var waypoint in target.waypoints)
            {
                if (waypoint != null)
                {
                    Vector3 pos = waypoint.position;
                    pos.z = CalculateDepth(pos);
                    positions.Add(pos);
                }
            }
        }

        Vector3 targetPos = target.lineEndPoint != null ? target.lineEndPoint.position : target.targetObject.transform.position;
        targetPos.z = CalculateDepth(targetPos);
        positions.Add(targetPos);

        return positions;
    }

    private float CalculateDepth(Vector3 position)
    {
        return -1f + 0.01f * Vector3.Distance(position, Camera.main.transform.position);
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

    private Color GetLineColor()
    {
        if (isActive) return activeLineColor;
        if (IsCharging) return Color.Lerp(inactiveLineColor, chargingLineColor, GetChargeProgress());
        return inactiveLineColor;
    }

    private float GetChargeProgress()
    {
        // Asumimos que el progreso se calcula en ChargeProgressively
        return energySlider != null ? energySlider.value : 0f;
    }

    private void AnimateEndPoint(TargetEntry target)
    {
        if (target.lineEndPoint == null) return;

        if (IsCharging || isActive)
        {
            float scale = 1f + Mathf.Sin(Time.time * 5f) * 0.1f;
            target.lineEndPoint.localScale = new Vector3(scale, scale, 1f);
        }
        else
        {
            target.lineEndPoint.localScale = Vector3.one;
        }
    }

    private void SetLineColor(TargetEntry target, Color color)
    {
        if (target.lineRenderer != null)
        {
            target.lineRenderer.startColor = color;
            target.lineRenderer.endColor = color;
        }
    }

    private IEnumerator ChargeProgressively(BatteryController battery)
    {
        IsCharging = true;
        float elapsedTime = 0f;
        float energyPerSecond = requiredEnergy / chargeDuration;

        InitializeCharging();

        while (elapsedTime < chargeDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / chargeDuration;
            float energyThisFrame = energyPerSecond * Time.deltaTime;

            if (!ConsumeBatteryEnergy(battery, energyThisFrame))
            {
                HandleChargingFailure();
                yield break;
            }

            UpdateChargingProgress(t);
            yield return null;
        }

        FinalizeCharging();
    }

    private void InitializeCharging()
    {
        foreach (var target in targets)
        {
            ToggleTargetActive(target, true);
            SetLineColor(target, chargingLineColor);
        }

        UpdateVisuals(true);

        if (energySlider != null)
        {
            energySlider.value = 0f;
            energySlider.maxValue = 1f;
        }
    }

    private bool ConsumeBatteryEnergy(BatteryController battery, float energyThisFrame)
    {
        if (battery.energyAmounts < energyThisFrame)
        {
            Debug.Log($"{gameObject.name} no puede activarse: Energía insuficiente ({battery.energyAmounts}/{energyThisFrame} requerida en este frame).");
            return false;
        }

        battery.energyAmounts -= energyThisFrame;
        battery.energyAmounts = Mathf.Clamp(battery.energyAmounts, 0f, battery.maxEnergy);
        return true;
    }

    private void HandleChargingFailure()
    {
        IsCharging = false;
        foreach (var target in targets)
        {
            ToggleTargetActive(target, false);
            SetLineColor(target, inactiveLineColor);
        }

        UpdateVisuals(false);

        if (energySlider != null)
        {
            energySlider.value = 0f;
        }
    }

    private void UpdateChargingProgress(float progress)
    {
        if (energySlider != null)
        {
            energySlider.value = progress;
        }

        if (lineMaterial != null)
        {
            lineMaterial.SetFloat("_Progress", progress);
        }
    }

    private void FinalizeCharging()
    {
        isActive = true;
        IsCharging = false;

        if (energySlider != null)
        {
            energySlider.value = 1f;
        }

        if (lineMaterial != null)
        {
            lineMaterial.SetFloat("_Progress", 1f);
        }

        foreach (var target in targets)
        {
            SetLineColor(target, activeLineColor);
        }

        Debug.Log($"{gameObject.name} ha terminado de cargarse.");
    }

    private void ToggleTargetActive(TargetEntry target, bool active)
    {
        if (target.activable != null)
        {
            target.activable.Toggle(active);
        }
    }
}
