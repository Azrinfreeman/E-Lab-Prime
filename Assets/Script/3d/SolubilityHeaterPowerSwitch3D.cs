using UnityEngine;

public class SolubilityHeaterPowerSwitch3D : MonoBehaviour
{
    public SolubilityManager3D manager;
    public Transform switchLever;
    public float offAngle = -15f;
    public float onAngle = 15f;

    [Header("Highlight Settings")]
    public Color highlightColor = new Color(1.0f, 0.3f, 0.3f, 1.0f); // Bright red highlight
    
    [Header("Audio Settings")]
    public AudioClip clickSFX;

    public bool isPowerOn = false;
    public bool isOn => isPowerOn;
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

        if (clickSFX == null)
        {
            clickSFX = CreateClickClip();
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

    private AudioClip CreateClickClip()
    {
        int samplerate = 44100;
        float duration = 0.12f;
        int numSamples = (int)(samplerate * duration);
        float[] samples = new float[numSamples];
        for (int i = 0; i < numSamples; i++)
        {
            float t = (float)i / samplerate;
            float env = Mathf.Exp(-t * 45f);
            float click = Mathf.Sin(2f * Mathf.PI * 750f * t) * 0.5f * env;
            float thump = Mathf.Sin(2f * Mathf.PI * 140f * t) * 0.3f * Mathf.Exp(-t * 25f);
            samples[i] = click + thump;
        }
        AudioClip clip = AudioClip.Create("SwitchClickLab5", numSamples, 1, samplerate, false);
        clip.SetData(samples, 0);
        return clip;
    }
}
