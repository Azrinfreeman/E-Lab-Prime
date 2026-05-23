using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PHManager : MonoBehaviour
{
    [Header("UI References")]
    public Slider phSlider;
    public Image phLiquid; 
    public TextMeshProUGUI statusText;

    [Header("Color Settings (pH Scale)")]
    public Gradient phColorGradient; 
    
    [Header("Transition Settings")]
    public float colorFadeSpeed = 3f;
    public float audioFadeSpeed = 5f;

    [Header("Audio References")]
    public AudioSource audioSource;
    public AudioClip bubblingSound;

    [Header("Safety Hazard & Effects")]
    public Image hazardIcon;
    public float blinkSpeed = 4f; 
    public ParticleSystem bubbleParticles; // [NEW] Reference to your bubbles

    private Color targetColor; 
    private float targetVolume = 0f; 
    private bool isDangerZone = false; 

    void Start()
    {
        if (phSlider != null)
        {
            phSlider.value = 7;
            phSlider.onValueChanged.AddListener(UpdatePHExperiment);
        }

        float gradientTime = (7f - 1f) / 13f;
        targetColor = phColorGradient.Evaluate(gradientTime);
        
        if (phLiquid != null) 
        {
            phLiquid.color = targetColor;
        }

        if (audioSource != null)
        {
            audioSource.clip = bubblingSound;
            audioSource.loop = true; 
            audioSource.volume = 0f; 
        }

        if (hazardIcon != null)
        {
            hazardIcon.gameObject.SetActive(false);
        }

        // [NEW] Make sure bubbles are turned off at the start
        if (bubbleParticles != null)
        {
            bubbleParticles.Stop();
        }

        UpdatePHExperiment(phSlider.value);
    }

    void Update()
    {
        // 1. Smoothly transition the liquid color
        if (phLiquid != null)
        {
            phLiquid.color = Color.Lerp(phLiquid.color, targetColor, Time.deltaTime * colorFadeSpeed);
        }

        // 2. Smoothly fade the audio volume in/out
        if (audioSource != null)
        {
            audioSource.volume = Mathf.Lerp(audioSource.volume, targetVolume, Time.deltaTime * audioFadeSpeed);
            
            if (targetVolume == 0f && audioSource.volume <= 0.01f && audioSource.isPlaying)
            {
                audioSource.Stop();
                audioSource.volume = 0f;
            }
        }

        // 3. Hazard Icon Blinking Animation
        if (hazardIcon != null)
        {
            if (isDangerZone)
            {
                hazardIcon.gameObject.SetActive(true);
                Color iconColor = hazardIcon.color;
                iconColor.a = Mathf.Lerp(0.3f, 1f, Mathf.PingPong(Time.time * blinkSpeed, 1f));
                hazardIcon.color = iconColor;
            }
            else
            {
                hazardIcon.gameObject.SetActive(false);
            }
        }
    }

    public void UpdatePHExperiment(float currentPH)
    {
        if (phColorGradient != null)
        {
            float gradientTime = (currentPH - 1f) / 13f; 
            targetColor = phColorGradient.Evaluate(gradientTime);
        }

        int roundedPH = Mathf.RoundToInt(currentPH);
        isDangerZone = (roundedPH <= 3 || roundedPH >= 11);

        if (isDangerZone)
        {
            targetVolume = 1f; 
            if (audioSource != null && !audioSource.isPlaying) audioSource.Play(); 
            
            // [NEW] Play the bubbles if they aren't already playing
            if (bubbleParticles != null && !bubbleParticles.isPlaying)
            {
                bubbleParticles.Play();
            }
        }
        else
        {
            targetVolume = 0f; 
            
            // [NEW] Stop the bubbles
            if (bubbleParticles != null && bubbleParticles.isPlaying)
            {
                bubbleParticles.Stop();
            }
        }

        if (statusText != null)
        {
            if (roundedPH < 7)
            {
                if(roundedPH <= 3) statusText.text = $"Status: pH {roundedPH} - Strong Acid (e.g., Hydrochloric Acid)";
                else statusText.text = $"Status: pH {roundedPH} - Weak Acid (e.g., Lemon Juice / Budu)";
            }
            else if (roundedPH == 7)
            {
                statusText.text = $"Status: pH {roundedPH} - Neutral (e.g., Pure Water)";
            }
            else
            {
                if(roundedPH >= 11) statusText.text = $"Status: pH {roundedPH} - Strong Alkali (e.g., Bleach)";
                else statusText.text = $"Status: pH {roundedPH} - Weak Alkali (e.g., Soap / Toothpaste)";
            }
        }
    }
}