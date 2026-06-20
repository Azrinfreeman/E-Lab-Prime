using UnityEngine;

public class GogglesInteraction3D : MonoBehaviour
{
    private WaterHeatingManager3D manager;
    private AudioSource audioSource;
    private Transform indicatorObj;
    private bool isEquipped = false;

    void Start()
    {
        manager = Object.FindAnyObjectByType<WaterHeatingManager3D>();
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0.5f;
        audioSource.volume = 0.6f;

        indicatorObj = transform.Find("Indicator");
        if (indicatorObj != null) indicatorObj.gameObject.SetActive(false);
    }

    void OnMouseDown()
    {
        if (isEquipped) return;
        Equip();
    }

    public void Equip()
    {
        isEquipped = true;
        
        // Play wear sound
        audioSource.PlayOneShot(CreateWearSound(), 1.0f);
        
        // Disable indicator
        SetIndicatorActive(false);

        if (manager != null)
        {
            manager.EquipGoggles();
            if (manager.tutorialManager != null) manager.tutorialManager.CompleteStep2();
        }
        
        // Hide the prop after a short delay so the sound can play
        Invoke("HideProp", 0.3f);
    }

    private void HideProp()
    {
        gameObject.SetActive(false);
    }

    public void SetIndicatorActive(bool active)
    {
        if (indicatorObj == null) indicatorObj = transform.Find("Indicator");
        if (indicatorObj != null)
        {
            indicatorObj.gameObject.SetActive(active);
            if (active)
            {
                Light lt = indicatorObj.GetComponent<Light>();
                if (lt != null)
                {
                    lt.intensity = 0.2f;
                    lt.range = 0.3f;
                }
            }
        }
    }

    private AudioClip CreateWearSound()
    {
        int samplerate = 44100;
        float duration = 0.25f;
        int numSamples = (int)(samplerate * duration);
        float[] samples = new float[numSamples];
        
        for (int i = 0; i < numSamples; i++)
        {
            float t = (float)i / samplerate;
            // A quick rustling "strap" sound: white noise combined with a quick frequency slide
            float noise = (Random.value * 2f - 1f) * 0.1f * Mathf.Exp(-t * 15f);
            float sine = Mathf.Sin(2f * Mathf.PI * Mathf.Lerp(300f, 150f, t) * t) * 0.2f * Mathf.Exp(-t * 20f);
            samples[i] = noise + sine;
        }
        
        AudioClip clip = AudioClip.Create("WearGogglesSFX", numSamples, 1, samplerate, false);
        clip.SetData(samples, 0);
        return clip;
    }
}
