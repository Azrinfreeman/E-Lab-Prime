using UnityEngine;

public class PhysicalSwitch3D : MonoBehaviour
{
    public ElectrolysisManager3D manager;
    public AmmeterNeedle3D ammeter;
    public Transform leverPivot;
    
    // The angle of the lever when OFF (open circuit)
    public float offAngle = -45f;
    // The angle of the lever when ON (closed circuit)
    public float onAngle = 45f;
    
    [Header("Highlight Settings")]
    public Color highlightColor = new Color(0.2f, 0.8f, 1.0f, 1.0f); // Glowing Cyan
    
    public bool isOn = false;
    private Renderer[] renderers;
    private Color[] originalColors;
    private AudioSource audioSource;
    private AudioSource humAudioSource;
    private GameObject[] glowShells;

    void Start()
    {
        if (leverPivot != null)
        {
            // Initial state is OFF
            Vector3 rot = leverPivot.localEulerAngles;
            rot.z = offAngle;
            leverPivot.localEulerAngles = rot;
        }

        // Cache renderers and original colors for the hover highlight
        renderers = GetComponentsInChildren<Renderer>();
        originalColors = new Color[renderers.Length];
        for (int i = 0; i < renderers.Length; i++)
        {
            originalColors[i] = renderers[i].material.color;
        }

        // Setup AudioSource for click sound
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 1.0f; // 3D spatial sound

        // Setup AudioSource for electrical hum
        humAudioSource = gameObject.AddComponent<AudioSource>();
        humAudioSource.clip = CreateHumSound();
        humAudioSource.loop = true;
        humAudioSource.playOnAwake = false;
        humAudioSource.spatialBlend = 1.0f;
        humAudioSource.volume = 0.05f;

        // Create the glow outline shells
        CreateGlowShells();
        
        // Ensure outlines are visible initially
        SetGlowShellsActive(true);
    }

    void Update()
    {
        // Pulse the outline to draw attention when OFF
        if (!isOn && glowShells != null && glowShells.Length > 0 && glowShells[0] != null)
        {
            float pulse = Mathf.PingPong(Time.time * 3f, 1f) * 0.4f + 0.2f; // 0.2 to 0.6 opacity
            MeshRenderer mr = glowShells[0].GetComponent<MeshRenderer>();
            if (mr != null && mr.sharedMaterial != null)
            {
                Color c = highlightColor;
                c.a = pulse;
                mr.sharedMaterial.color = c;
                if (mr.sharedMaterial.HasProperty("_BaseColor"))
                {
                    mr.sharedMaterial.SetColor("_BaseColor", c);
                }
            }
        }
    }

    void OnMouseDown()
    {
        if (!isOn && manager != null)
        {
            TurnOn();
        }
    }

    void OnMouseEnter()
    {
        if (!isOn)
        {
            // Apply highlight and emission glow to casing and lever
            for (int i = 0; i < renderers.Length; i++)
            {
                if (renderers[i] != null)
                {
                    renderers[i].material.color = highlightColor;
                    renderers[i].material.SetColor("_EmissionColor", highlightColor * 0.4f);
                    renderers[i].material.EnableKeyword("_EMISSION");
                }
            }
        }
    }

    void OnMouseExit()
    {
        // Revert to original colors (casing/lever)
        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i] != null && originalColors != null && i < originalColors.Length)
            {
                renderers[i].material.color = originalColors[i];
                renderers[i].material.SetColor("_EmissionColor", Color.clear);
                renderers[i].material.DisableKeyword("_EMISSION");
            }
        }
    }

    private void SetGlowShellsActive(bool active)
    {
        if (glowShells != null)
        {
            for (int i = 0; i < glowShells.Length; i++)
            {
                if (glowShells[i] != null) glowShells[i].SetActive(active);
            }
        }
    }

    public void TurnOn()
    {
        isOn = true;
        var tut = Object.FindAnyObjectByType<Lab2TutorialManager>();
        if (tut != null) tut.CompleteStep1();
        
        // Remove hover highlights once turned ON
        OnMouseExit();

        // Turn off the pulsing outline since it has been clicked
        SetGlowShellsActive(false);

        // Rotate lever visually
        if (leverPivot != null)
        {
            Vector3 rot = leverPivot.localEulerAngles;
            rot.z = onAngle;
            leverPivot.localEulerAngles = rot;
        }

        // Play procedural click sound
        PlayClickSound();

        // Trigger electric spark particles
        SpawnSparks();

        // Turn on wire current pulse animations
        SetWireAnimationsActive(true);

        // Turn on electrical hum
        if (humAudioSource != null) humAudioSource.Play();

        // Turn on electrode glow
        SetElectrodeGlow(true);

        // Activate Ammeter
        if (ammeter != null) ammeter.TurnOn();

        // Trigger the experiment
        manager.StartElectrolysis();
        Debug.Log("Physical Switch turned ON.");
    }

    public void ResetSwitch()
    {
        isOn = false;
        
        // Rotate lever back to OFF position
        if (leverPivot != null)
        {
            Vector3 rot = leverPivot.localEulerAngles;
            rot.z = offAngle;
            leverPivot.localEulerAngles = rot;
        }

        // Re-activate outline shells since it needs to be clicked again
        SetGlowShellsActive(true);

        // Turn off electrical hum
        if (humAudioSource != null) humAudioSource.Stop();

        // Turn off electrode glow
        SetElectrodeGlow(false);

        // Turn off Ammeter
        if (ammeter != null) ammeter.TurnOff();

        // Revert colors and disable emission
        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i] != null && originalColors != null && i < originalColors.Length)
            {
                renderers[i].material.color = originalColors[i];
                renderers[i].material.SetColor("_EmissionColor", Color.clear);
                renderers[i].material.DisableKeyword("_EMISSION");
            }
        }
    }

    private void CreateGlowShells()
    {
        MeshFilter[] filters = GetComponentsInChildren<MeshFilter>(true);
        glowShells = new GameObject[filters.Length];

        // Create a dedicated outline material compatible with URP and Standard
        Material outlineMat = null;
        Shader urpUnlit = Shader.Find("Universal Render Pipeline/Unlit");
        if (urpUnlit != null)
        {
            outlineMat = new Material(urpUnlit);
            outlineMat.SetColor("_BaseColor", new Color(0.2f, 0.8f, 1.0f, 0.4f));
            outlineMat.SetFloat("_Surface", 1f); // Transparent
            outlineMat.SetInt("_Blend", 0); // Alpha
            outlineMat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            outlineMat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            outlineMat.SetInt("_ZWrite", 0);
            outlineMat.SetFloat("_Cull", 1f); // Cull Front (draws back faces only)
        }
        else
        {
            Shader standardShader = Shader.Find("Standard");
            outlineMat = new Material(standardShader != null ? standardShader : Shader.Find("Sprites/Default"));
            outlineMat.color = new Color(0.2f, 0.8f, 1.0f, 0.4f);
            if (outlineMat.HasProperty("_Mode")) outlineMat.SetFloat("_Mode", 3f); // Transparent
            outlineMat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            outlineMat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            outlineMat.SetInt("_ZWrite", 0);
            outlineMat.DisableKeyword("_ALPHATEST_ON");
            outlineMat.EnableKeyword("_ALPHABLEND_ON");
            outlineMat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            outlineMat.SetFloat("_Cull", 1f); // Cull Front
        }
        outlineMat.renderQueue = 3100;

        for (int i = 0; i < filters.Length; i++)
        {
            GameObject shell = new GameObject("GlowOutlineShell");
            shell.transform.SetParent(filters[i].transform, false);
            // Scale up slightly to sit outside the parent mesh
            shell.transform.localScale = new Vector3(1.12f, 1.12f, 1.12f);

            MeshFilter mf = shell.AddComponent<MeshFilter>();
            mf.sharedMesh = filters[i].sharedMesh;

            MeshRenderer mr = shell.AddComponent<MeshRenderer>();
            mr.sharedMaterial = outlineMat;
            mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            mr.receiveShadows = false;

            shell.SetActive(false); // Disabled by default
            glowShells[i] = shell;
        }
    }

    private void PlayClickSound()
    {
        int samplerate = 44100;
        float duration = 0.15f;
        int numSamples = (int)(samplerate * duration);
        float[] samples = new float[numSamples];
        
        for (int i = 0; i < numSamples; i++)
        {
            float t = (float)i / samplerate;
            
            // Part 1: High frequency snap
            float snapEnvelope = Mathf.Exp(-t * 120f); // Very fast decay
            float snapWave = Mathf.Sin(2f * Mathf.PI * 1800f * t);
            float snapNoise = (Random.value * 2f - 1f) * 0.3f;
            float snap = (snapWave + snapNoise) * snapEnvelope;
            
            // Part 2: Low frequency resonant body thud
            float thudEnvelope = Mathf.Exp(-t * 30f); // Slower decay
            float thudWave = Mathf.Sin(2f * Mathf.PI * 220f * t);
            float thud = thudWave * thudEnvelope;
            
            // Combine them
            samples[i] = (snap * 0.4f + thud * 0.6f) * 0.5f;
        }
        
        AudioClip clip = AudioClip.Create("SwitchClickSFX", numSamples, 1, samplerate, false);
        clip.SetData(samples, 0);
        
        if (audioSource != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }

    private void SpawnSparks()
    {
        // 1. Spark at the switch terminals (local Z offset -0.1f and 0.1f, Y offset 0.1f)
        Vector3 switchPos = transform.position;
        SpawnSparkAt(switchPos + new Vector3(0f, 0.05f, -0.1f));
        SpawnSparkAt(switchPos + new Vector3(0f, 0.05f, 0.1f));

        // 2. Spark at the battery terminals (x offset -0.06f and 0.06f, y offset 0.26f)
        GameObject battery = GameObject.Find("Battery_9V_3D");
        if (battery != null)
        {
            Vector3 batPos = battery.transform.position;
            SpawnSparkAt(batPos + new Vector3(-0.06f, 0.26f, 0f));
            SpawnSparkAt(batPos + new Vector3(0.06f, 0.26f, 0f));
        }
    }

    private void SpawnSparkAt(Vector3 pos)
    {
        GameObject sparkObj = new GameObject("SparkParticles");
        sparkObj.transform.position = pos;

        ParticleSystem ps = sparkObj.AddComponent<ParticleSystem>();
        ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        
        // Main module settings for a sharp electrical spark burst
        var main = ps.main;
        main.duration = 0.4f;
        main.loop = false;
        main.startLifetime = new ParticleSystem.MinMaxCurve(0.08f, 0.22f);
        main.startSpeed = new ParticleSystem.MinMaxCurve(1.5f, 4.0f);
        main.startSize = new ParticleSystem.MinMaxCurve(0.015f, 0.045f);
        main.startColor = new Color(1.0f, 0.85f, 0.3f, 1.0f); // Bright yellow-orange spark
        main.gravityModifier = 1.2f; // Sparks fall downwards due to gravity
        main.stopAction = ParticleSystemStopAction.Destroy; // Clean up when finished

        // Burst emission
        var emission = ps.emission;
        emission.rateOverTime = 0f;
        var burst = new ParticleSystem.Burst(0f, 10, 15);
        emission.SetBursts(new ParticleSystem.Burst[] { burst });

        // Sphere shape for outward explosion
        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 0.01f;

        // Shrink particles over time
        var sizeOverLifetime = ps.sizeOverLifetime;
        sizeOverLifetime.enabled = true;
        AnimationCurve sizeCurve = new AnimationCurve();
        sizeCurve.AddKey(0f, 1.0f);
        sizeCurve.AddKey(1f, 0f);
        sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1.0f, sizeCurve);

        // Fade colors (bright white-yellow to hot red-orange, then transparent)
        var colorOverLifetime = ps.colorOverLifetime;
        colorOverLifetime.enabled = true;
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new GradientColorKey[] { new GradientColorKey(new Color(1f, 0.95f, 0.6f), 0f), new GradientColorKey(new Color(1f, 0.3f, 0f), 0.7f) },
            new GradientAlphaKey[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(0f, 1f) }
        );
        colorOverLifetime.color = new ParticleSystem.MinMaxGradient(gradient);

        // Configure renderer to stretch along velocity vector (looks like lines/sparks)
        ParticleSystemRenderer psr = sparkObj.GetComponent<ParticleSystemRenderer>();
        psr.renderMode = ParticleSystemRenderMode.Stretch;
        psr.lengthScale = 2.5f;
        psr.cameraVelocityScale = 0f;
        psr.velocityScale = 0.12f;

        // Runtime-safe material creation using a fast unlit built-in shader
        Material sparkMat = new Material(Shader.Find("Sprites/Default"));
        psr.sharedMaterial = sparkMat;

        ps.Play();
    }

    private void SetWireAnimationsActive(bool active)
    {
        WirePulseAnimator[] animators = Object.FindObjectsByType<WirePulseAnimator>(FindObjectsInactive.Include);
        foreach (var anim in animators)
        {
            if (anim != null) anim.SetActive(active);
        }
    }

    private void SetElectrodeGlow(bool active)
    {
        GameObject anode = GameObject.Find("Electrode_Anode");
        if (anode != null)
        {
            Renderer r = anode.GetComponent<Renderer>();
            if (r != null && r.material != null)
            {
                if (active)
                {
                    r.material.EnableKeyword("_EMISSION");
                    r.material.SetColor("_EmissionColor", new Color(1.0f, 0.4f, 0.0f) * 1.5f); // Orange glow
                }
                else
                {
                    r.material.DisableKeyword("_EMISSION");
                    r.material.SetColor("_EmissionColor", Color.black);
                }
            }
        }

        GameObject cathode = GameObject.Find("Electrode_Cathode");
        if (cathode != null)
        {
            Renderer r = cathode.GetComponent<Renderer>();
            if (r != null && r.material != null)
            {
                if (active)
                {
                    r.material.EnableKeyword("_EMISSION");
                    r.material.SetColor("_EmissionColor", new Color(0.0f, 0.8f, 1.0f) * 1.5f); // Cyan glow
                }
                else
                {
                    r.material.DisableKeyword("_EMISSION");
                    r.material.SetColor("_EmissionColor", Color.black);
                }
            }
        }
    }

    private AudioClip CreateHumSound()
    {
        int samplerate = 44100;
        float duration = 1.0f; // loop length
        int numSamples = (int)(samplerate * duration);
        float[] samples = new float[numSamples];
        
        for (int i = 0; i < numSamples; i++)
        {
            float t = (float)i / samplerate;
            // 50Hz and 60Hz mains hum harmonics
            float hum50 = Mathf.Sin(2f * Mathf.PI * 50f * t);
            float hum100 = Mathf.Sin(2f * Mathf.PI * 100f * t);
            float hum150 = Mathf.Sin(2f * Mathf.PI * 150f * t);
            
            float combinedHum = (hum50 * 0.5f) + (hum100 * 0.3f) + (hum150 * 0.2f);
            
            // Add a little bit of high frequency buzz
            float buzz = Mathf.Sin(2f * Mathf.PI * 300f * t) * 0.05f;
            
            samples[i] = (combinedHum + buzz) * 0.5f;
        }
        
        AudioClip clip = AudioClip.Create("ElectricalHum", numSamples, 1, samplerate, false);
        clip.SetData(samples, 0);
        return clip;
    }
}
