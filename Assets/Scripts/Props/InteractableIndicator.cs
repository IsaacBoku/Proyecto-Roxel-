using UnityEngine;

public class InteractableIndicator : MonoBehaviour
{
    [SerializeField] private SpriteRenderer indicatorSprite;
    [SerializeField] private Animator indicatorAnimator;
    [SerializeField] private float blinkSpeed = 2f;
    private float blinkTimer = 0f;

    void Update()
    {
        blinkTimer += Time.deltaTime * blinkSpeed;
        float alpha = Mathf.PingPong(blinkTimer, 1f);
        Color spriteColor = indicatorSprite.color;
        spriteColor.a = alpha;
        indicatorSprite.color = spriteColor;
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
