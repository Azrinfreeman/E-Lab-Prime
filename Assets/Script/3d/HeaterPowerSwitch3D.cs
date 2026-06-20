using UnityEngine;

public class HeaterPowerSwitch3D : MonoBehaviour
{
    public EvaporationManager3D manager;
    public Transform switchLever;
    public float offAngle = -15f;
    public float onAngle = 15f;

    [Header("Highlight Settings")]
    public Color highlightColor = new Color(1.0f, 0.3f, 0.3f, 1.0f); // Bright red highlight
    
    [Header("Audio Settings")]
    public AudioClip clickSFX;

    private bool isPowerOn = false;
    private AudioSource audioSource;
    private Renderer[] renderers;
    private Color[] originalColors;

    void Start()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0.8f;
        audioSource.volume = 0.4f;

        // Cache renderers and original colors for hover highlight
        renderers = GetComponentsInChildren<Renderer>();
        originalColors = new Color[renderers.Length];
        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i] != null && renderers[i].material != null)
            {
                originalColors[i] = renderers[i].material.color;
            }
        }

        if (clickSFX != null)
        {
            audioSource.clip = clickSFX;
        }

        UpdateLeverRotation();
    }

    void OnMouseDown()
    {
        isPowerOn = !isPowerOn;
        if (audioSource != null && clickSFX != null)
        {
            audioSource.PlayOneShot(clickSFX);
        }

        UpdateLeverRotation();

        if (manager != null)
        {
            manager.SetHeaterPower(isPowerOn);
        }
    }

    void OnMouseEnter()
    {
        // Simple highlight on hover to show interactivity
        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i] != null && renderers[i].material != null)
            {
                renderers[i].material.color = Color.Lerp(originalColors[i], Color.white, 0.3f);
            }
        }
    }

    void OnMouseExit()
    {
        // Restore colors on hover exit
        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i] != null && renderers[i].material != null)
            {
                renderers[i].material.color = originalColors[i];
            }
        }
    }

    private void UpdateLeverRotation()
    {
        if (switchLever != null)
        {
            float targetAngle = isPowerOn ? onAngle : offAngle;
            switchLever.localEulerAngles = new Vector3(targetAngle, 0f, 0f);
        }
    }

    public void ResetSwitch()
    {
        isPowerOn = false;
        UpdateLeverRotation();
    }
}
