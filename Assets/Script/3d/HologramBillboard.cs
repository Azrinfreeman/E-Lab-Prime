using UnityEngine;

[ExecuteInEditMode]
public class HologramBillboard : MonoBehaviour
{
    private Camera mainCam;
    private TextMesh textMesh;
    public bool animateOnStart = true;
    public float animationDuration = 1.0f;

    void Start()
    {
        mainCam = Camera.main;
        textMesh = GetComponent<TextMesh>();

        if (animateOnStart && textMesh != null && Application.isPlaying)
        {
            StartCoroutine(GlitchFadeIn());
        }
    }

    private System.Collections.IEnumerator GlitchFadeIn()
    {
        Color originalColor = textMesh.color;
        float elapsed = 0f;
        
        while (elapsed < animationDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / animationDuration;
            
            // Random glitch factor (flickers opacity)
            float glitch = (Random.value > 0.8f) ? Random.Range(0f, 0.4f) : 1f;
            
            Color c = originalColor;
            c.a = Mathf.Lerp(0f, originalColor.a, t) * glitch;
            textMesh.color = c;
            
            yield return null;
        }
        
        textMesh.color = originalColor;
    }

    void LateUpdate()
    {
        if (mainCam != null)
        {
            // Rotate the board to always look directly at the main camera, keeping its UP aligned with camera's UP
            transform.rotation = Quaternion.LookRotation(transform.position - mainCam.transform.position, mainCam.transform.up);
        }
    }
}
