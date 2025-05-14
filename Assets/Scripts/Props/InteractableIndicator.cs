using TMPro;
using UnityEngine;

public class InteractableIndicator : MonoBehaviour
{
    [SerializeField] private SpriteRenderer indicatorSprite;
    [SerializeField] private Animator indicatorAnimator;
    [SerializeField] private float blinkSpeed = 2f;
    [SerializeField] private float oscillateSpeed = 2f; 
    [SerializeField] private float oscillateAmount = 0.2f; 
    private float blinkTimer = 0f;
    private Vector3 basePosition;

    private void Start()
    {
        if (indicatorSprite != null)
        {
            indicatorSprite.flipX = false;
            indicatorSprite.flipY = false;
        }
        // Initialize basePosition to current position if not set
        basePosition = transform.position;
    }

    void Update()
    {
        blinkTimer += Time.deltaTime * blinkSpeed;
        float alpha = Mathf.PingPong(blinkTimer, 1f);
        Color spriteColor = indicatorSprite.color;
        spriteColor.a = alpha;
        indicatorSprite.color = spriteColor;

        float offsetY = Mathf.Sin(Time.time * oscillateSpeed) * oscillateAmount;
        transform.position = basePosition + new Vector3(0f, offsetY, 0f);

    }

    public void Show(Vector3 targetPosition)
    {
        basePosition = targetPosition + new Vector3(0f, 1f, 0f); 
        //transform.position = basePosition;

        indicatorSprite.enabled = true;
        //indicatorAnimator.SetBool("IsVisible", true);
    }

    public void Hide()
    {
        indicatorSprite.enabled = false;
        //indicatorAnimator.SetBool("IsVisible", false);
    }
}
