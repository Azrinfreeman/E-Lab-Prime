using UnityEngine;

public class WirePulseAnimator : MonoBehaviour
{
    private LineRenderer lineRenderer;
    
    [Header("Color Settings")]
    public Color baseColor = Color.red;
    public Color pulseColor = new Color(1f, 0.8f, 0.2f, 1f); // Glowing gold/yellow
    
    [Header("Animation Settings")]
    public float speed = 2.0f;
    public float pulseWidth = 0.2f;
    
    private float pulsePosition = 0f;
    private bool isActive = false;
    private Gradient gradient;
    private GradientColorKey[] colorKeys;
    private GradientAlphaKey[] alphaKeys;

    void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
        gradient = new Gradient();
        colorKeys = new GradientColorKey[5];
        alphaKeys = new GradientAlphaKey[5];
        
        // Initialize keys
        for (int i = 0; i < 5; i++)
        {
            colorKeys[i] = new GradientColorKey(baseColor, i / 4f);
            alphaKeys[i] = new GradientAlphaKey(baseColor.a, i / 4f);
        }
        
        if (lineRenderer != null)
        {
            // Ensure the line has the base color from the start
            SetActive(isActive);
        }
    }

    public void SetActive(bool active)
    {
        isActive = active;
        if (!active && lineRenderer != null)
        {
            // Reset to static base color
            Gradient staticGrad = new Gradient();
            staticGrad.SetKeys(
                new GradientColorKey[] { new GradientColorKey(baseColor, 0f), new GradientColorKey(baseColor, 1f) },
                new GradientAlphaKey[] { new GradientAlphaKey(baseColor.a, 0f), new GradientAlphaKey(baseColor.a, 1f) }
            );
            lineRenderer.colorGradient = staticGrad;
        }
    }

    void Update()
    {
        if (!isActive || lineRenderer == null) return;

        // Animate the pulse position along the wire (0 to 1)
        pulsePosition += speed * Time.deltaTime;
        if (pulsePosition > 1.2f)
        {
            pulsePosition = -0.2f; // Start before the wire begins for a smooth fade-in
        }

        UpdateGradient();
    }

    private void UpdateGradient()
    {
        // Define a 5-key gradient to represent: BaseColor -> Blend -> Pulse -> Blend -> BaseColor
        float p = pulsePosition;
        float halfW = pulseWidth * 0.5f;

        float t0 = Mathf.Clamp01(p - halfW * 2f);
        float t1 = Mathf.Clamp01(p - halfW);
        float t2 = Mathf.Clamp01(p);
        float t3 = Mathf.Clamp01(p + halfW);
        float t4 = Mathf.Clamp01(p + halfW * 2f);

        // Assign colors at each key position
        colorKeys[0] = new GradientColorKey(baseColor, t0);
        colorKeys[1] = new GradientColorKey(Color.Lerp(baseColor, pulseColor, 0.5f), t1);
        colorKeys[2] = new GradientColorKey(pulseColor, t2);
        colorKeys[3] = new GradientColorKey(Color.Lerp(baseColor, pulseColor, 0.5f), t3);
        colorKeys[4] = new GradientColorKey(baseColor, t4);

        // Assign alphas (matching baseColor alpha and pulseColor alpha)
        float baseAlpha = baseColor.a;
        float pulseAlpha = pulseColor.a;
        alphaKeys[0] = new GradientAlphaKey(baseAlpha, t0);
        alphaKeys[1] = new GradientAlphaKey(Mathf.Lerp(baseAlpha, pulseAlpha, 0.5f), t1);
        alphaKeys[2] = new GradientAlphaKey(pulseAlpha, t2);
        alphaKeys[3] = new GradientAlphaKey(Mathf.Lerp(baseAlpha, pulseAlpha, 0.5f), t3);
        alphaKeys[4] = new GradientAlphaKey(baseAlpha, t4);

        // Sort keys by time to prevent Unity gradient warnings
        System.Array.Sort(colorKeys, (x, y) => x.time.CompareTo(y.time));
        System.Array.Sort(alphaKeys, (x, y) => x.time.CompareTo(y.time));

        // Edge adjustments to ensure the pulse is local and doesn't smear
        if (colorKeys[0].time > 0f) colorKeys[0].time = 0f; // Fix start
        if (colorKeys[4].time < 1f) colorKeys[4].time = 1f; // Fix end
        if (alphaKeys[0].time > 0f) alphaKeys[0].time = 0f;
        if (alphaKeys[4].time < 1f) alphaKeys[4].time = 1f;

        gradient.SetKeys(colorKeys, alphaKeys);
        lineRenderer.colorGradient = gradient;
    }
}
