using UnityEngine;

public class InteractableIndicator : MonoBehaviour
{
    [SerializeField] private SpriteRenderer indicatorSprite;
    [SerializeField] private Animator indicatorAnimator;
    [SerializeField] private float blinkSpeed = 2f;
    [SerializeField] private float oscillateSpeed = 2f; // Velocidad de oscilación
    [SerializeField] private float oscillateAmount = 0.2f; // Amplitud de oscilación
    private float blinkTimer = 0f;
    private Vector3 initialLocalPosition;

    void Start()
    {
        initialLocalPosition = transform.localPosition;
    }
    void Update()
    {
        blinkTimer += Time.deltaTime * blinkSpeed;
        float alpha = Mathf.PingPong(blinkTimer, 1f);
        Color spriteColor = indicatorSprite.color;
        spriteColor.a = alpha;
        indicatorSprite.color = spriteColor;

        // Oscilación hacia arriba y abajo
        float offsetY = Mathf.Sin(Time.time * oscillateSpeed) * oscillateAmount;
        transform.localPosition = initialLocalPosition + new Vector3(0f, offsetY, 0f);
    }

    public void Show()
    {
        indicatorSprite.enabled = true;
        indicatorAnimator.SetBool("IsVisible", true);
    }

    public void Hide()
    {
        indicatorSprite.enabled = false;
        indicatorAnimator.SetBool("IsVisible", false);
    }
}
