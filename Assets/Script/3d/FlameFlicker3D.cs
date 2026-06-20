using UnityEngine;

public class FlameFlicker3D : MonoBehaviour
{
    private Light fireLight;
    private float baseIntensity;

    [Header("Flicker Settings")]
    public float minIntensityOffset = -0.4f;
    public float maxIntensityOffset = 0.4f;
    public float flickerSpeed = 12f; // Smooth but lively flicker

    void Start()
    {
        fireLight = GetComponent<Light>();
        if (fireLight != null)
        {
            baseIntensity = fireLight.intensity;
        }
    }

    void Update()
    {
        if (fireLight == null) return;

        // Use Perlin noise for a continuous organic flicker
        float noise = Mathf.PerlinNoise(Time.time * flickerSpeed, transform.position.x);
        fireLight.intensity = baseIntensity + Mathf.Lerp(minIntensityOffset, maxIntensityOffset, noise);
    }
}
