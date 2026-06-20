using UnityEngine;

public class WaterRipple3D : MonoBehaviour
{
    private Vector3 baseScale;
    private MaterialPropertyBlock propBlock;
    private MeshRenderer meshRenderer;

    public float currentTemp = 25f;
    public float boilingPoint = 100f;

    void Start()
    {
        baseScale = transform.localScale;
        meshRenderer = GetComponent<MeshRenderer>();
        propBlock = new MaterialPropertyBlock();
    }

    void Update()
    {
        float progressToBoil = Mathf.Clamp01((currentTemp - 25f) / (boilingPoint - 25f));
        bool isBoiling = currentTemp >= boilingPoint;

        float speed = Mathf.Lerp(1.5f, 8f, progressToBoil);
        float strength = Mathf.Lerp(0.005f, 0.02f, progressToBoil);

        // Subtle geometric ripple using PerlinNoise mapping over time and position
        float noiseX = Mathf.PerlinNoise(Time.time * speed, transform.position.x * 10f) * 2f - 1f;
        float noiseZ = Mathf.PerlinNoise(transform.position.z * 10f, Time.time * speed) * 2f - 1f;
        
        // Modulate the scale slightly to simulate ripples/boiling disturbance
        transform.localScale = new Vector3(
            baseScale.x + (noiseX * strength * 0.5f), 
            baseScale.y + (Mathf.Abs(noiseX + noiseZ) * strength), // height fluctuates up
            baseScale.z + (noiseZ * strength * 0.5f)
        );

        // Optionally modulate color alpha for a shimmer
        if (meshRenderer != null)
        {
            meshRenderer.GetPropertyBlock(propBlock);
            float alphaShift = (noiseX + noiseZ) * 0.05f * progressToBoil;
            Color baseCol = meshRenderer.sharedMaterial.color;
            Color shimmerCol = new Color(baseCol.r, baseCol.g, baseCol.b, Mathf.Clamp01(baseCol.a + alphaShift));
            propBlock.SetColor("_Color", shimmerCol);
            meshRenderer.SetPropertyBlock(propBlock);
        }
    }
}
