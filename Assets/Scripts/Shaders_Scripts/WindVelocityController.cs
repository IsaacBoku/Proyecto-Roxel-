using UnityEngine;

public class WindVelocityController : MonoBehaviour
{
    [Range(0f, 1f)] public float ExternalInfluenceStrenght = 0.25f;
    public float EaseInTime = 0.15f;
    public float EaseOutTime = 0.15f;
    public float VelocityThreshold = 5f;

    private int _externalInfluence = Shader.PropertyToID("_ExternalInfluence");

    public void InfluenceWind(Material mat, float XVelocity)
    {
        mat.SetFloat(_externalInfluence, XVelocity);
    }

}
