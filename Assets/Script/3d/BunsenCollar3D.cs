using UnityEngine;

public class BunsenCollar3D : MonoBehaviour
{
    private bool isAirHoleOpen = false;
    public bool IsAirHoleOpen => isAirHoleOpen;

    [Header("References")]
    public ParticleSystem targetFlame;
    public ParticleSystem targetSmoke;

    private float currentYRotation = 0f;
    private float targetYRotation = 0f;
    private float rotateSpeed = 8f;

    private AudioSource audioSource;
    private AudioClip clickSFX;

    private MeshRenderer collarRenderer;
    private MaterialPropertyBlock propBlock;

    private GameObject floatingIndicator;
    private static Texture2D handCursorTex;

    void Start()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0.5f;
        audioSource.volume = 0.5f;

        clickSFX = CreateClickSound();

        // If targetFlame is not assigned, find it in siblings or children
        if (targetFlame == null)
        {
            Transform flameTrans = transform.parent != null ? transform.parent.Find("FlameVisual") : null;
            if (flameTrans != null) targetFlame = flameTrans.GetComponent<ParticleSystem>();
        }

        // Find smoke visual
        if (targetSmoke == null)
        {
            Transform smokeTrans = transform.parent != null ? transform.parent.Find("SmokeVisual") : null;
            if (smokeTrans != null) targetSmoke = smokeTrans.GetComponent<ParticleSystem>();
        }

        collarRenderer = GetComponent<MeshRenderer>();
        if (collarRenderer != null && collarRenderer.sharedMaterial != null)
        {
            collarRenderer.sharedMaterial.EnableKeyword("_EMISSION");
        }
        propBlock = new MaterialPropertyBlock();

        // Dynamically add TooltipHover3D component if missing so players see instructions on hover
        TooltipHover3D tooltip = GetComponent<TooltipHover3D>();
        if (tooltip == null)
        {
            tooltip = gameObject.AddComponent<TooltipHover3D>();
            tooltip.tooltipText = "Klik untuk putar lubang udara";
        }

        // Initialize visually based on state
        UpdateFlameVisuals(true);
    }

    void Update()
    {
        // Smoothly animate collar rotation Y
        if (Mathf.Abs(currentYRotation - targetYRotation) > 0.1f)
        {
            currentYRotation = Mathf.Lerp(currentYRotation, targetYRotation, Time.deltaTime * rotateSpeed);
            transform.localRotation = Quaternion.Euler(0f, currentYRotation, 0f);
        }

        // Pulse emission when flame is on but air hole is closed
        if (collarRenderer != null && propBlock != null)
        {
            if (!isAirHoleOpen && targetFlame != null && targetFlame.isPlaying)
            {
                float pulse = (Mathf.Sin(Time.time * 6f) + 1f) * 0.5f; // 0 to 1
                Color glowColor = new Color(0.8f, 0.6f, 0.1f) * pulse * 1.5f; // Golden glow
                
                collarRenderer.GetPropertyBlock(propBlock);
                propBlock.SetColor("_EmissionColor", glowColor);
                collarRenderer.SetPropertyBlock(propBlock);
            }
            else
            {
                collarRenderer.SetPropertyBlock(null);
            }
        }

        // Bouncing floating indicator [PUTAR COLLAR] when flame is active but collar is closed (disabled as collar highlight is sufficient)
        bool shouldShowIndicator = false;
        if (shouldShowIndicator)
        {
            if (floatingIndicator == null)
            {
                CreateFloatingIndicator();
            }
            else
            {
                floatingIndicator.SetActive(true);
                float bounce = Mathf.Sin(Time.time * 6f) * 0.012f;
                // Positioned around collar level
                floatingIndicator.transform.position = transform.position + new Vector3(0f, 0.03f, 0f);
                
                // Make the hand bounce sideways by adjusting its local position
                Transform handModel = floatingIndicator.transform.Find("3DHandPointer");
                if (handModel != null)
                {
                    handModel.localPosition = new Vector3(0.14f + bounce, 0f, 0f);
                }
            }
        }
        else
        {
            if (floatingIndicator != null)
            {
                floatingIndicator.SetActive(false);
            }
        }
    }

    private void CreateFloatingIndicator()
    {
        floatingIndicator = new GameObject("CollarClickIndicator");
        floatingIndicator.transform.position = transform.position + new Vector3(0f, 0.03f, 0f); // Lowered to collar level
        floatingIndicator.transform.localScale = Vector3.one;
        
        // Face the camera dynamically so it is always oriented nicely towards the player
        floatingIndicator.AddComponent<LookAtCamera3D>();

        // Create the 3D hand model
        GameObject handModel = Create3DHandModel();
        handModel.transform.SetParent(floatingIndicator.transform, false);
        handModel.transform.localPosition = new Vector3(0.14f, 0f, 0f); // Shifted to the right
        handModel.transform.localRotation = Quaternion.Euler(0f, 0f, -90f); // Pointing to the left
        handModel.transform.localScale = Vector3.one * 5f; // Made 5 times bigger and more visible
    }

    private GameObject Create3DHandModel()
    {
        GameObject handParent = new GameObject("3DHandPointer");
        
        // Holographic cyan material supporting both URP and Standard rendering pipelines
        Material holoMat = null;
        Shader urpLit = Shader.Find("Universal Render Pipeline/Lit");
        if (urpLit != null)
        {
            holoMat = new Material(urpLit);
            holoMat.color = new Color(0f, 0.8f, 1f, 0.8f);
            holoMat.SetColor("_BaseColor", new Color(0f, 0.8f, 1f, 0.8f));
            holoMat.EnableKeyword("_EMISSION");
            holoMat.SetColor("_EmissionColor", new Color(0f, 0.6f, 0.8f) * 1.5f);
            holoMat.SetFloat("_Surface", 1f); // 1 = Transparent
            holoMat.SetFloat("_Blend", 0f); // 0 = Alpha
            holoMat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            holoMat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            holoMat.SetInt("_ZWrite", 0);
            holoMat.DisableKeyword("_ALPHATEST_ON");
            holoMat.EnableKeyword("_ALPHABLEND_ON");
            holoMat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            holoMat.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
        }
        else
        {
            Shader standardShader = Shader.Find("Standard");
            if (standardShader == null) standardShader = Shader.Find("Diffuse");
            
            holoMat = new Material(standardShader);
            holoMat.color = new Color(0f, 0.8f, 1f, 0.8f);
            holoMat.EnableKeyword("_EMISSION");
            holoMat.SetColor("_EmissionColor", new Color(0f, 0.6f, 0.8f) * 1.5f);
            holoMat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            holoMat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            holoMat.SetInt("_ZWrite", 0);
            holoMat.DisableKeyword("_ALPHATEST_ON");
            holoMat.EnableKeyword("_ALPHABLEND_ON");
            holoMat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            holoMat.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
        }

        // Palm (Box)
        GameObject palm = GameObject.CreatePrimitive(PrimitiveType.Cube);
        palm.name = "Palm";
        palm.transform.SetParent(handParent.transform, false);
        palm.transform.localPosition = new Vector3(0f, 0f, 0f);
        palm.transform.localScale = new Vector3(0.015f, 0.015f, 0.006f);
        palm.GetComponent<Renderer>().material = holoMat;
        Destroy(palm.GetComponent<Collider>());

        // Index Finger (Cylinder - pointing downwards/forwards)
        GameObject indexFinger = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        indexFinger.name = "IndexFinger";
        indexFinger.transform.SetParent(handParent.transform, false);
        indexFinger.transform.localPosition = new Vector3(0f, -0.015f, 0f);
        indexFinger.transform.localScale = new Vector3(0.0035f, 0.008f, 0.0035f);
        indexFinger.transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
        indexFinger.GetComponent<Renderer>().material = holoMat;
        Destroy(indexFinger.GetComponent<Collider>());

        // Thumb (Cylinder)
        GameObject thumb = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        thumb.name = "Thumb";
        thumb.transform.SetParent(handParent.transform, false);
        thumb.transform.localPosition = new Vector3(-0.009f, -0.003f, -0.003f);
        thumb.transform.localScale = new Vector3(0.003f, 0.005f, 0.003f);
        thumb.transform.localRotation = Quaternion.Euler(30f, 0f, -45f);
        thumb.GetComponent<Renderer>().material = holoMat;
        Destroy(thumb.GetComponent<Collider>());

        // Folded Middle Finger (Cylinder)
        GameObject middle = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        middle.name = "MiddleFinger";
        middle.transform.SetParent(handParent.transform, false);
        middle.transform.localPosition = new Vector3(0.004f, -0.003f, -0.005f);
        middle.transform.localScale = new Vector3(0.0035f, 0.003f, 0.0035f);
        middle.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
        middle.GetComponent<Renderer>().material = holoMat;
        Destroy(middle.GetComponent<Collider>());

        // Folded Ring Finger (Cylinder)
        GameObject ring = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        ring.name = "RingFinger";
        ring.transform.SetParent(handParent.transform, false);
        ring.transform.localPosition = new Vector3(0.008f, -0.003f, -0.005f);
        ring.transform.localScale = new Vector3(0.0032f, 0.003f, 0.0032f);
        ring.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
        ring.GetComponent<Renderer>().material = holoMat;
        Destroy(ring.GetComponent<Collider>());

        // Folded Pinky Finger (Cylinder)
        GameObject pinky = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        pinky.name = "PinkyFinger";
        pinky.transform.SetParent(handParent.transform, false);
        pinky.transform.localPosition = new Vector3(0.012f, -0.003f, -0.005f);
        pinky.transform.localScale = new Vector3(0.0028f, 0.003f, 0.0028f);
        pinky.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
        pinky.GetComponent<Renderer>().material = holoMat;
        Destroy(pinky.GetComponent<Collider>());

        return handParent;
    }

    private Texture2D GetHandCursorTexture()
    {
        if (handCursorTex != null) return handCursorTex;

        Texture2D tex = new Texture2D(32, 32, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Point;

        for (int y = 0; y < 32; y++)
        {
            for (int x = 0; x < 32; x++)
            {
                tex.SetPixel(x, y, Color.clear);
            }
        }

        // Draw classic pixel pointer hand
        string[] pattern = new string[] {
            "        X                       ", // 0
            "       X.X                      ", // 1
            "       X.X                      ", // 2
            "       X.X                      ", // 3
            "       X.X      X   X   X       ", // 4
            "       X.X     X.X X.X X.X      ", // 5
            "       X.X     X.X X.X X.X      ", // 6
            "       X.X     X.X X.X X.X      ", // 7
            "   X   X.X     X.X X.X X.X      ", // 8
            "  X.X  X.X     X.X X.X X.X      ", // 9
            "  X.X  X.X     X.X X.X X.X  X   ", // 10
            "  X.X  X.X     X.X X.X X.X X.X  ", // 11
            "  X.X  X.X     X.X X.X X.X X.X  ", // 12
            "  X.X  X.X     X.X X.X X.X X.X  ", // 13
            "  X.X  X.X     X.X X.X X.X X.X  ", // 14
            "  X.X  X.X     X.X X.X X.X X.X  ", // 15
            "   X.X X.X     X.X X.X X.X X.X  ", // 16
            "   X.X X.X     X.X X.X X.X X.X  ", // 17
            "    X.X.X.XXXXX.X.X.X.X.X.X.X   ", // 18
            "    X.X.X.X.X.X.X.X.X.X.X.X.X   ", // 19
            "     X.X.X.X.X.X.X.X.X.X.X.X    ", // 20
            "     X.X.X.X.X.X.X.X.X.X.X.X    ", // 21
            "      X.X.X.X.X.X.X.X.X.X.X     ", // 22
            "      X.X.X.X.X.X.X.X.X.X.X     ", // 23
            "       X.X.X.X.X.X.X.X.X.X      ", // 24
            "       X.X.X.X.X.X.X.X.X.X      ", // 25
            "       X.X.X.X.X.X.X.X.X.X      ", // 26
            "       X.XXXXXX.XXXXXX.X        ", // 27
            "       X......X.X......X        ", // 28
            "       XXXXXXXX.XXXXXXXX        ", // 29
            "                                ", // 30
            "                                "  // 31
        };

        for (int y = 0; y < 32; y++)
        {
            if (y >= pattern.Length) continue;
            string row = pattern[y];
            int texY = 31 - y;
            for (int x = 0; x < row.Length && x < 32; x++)
            {
                char c = row[x];
                if (c == 'X')
                {
                    tex.SetPixel(x, texY, Color.black);
                }
                else if (c == '.')
                {
                    tex.SetPixel(x, texY, Color.white);
                }
            }
        }
        tex.Apply();
        handCursorTex = tex;
        return handCursorTex;
    }

    private void OnMouseEnter()
    {
        Cursor.SetCursor(GetHandCursorTexture(), new Vector2(8, 0), CursorMode.Auto);
    }

    private void OnMouseExit()
    {
        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
    }

    private void OnDisable()
    {
        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
        if (floatingIndicator != null)
        {
            Destroy(floatingIndicator);
        }
    }

    private void OnDestroy()
    {
        if (floatingIndicator != null)
        {
            Destroy(floatingIndicator);
        }
    }

    void OnMouseDown()
    {
        // Toggle state
        isAirHoleOpen = !isAirHoleOpen;
        targetYRotation = isAirHoleOpen ? 90f : 0f;

        // Play click sound
        if (audioSource != null && clickSFX != null)
        {
            audioSource.PlayOneShot(clickSFX);
        }

        UpdateFlameVisuals(false);
        Debug.Log($"{gameObject.name}: Lubang udara {(isAirHoleOpen ? "DIBUKA (Nyalaan Biru)" : "DITUTUP (Nyalaan Keselamatan)")}");
    }

    public void ResetCollar()
    {
        isAirHoleOpen = false;
        targetYRotation = 0f;
        currentYRotation = 0f;
        transform.localRotation = Quaternion.identity;
        UpdateFlameVisuals(true);
    }

    public void UpdateFlameVisuals(bool forceInstant)
    {
        if (targetFlame == null) return;

        var main = targetFlame.main;
        var shape = targetFlame.shape;

        if (isAirHoleOpen)
        {
            // --- BLUE HEATING FLAME (Hot roaring jet) ---
            main.startLifetime = new ParticleSystem.MinMaxCurve(0.07f, 0.09f); // Dies at gauze
            main.startSpeed = new ParticleSystem.MinMaxCurve(1.3f, 1.6f); // High speed
            main.startSize3D = true;
            main.startSizeX = new ParticleSystem.MinMaxCurve(0.04f, 0.06f);
            main.startSizeY = new ParticleSystem.MinMaxCurve(0.12f, 0.16f);
            main.startSizeZ = new ParticleSystem.MinMaxCurve(0.04f, 0.06f);

            Gradient blueGrad = new Gradient();
            blueGrad.SetKeys(
                new GradientColorKey[] { 
                    new GradientColorKey(new Color(0.7f, 0.95f, 1f), 0.0f), // Bright white-cyan core
                    new GradientColorKey(new Color(0.1f, 0.6f, 1f), 0.3f), // Cyan-blue inner flame
                    new GradientColorKey(new Color(0.05f, 0.2f, 0.9f), 0.7f), // Blue outer envelope
                    new GradientColorKey(new Color(0f, 0.05f, 0.6f), 1.0f) // Deep indigo tip
                },
                new GradientAlphaKey[] { 
                    new GradientAlphaKey(1.0f, 0.0f), 
                    new GradientAlphaKey(0.8f, 0.5f), 
                    new GradientAlphaKey(0.0f, 1.0f) 
                }
            );
            main.startColor = new ParticleSystem.MinMaxGradient(blueGrad);
            shape.angle = 1.5f; // Sharp jet
            shape.radius = 0.005f; // Focused
            
            if (targetSmoke != null)
            {
                var em = targetSmoke.emission;
                em.rateOverTime = 0f;
            }
        }
        else
        {
            // --- YELLOW SAFETY FLAME (Lazy luminous flame) ---
            main.startLifetime = new ParticleSystem.MinMaxCurve(0.15f, 0.22f); // Lazy, taller flame
            main.startSpeed = new ParticleSystem.MinMaxCurve(0.6f, 0.9f); // Slower rise
            main.startSize3D = true;
            main.startSizeX = new ParticleSystem.MinMaxCurve(0.05f, 0.07f);
            main.startSizeY = new ParticleSystem.MinMaxCurve(0.16f, 0.22f);
            main.startSizeZ = new ParticleSystem.MinMaxCurve(0.05f, 0.07f);

            Gradient yellowGrad = new Gradient();
            yellowGrad.SetKeys(
                new GradientColorKey[] { 
                    new GradientColorKey(new Color(1f, 0.95f, 0.7f), 0.0f), // Warm white base
                    new GradientColorKey(new Color(1f, 0.65f, 0f), 0.3f), // Yellow-orange middle
                    new GradientColorKey(new Color(0.9f, 0.2f, 0f), 0.7f), // Reddish outer tip
                    new GradientColorKey(new Color(0.5f, 0.05f, 0f), 1.0f) // Soft dark red tip
                },
                new GradientAlphaKey[] { 
                    new GradientAlphaKey(1.0f, 0.0f), 
                    new GradientAlphaKey(0.7f, 0.6f), 
                    new GradientAlphaKey(0.0f, 1.0f) 
                }
            );
            main.startColor = new ParticleSystem.MinMaxGradient(yellowGrad);
            shape.angle = 3.5f; // Wider, lazier base
            shape.radius = 0.015f;
            
            if (targetSmoke != null)
            {
                var em = targetSmoke.emission;
                em.rateOverTime = 8f; // Emit smoke for yellow flame
            }
        }

        // If lit, refresh/re-play particles immediately to reflect properties
        if (targetFlame.isPlaying)
        {
            targetFlame.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            targetFlame.Play();
        }
    }

    private AudioClip CreateClickSound()
    {
        int samplerate = 44100;
        float duration = 0.15f;
        int numSamples = (int)(samplerate * duration);
        float[] samples = new float[numSamples];
        for (int i = 0; i < numSamples; i++)
        {
            float t = (float)i / samplerate;
            // A sharp metallic tick/ping sound
            float tick = Mathf.Sin(2f * Mathf.PI * 1500f * t) * Mathf.Exp(-t * 80f);
            float noise = (Random.value * 2f - 1f) * 0.1f * Mathf.Exp(-t * 50f);
            samples[i] = (tick + noise) * 0.4f;
        }
        AudioClip clip = AudioClip.Create("CollarClickSFX", numSamples, 1, samplerate, false);
        clip.SetData(samples, 0);
        return clip;
    }
}
