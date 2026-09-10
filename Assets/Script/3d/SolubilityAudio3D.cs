using UnityEngine;

public class SolubilityAudio3D : MonoBehaviour
{
    private SolubilityManager3D manager;
    private AudioSource boilingSource;
    private AudioSource stirringSource;
    private AudioSource clickSource;

    private AudioClip boilingClip;
    private AudioClip stirringClip;
    private AudioClip clickClip;

    void Start()
    {
        manager = GetComponent<SolubilityManager3D>();

        // Create Procedural Clips
        boilingClip = CreateBoilingClip();
        stirringClip = CreateStirringClip();
        clickClip = CreateClickClip();

        // 1. Boiling Audio Source
        boilingSource = gameObject.AddComponent<AudioSource>();
        boilingSource.clip = boilingClip;
        boilingSource.loop = true;
        boilingSource.playOnAwake = false;
        boilingSource.spatialBlend = 0.85f;
        boilingSource.volume = 0f;
        boilingSource.pitch = 1f;

        // 2. Stirring Audio Source
        stirringSource = gameObject.AddComponent<AudioSource>();
        stirringSource.clip = stirringClip;
        stirringSource.loop = true;
        stirringSource.playOnAwake = false;
        stirringSource.spatialBlend = 0.85f;
        stirringSource.volume = 0f;
        stirringSource.pitch = 1f;

        // 3. Click Audio Source
        clickSource = gameObject.AddComponent<AudioSource>();
        clickSource.clip = clickClip;
        clickSource.loop = false;
        clickSource.playOnAwake = false;
        clickSource.spatialBlend = 0.8f;
        clickSource.volume = 0.5f;
    }

    void Update()
    {
        if (manager == null) return;

        // Check heater state and temperature slider
        bool isHeaterOn = manager.powerSwitch != null && manager.powerSwitch.isOn;
        float tempVal = manager.tempSlider != null ? manager.tempSlider.value : 0f;
        float stirVal = manager.stirSlider != null ? manager.stirSlider.value : 0f;

        // --- Handle Boiling Audio ---
        if (isHeaterOn && tempVal > 0.2f)
        {
            if (!boilingSource.isPlaying) boilingSource.Play();
            float intensity = Mathf.Clamp01((tempVal - 0.2f) / 0.8f);
            boilingSource.volume = Mathf.Lerp(0.01f, 0.2f, intensity);
            boilingSource.pitch = Mathf.Lerp(0.8f, 1.25f, intensity);
        }
        else
        {
            if (boilingSource.isPlaying && boilingSource.volume > 0.001f)
            {
                boilingSource.volume = Mathf.Lerp(boilingSource.volume, 0f, Time.deltaTime * 5f);
                if (boilingSource.volume <= 0.005f) boilingSource.Stop();
            }
            else if (boilingSource.isPlaying)
            {
                boilingSource.Stop();
            }
        }

        // --- Handle Stirring Audio ---
        if (stirVal > 0.05f)
        {
            if (!stirringSource.isPlaying) stirringSource.Play();
            stirringSource.volume = Mathf.Lerp(0.02f, 0.15f, stirVal);
            stirringSource.pitch = Mathf.Lerp(0.7f, 1.3f, stirVal);
        }
        else
        {
            if (stirringSource.isPlaying && stirringSource.volume > 0.001f)
            {
                stirringSource.volume = Mathf.Lerp(stirringSource.volume, 0f, Time.deltaTime * 8f);
                if (stirringSource.volume <= 0.005f) stirringSource.Stop();
            }
            else if (stirringSource.isPlaying)
            {
                stirringSource.Stop();
            }
        }
    }

    public void PlayClick()
    {
        if (clickSource != null && clickClip != null)
        {
            clickSource.PlayOneShot(clickClip, 0.6f);
        }
    }

    private AudioClip CreateBoilingClip()
    {
        int samplerate = 44100;
        float duration = 1.0f;
        int numSamples = (int)(samplerate * duration);
        float[] samples = new float[numSamples];

        for (int i = 0; i < numSamples; i++)
        {
            float t = (float)i / samplerate;
            float noise = (Random.value * 2f - 1f) * 0.08f;
            float lowRumble = Mathf.Sin(2f * Mathf.PI * 65f * t) * 0.05f;
            float midBubble = Mathf.Sin(2f * Mathf.PI * 180f * t + Mathf.Sin(t * 50f) * 2f) * 0.04f;
            samples[i] = noise + lowRumble + midBubble;
        }

        AudioClip clip = AudioClip.Create("BoilingAudioLab5", numSamples, 1, samplerate, false);
        clip.SetData(samples, 0);
        return clip;
    }

    private AudioClip CreateStirringClip()
    {
        int samplerate = 44100;
        float duration = 1.0f;
        int numSamples = (int)(samplerate * duration);
        float[] samples = new float[numSamples];

        for (int i = 0; i < numSamples; i++)
        {
            float t = (float)i / samplerate;
            // Swish sound: rhythmic filtered noise + low frequency wave
            float swishMod = Mathf.Sin(2f * Mathf.PI * 3f * t) * 0.5f + 0.5f;
            float noise = (Random.value * 2f - 1f) * 0.05f * (0.3f + 0.7f * swishMod);
            float waterResonance = Mathf.Sin(2f * Mathf.PI * 120f * t) * 0.03f * swishMod;
            samples[i] = noise + waterResonance;
        }

        AudioClip clip = AudioClip.Create("StirringAudioLab5", numSamples, 1, samplerate, false);
        clip.SetData(samples, 0);
        return clip;
    }

    private AudioClip CreateClickClip()
    {
        int samplerate = 44100;
        float duration = 0.15f;
        int numSamples = (int)(samplerate * duration);
        float[] samples = new float[numSamples];

        for (int i = 0; i < numSamples; i++)
        {
            float t = (float)i / samplerate;
            // Sharp mechanical impulse with exponential decay
            float env = Mathf.Exp(-t * 40f);
            float click = Mathf.Sin(2f * Mathf.PI * 800f * t) * 0.5f * env;
            float thump = Mathf.Sin(2f * Mathf.PI * 150f * t) * 0.3f * Mathf.Exp(-t * 25f);
            samples[i] = click + thump;
        }

        AudioClip clip = AudioClip.Create("SwitchClickLab5", numSamples, 1, samplerate, false);
        clip.SetData(samples, 0);
        return clip;
    }
}
