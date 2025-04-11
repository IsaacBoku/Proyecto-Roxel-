using UnityEngine;

public class InteractableIndicator : MonoBehaviour
{
    [SerializeField] private SpriteRenderer indicatorSprite;
    [SerializeField] private float blinkSpeed = 2f;
    private float blinkTimer = 0f;

    void Update()
    {
        // Hacer que el indicador parpadee para que sea más visible
        blinkTimer += Time.deltaTime * blinkSpeed;
        float alpha = Mathf.PingPong(blinkTimer, 1f);
        Color spriteColor = indicatorSprite.color;
        spriteColor.a = alpha;
        indicatorSprite.color = spriteColor;
    }

    public void Show()
    {
        indicatorSprite.enabled = true;
    }

    public void Hide()
    {
        indicatorSprite.enabled = false;
    }
}
