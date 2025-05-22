using System.Collections.Generic;
using UnityEngine;

public class PlatformPedestal : InteractableBase
{
    public enum TargetType
    {
        Platform,
        Laser,
        Door
    }

    [System.Serializable]
    public class TargetEntry
    {
        public TargetType type;
        public GameObject targetObject;
        [HideInInspector] public IActivable activable;
        [HideInInspector] public MovingPlatform platform;
        public Transform[] waypoints; 
        [HideInInspector] public LineRenderer lineRenderer;
        [SerializeField] public int sortingOrder = 10;
        [SerializeField] public Transform lineEndPoint;
    }

    [SerializeField] private List<TargetEntry> targets = new List<TargetEntry>();
    [SerializeField] private ParticleSystem activateEffect;
    [SerializeField] private Transform batteryConnectionPoint;
    [SerializeField] private Material lineMaterial;
    [SerializeField] private Transform lineStartPoint;
    [SerializeField] private Color inactiveLineColor = Color.gray;
    [SerializeField] private Color activeLineColor = Color.green;
    [SerializeField] private string lineRendererLayer = "LineRenderer";

    private bool hasBattery = false;
    private SpriteRenderer sr;
    private Animator anim;
    private GameObject connectedBattery;

    [Header("Audio Settings")]
    [SerializeField, Tooltip("Nombre del sonido al activar el pedestal en el AudioManager")]
    private string activateSoundName = "PedestalOn";
    [SerializeField, Tooltip("Nombre del sonido al desactivar el pedestal en el AudioManager")]
    private string deactivateSoundName = "PedestalOff";

    public bool HasBattery => hasBattery;

    protected override void Start()
    {
        base.Start();
        sr = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();
        if (requiresSpecificPolarity && sr != null)
        {
            sr.color = requiredPolarityIsPositive ? Color.red : Color.blue;
        }
        InitializeTargets();
    }


    public override void Interact()
    {
        Debug.Log($"PlatformPedestal '{gameObject.name}': No se puede interactuar directamente. Coloca una batería.");
    }
    protected override void Update()
    {
        base.Update();
        UpdateAllLineRenderers();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Battery"))
        {
            var batteryController = other.GetComponent<BatteryController>();
            if (requiresSpecificPolarity && batteryController.isPositivePolarity != requiredPolarityIsPositive)
            {
                Debug.Log($"PlatformPedestal '{gameObject.name}': Polaridad incorrecta. Se requiere {(requiredPolarityIsPositive ? "positiva" : "negativa")}.");
                return;
            }

            // Guardar referencia a la batería
            connectedBattery = other.gameObject;
            hasBattery = true;

            // Posicionar la batería en el punto de conexión
            if (batteryConnectionPoint != null)
            {
                connectedBattery.transform.position = batteryConnectionPoint.position;
                connectedBattery.transform.rotation = batteryConnectionPoint.rotation;

                // Desactivar física para que no se mueva
                Rigidbody2D rb = connectedBattery.GetComponent<Rigidbody2D>();
                if (rb != null)
                {
                    rb.linearVelocity = Vector2.zero;
                    rb.bodyType = RigidbodyType2D.Kinematic;
                }
            }

            ActivateTargets();
            if (anim != null) anim.SetBool("Power", true);
            if (activateEffect != null) activateEffect.Play();
            AudioManager.instance.PlaySFX(activateSoundName);
            Debug.Log($"PlatformPedestal '{gameObject.name}': Objetivos activados");
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Battery") && other.gameObject == connectedBattery)
        {
            // Restaurar la física de la batería
            Rigidbody2D rb = other.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.bodyType = RigidbodyType2D.Dynamic;
            }

            hasBattery = false;
            connectedBattery = null;
            DeactivateTargets();
            if (anim != null) anim.SetBool("Power", false);
            AudioManager.instance.PlaySFX(deactivateSoundName);
            Debug.Log($"PlatformPedestal '{gameObject.name}': Objetivos desactivados");
        }
    }

    private void InitializeTargets()
    {
        foreach (var target in targets)
        {
            if (target.targetObject == null)
            {
                Debug.LogWarning($"PlatformPedestal '{gameObject.name}': Un objetivo en la lista 'Targets' no tiene GameObject asignado.");
                continue;
            }

            switch (target.type)
            {
                case TargetType.Platform:
                    target.platform = target.targetObject.GetComponent<MovingPlatform>();
                    if (target.platform == null)
                    {
                        Debug.LogWarning($"PlatformPedestal '{gameObject.name}': El objetivo '{target.targetObject.name}' no tiene un componente MovingPlatform.");
                    }
                    break;
                case TargetType.Laser:
                case TargetType.Door:
                    target.activable = target.targetObject.GetComponent<IActivable>();
                    if (target.activable == null)
                    {
                        Debug.LogWarning($"PlatformPedestal '{gameObject.name}': El objetivo '{target.targetObject.name}' no tiene un componente IActivable.");
                    }
                    break;
            }

            SetupLineRenderer(target);
        }
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
            Debug.LogWarning($"PlatformPedestal '{gameObject.name}': No se encontró 'ConnectorSprite' en la carpeta Resources.");
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

        Color targetColor = hasBattery ? activeLineColor : inactiveLineColor;
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

    private void AnimateEndPoint(TargetEntry target)
    {
        if (target.lineEndPoint == null) return;

        if (hasBattery)
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


    private void ActivateTargets()
    {
        foreach (var target in targets)
        {
            if (target.type == TargetType.Platform && target.platform != null)
            {
                target.platform.Activate();
            }
            else if ((target.type == TargetType.Laser || target.type == TargetType.Door) && target.activable != null)
            {
                target.activable.Toggle(true);
                target.activable.SetIgnoreTrigger(true);
            }
        }
    }

    private void DeactivateTargets()
    {
        foreach (var target in targets)
        {
            if (target.type == TargetType.Platform && target.platform != null)
            {
                target.platform.Deactivate();
            }
            else if ((target.type == TargetType.Laser || target.type == TargetType.Door) && target.activable != null)
            {
                target.activable.Toggle(false);
                target.activable.SetIgnoreTrigger(true);
            }
        }
    }

}
