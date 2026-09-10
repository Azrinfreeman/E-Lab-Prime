using UnityEngine;
using System.Collections;

public class ElectrolysisManager3D : MonoBehaviour
{
    [Header("Quiz Completion")]
    public GameObject successPanel;
    private int correctLabelsCount = 0;

    [Header("3D Water Level Transforms (Pivot at bottom)")]
    public Transform waterAnodePivot; // Oxygen side (Left)
    public Transform waterCathodePivot; // Hydrogen side (Right)

    [Header("Particle Systems")]
    public GameObject bubblesAnode;
    public GameObject bubblesCathode;

    [Header("Hologram UI")]
    public TextMesh hologramText;

    [Header("Simulation Settings")]
    public float oxygenRate = 0.05f;
    public float hydrogenRate = 0.10f; // Ratio 2:1
    public float minWaterScaleY = 0.2f;

    private bool isRunning = false;
    private Vector3 originalAnodeScale;
    private Vector3 originalCathodeScale;

    private AudioSource bubblingAudioSource;
    private RealTimeGraph3D realTimeGraph;
    private float elapsedTime = 0f;
    private GameObject progressRing;
    private Material progressRingMat;

    void Start()
    {
        if (waterAnodePivot != null)
            originalAnodeScale = waterAnodePivot.localScale;
        if (waterCathodePivot != null)
            originalCathodeScale = waterCathodePivot.localScale;

        if (bubblesAnode != null)
            bubblesAnode.SetActive(false);
        if (bubblesCathode != null)
            bubblesCathode.SetActive(false);

        // Find the real-time graph dynamically in the scene
        realTimeGraph = Object.FindAnyObjectByType<RealTimeGraph3D>();

        // Setup AudioSource for loop bubbling
        bubblingAudioSource = gameObject.AddComponent<AudioSource>();
        bubblingAudioSource.clip = CreateBubblingSound();
        bubblingAudioSource.loop = true;
        bubblingAudioSource.playOnAwake = false;
        bubblingAudioSource.spatialBlend = 0.5f; // Semi-spatial 3D
        bubblingAudioSource.volume = 0.35f;

        // Dynamically find and bind the 3D tablet's RESET button
        GameObject resetBtnObj = GameObject.Find("Btn_ResetExperiment");
        if (resetBtnObj != null)
        {
            UnityEngine.UI.Button btn = resetBtnObj.GetComponent<UnityEngine.UI.Button>();
            if (btn != null)
            {
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(ResetExperiment);
            }
        }

        // Reset hologram text at start
        UpdateHologramText(0f, 0f);

        // Setup Progress Ring
        CreateProgressRing();
    }

    void Update()
    {
        if (isRunning)
        {
            RunElectrolysis();
        }
    }

    public void StartElectrolysis()
    {
        if (!isRunning)
        {
            isRunning = true;
            elapsedTime = 0f;

            // Lazy-find the graph in case Start() ran before it was ready
            if (realTimeGraph == null)
                realTimeGraph = Object.FindAnyObjectByType<RealTimeGraph3D>();

            if (realTimeGraph != null)
                realTimeGraph.ResetGraph();

            if (bubblesAnode != null)
                bubblesAnode.SetActive(true);
            if (bubblesCathode != null)
                bubblesCathode.SetActive(true);

            if (bubblingAudioSource != null && !bubblingAudioSource.isPlaying)
            {
                bubblingAudioSource.Play();
            }
        }
    }

    private void RunElectrolysis()
    {
        elapsedTime += Time.deltaTime;

        // Anode (Oxygen)
        if (waterAnodePivot != null && waterAnodePivot.localScale.y > (minWaterScaleY * 2f))
        {
            Vector3 scaleA = waterAnodePivot.localScale;
            scaleA.y -= oxygenRate * Time.deltaTime;
            waterAnodePivot.localScale = scaleA;
        }

        // Cathode (Hydrogen)
        if (waterCathodePivot != null && waterCathodePivot.localScale.y > minWaterScaleY)
        {
            Vector3 scaleC = waterCathodePivot.localScale;
            scaleC.y -= hydrogenRate * Time.deltaTime;
            waterCathodePivot.localScale = scaleC;
        }
        else
        {
            if (isRunning) // only trigger once when hitting the minimum scale
            {
                isRunning = false;
                if (bubblesAnode != null)
                    bubblesAnode.SetActive(false);
                if (bubblesCathode != null)
                    bubblesCathode.SetActive(false);

                if (bubblingAudioSource != null && bubblingAudioSource.isPlaying)
                {
                    bubblingAudioSource.Stop();
                }

                StartCoroutine(AnalysisPhaseRoutine());
            }
        }

        // Update real-time hologram percentages
        float oxygenPct = 0f;
        float hydrogenPct = 0f;

        if (originalAnodeScale.y > 0f)
        {
            oxygenPct = Mathf.Clamp(
                (originalAnodeScale.y - waterAnodePivot.localScale.y)
                    / (originalAnodeScale.y - minWaterScaleY * 2f)
                    * 100f,
                0f,
                100f
            );
        }
        if (originalCathodeScale.y > 0f)
        {
            hydrogenPct = Mathf.Clamp(
                (originalCathodeScale.y - waterCathodePivot.localScale.y)
                    / (originalCathodeScale.y - minWaterScaleY)
                    * 100f,
                0f,
                100f
            );
        }

        UpdateProgressRing(Mathf.Max(oxygenPct, hydrogenPct));

        UpdateHologramText(oxygenPct, hydrogenPct);
        UpdateWaterTurbidity(oxygenPct, hydrogenPct);

        // Feed data to graph
        float oxygenVolume = (oxygenPct / 100f) * 5.0f; // 0 to 5.0 cm³
        float hydrogenVolume = (hydrogenPct / 100f) * 10.0f; // 0 to 10.0 cm³

        // Lazy-find fallback for graph reference
        if (realTimeGraph == null)
            realTimeGraph = Object.FindAnyObjectByType<RealTimeGraph3D>();

        if (realTimeGraph != null)
        {
            realTimeGraph.AddDataPoint(elapsedTime, oxygenVolume, hydrogenVolume);
        }
    }

    public void ResetExperiment()
    {
        Debug.Log("Reset Simulasi button clicked! Reloading the entire scene for a clean slate...");
        UnityEngine.SceneManagement.SceneManager.LoadScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().name
        );
    }

    public void LabelPlacedCorrectly()
    {
        correctLabelsCount++;
        Debug.Log("Label diletakkan! Jumlah label betul dikesan: " + correctLabelsCount);

        // Play droplet splash SFX
        PlaySplashSound();

        if (correctLabelsCount >= 2)
        {
            Debug.Log(
                "Kedua-dua label telah diletakkan. Memanggil Success Panel dalam 1.2 saat..."
            );
            Invoke("ShowSuccessPopup", 1.2f);
        }
    }

    private IEnumerator AnalysisPhaseRoutine()
    {
        Debug.Log("Starting Analysis Phase...");
        
        if (progressRing != null) progressRing.SetActive(false); // Hide progress ring

        // Play scanning sound
        PlayScanningSound();

        // Simulate scanning delay
        yield return new WaitForSeconds(1.5f);
        
        AutoLabelAndComplete();
    }

    private void AutoLabelAndComplete()
    {
        Debug.Log("Electrolysis complete! Auto-labeling tubes...");
        var tut = Object.FindAnyObjectByType<Lab2TutorialManager>();
        if (tut != null) tut.CompleteStep2();

        // Hide any 2D Drag UI buttons from the screen
        GasLabelDragUI[] oldLabels = Object.FindObjectsByType<GasLabelDragUI>(
            FindObjectsInactive.Include
        );
        foreach (var lbl in oldLabels)
        {
            lbl.gameObject.SetActive(false);
        }

        // Create 3D label for Oxygen (Anode)
        if (waterAnodePivot != null && GameObject.Find("OxygenResultLabel") == null)
        {
            GameObject oxLabel = new GameObject("OxygenResultLabel");
            oxLabel.transform.position = waterAnodePivot.position + new Vector3(0f, -0.12f, -0.08f);
            TextMesh tm = oxLabel.AddComponent<TextMesh>();
            tm.text = "Gas Oksigen";
            tm.color = new Color(0.2f, 1.0f, 0.4f, 1f); // Bright green
            tm.characterSize = 0.0035f;
            tm.fontSize = 80;
            tm.anchor = TextAnchor.MiddleCenter;
            tm.alignment = TextAlignment.Center;
            oxLabel.AddComponent<HologramBillboard>();

            // Add a nice splash sound for the pop-up effect
            PlaySplashSound();
        }

        // Create 3D label for Hydrogen (Cathode)
        if (waterCathodePivot != null && GameObject.Find("HydrogenResultLabel") == null)
        {
            GameObject hyLabel = new GameObject("HydrogenResultLabel");
            hyLabel.transform.position =
                waterCathodePivot.position + new Vector3(0f, -0.12f, -0.08f);
            TextMesh tm = hyLabel.AddComponent<TextMesh>();
            tm.text = "Gas Hidrogen";
            tm.color = new Color(0.2f, 1.0f, 0.4f, 1f); // Bright green
            tm.characterSize = 0.0035f;
            tm.fontSize = 80;
            tm.anchor = TextAnchor.MiddleCenter;
            tm.alignment = TextAlignment.Center;
            hyLabel.AddComponent<HologramBillboard>();
        }

        // Trigger the success panel after a short delay
        Invoke("ShowSuccessPopup", 1.5f);
    }

    private void ShowSuccessPopup()
    {
        Debug.Log("Fungsi ShowSuccessPopup() dijalankan.");
        var tut = Object.FindAnyObjectByType<Lab2TutorialManager>();
        if (tut != null) tut.CompleteStep3();
        if (successPanel != null)
        {
            successPanel.SetActive(true);
            Debug.Log("Success Panel diaktifkan!");

            if (PlayerPrefs.GetInt("Lab2_3D_Completed", 0) == 0)
            {
                if (EconomyManager.Instance != null)
                {
                    EconomyManager.Instance.AddXP(500);
                    EconomyManager.Instance.AddCredits(100);
                }
            }
            PlayerPrefs.SetInt("Lab2_3D_Completed", 1);
            PlayerPrefs.Save();

            if (bubblesAnode != null)
                bubblesAnode.SetActive(false);
            if (bubblesCathode != null)
                bubblesCathode.SetActive(false);
        }
        else
        {
            Debug.LogError(
                "RALAT: Anda terlupa menarik objek SuccessPanel_Amali2 ke dalam ruangan 'Success Panel' pada ElectrolysisManager3D_Controller!"
            );
        }
    }

    public void GoToNextExperiment()
    {
        Debug.Log("Loading Next Experiment...");
        // SceneManager.LoadScene(nextSceneIndex);
    }

    private void UpdateHologramText(float ox, float hy)
    {
        if (hologramText != null)
        {
            hologramText.text = $"DATA GAS\nOksigen (O₂): {ox:F1}%\nHidrogen (H₂): {hy:F1}%";
        }
    }

    private void UpdateWaterTurbidity(float oxPct, float hyPct)
    {
        // As reaction progresses, water becomes slightly cloudy/turbid
        if (waterAnodePivot != null)
        {
            Renderer r = waterAnodePivot.GetComponentInChildren<Renderer>();
            if (r != null && r.material != null)
            {
                Color baseColor = r.material.color;
                // Increase alpha slightly based on oxygen percentage (up to +0.2 opacity)
                baseColor.a = Mathf.Lerp(0.3f, 0.5f, oxPct / 100f);
                r.material.color = baseColor;
            }
        }
        
        if (waterCathodePivot != null)
        {
            Renderer r = waterCathodePivot.GetComponentInChildren<Renderer>();
            if (r != null && r.material != null)
            {
                Color baseColor = r.material.color;
                // Increase alpha slightly based on hydrogen percentage
                baseColor.a = Mathf.Lerp(0.3f, 0.5f, hyPct / 100f);
                r.material.color = baseColor;
            }
        }
    }

    private void CreateProgressRing()
    {
        if (waterAnodePivot == null || waterCathodePivot == null) return;
        
        Vector3 centerPos = (waterAnodePivot.position + waterCathodePivot.position) * 0.5f;
        centerPos.y += 0.45f; // Hover above the setup

        progressRing = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        Destroy(progressRing.GetComponent<Collider>());
        progressRing.name = "HoloProgressRing";
        progressRing.transform.position = centerPos;
        progressRing.transform.rotation = Quaternion.Euler(90f, 0f, 0f); // Lay flat
        progressRing.transform.localScale = new Vector3(0.15f, 0.005f, 0.15f);

        progressRingMat = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
        progressRingMat.SetColor("_BaseColor", new Color(0.2f, 0.8f, 1.0f, 0.1f)); // Semi-transparent cyan
        progressRingMat.SetFloat("_Surface", 1f); // Transparent
        progressRingMat.SetInt("_Blend", 0); // Alpha
        progressRingMat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        progressRingMat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        progressRingMat.SetInt("_ZWrite", 0);
        progressRingMat.EnableKeyword("_EMISSION");
        progressRingMat.SetColor("_EmissionColor", new Color(0.2f, 0.8f, 1.0f) * 0.5f);
        
        progressRing.GetComponent<Renderer>().sharedMaterial = progressRingMat;
    }

    private void UpdateProgressRing(float maxPct)
    {
        if (progressRing != null && progressRingMat != null)
        {
            // Rotate the ring based on time and progress
            progressRing.transform.Rotate(0f, 50f * Time.deltaTime, 0f);

            // Scale up slightly as it progresses
            float s = Mathf.Lerp(0.15f, 0.18f, maxPct / 100f);
            progressRing.transform.localScale = new Vector3(s, 0.005f, s);

            // Change color from Cyan to Bright Green
            Color c = Color.Lerp(new Color(0.2f, 0.8f, 1.0f), new Color(0.2f, 1.0f, 0.4f), maxPct / 100f);
            progressRingMat.SetColor("_BaseColor", new Color(c.r, c.g, c.b, 0.4f));
            progressRingMat.SetColor("_EmissionColor", c * Mathf.Lerp(0.5f, 2.0f, maxPct / 100f));
        }
    }

    private AudioClip CreateBubblingSound()
    {
        int samplerate = 44100;
        float duration = 2.0f;
        int numSamples = (int)(samplerate * duration);
        float[] samples = new float[numSamples];

        // Generate random bubble pops (sinusoidal sweeps)
        int numPops = 35;
        float[] popTimes = new float[numPops];
        float[] popFreqs = new float[numPops];
        float[] popDurations = new float[numPops];
        for (int i = 0; i < numPops; i++)
        {
            popTimes[i] = Random.value * duration;
            popFreqs[i] = Random.Range(600f, 1400f);
            popDurations[i] = Random.Range(0.02f, 0.07f);
        }

        for (int i = 0; i < numSamples; i++)
        {
            float t = (float)i / samplerate;
            float sampleVal = 0f;

            // Low level background fizz noise
            float noise = (Random.value * 2f - 1f) * 0.015f;

            for (int p = 0; p < numPops; p++)
            {
                float dt = t - popTimes[p];
                if (dt >= 0 && dt < popDurations[p])
                {
                    float env = Mathf.Exp(-dt * (1f / (popDurations[p] * 0.25f)));
                    float freq = popFreqs[p] + dt * 1500f; // Pitch sweep up
                    float wave = Mathf.Sin(2f * Mathf.PI * freq * dt);
                    sampleVal += wave * env * 0.12f;
                }
            }

            samples[i] = Mathf.Clamp(sampleVal + noise, -1.0f, 1.0f) * 0.35f;
        }

        AudioClip clip = AudioClip.Create("BubblingSFX", numSamples, 1, samplerate, false);
        clip.SetData(samples, 0);
        return clip;
    }

    private void PlaySplashSound()
    {
        int samplerate = 44100;
        float duration = 0.5f;
        int numSamples = (int)(samplerate * duration);
        float[] samples = new float[numSamples];

        for (int i = 0; i < numSamples; i++)
        {
            float t = (float)i / samplerate;

            // Noise splash envelope
            float noiseEnv = Mathf.Exp(-t * 16f);
            float noise =
                (Random.value * 2f - 1f)
                * 0.35f
                * Mathf.Sin(2f * Mathf.PI * (600f + 1000f * (1f - t)) * t)
                * noiseEnv;

            // Bubble droplets
            float chirpVal = 0f;
            int numChirps = 3;
            float[] delays = { 0.02f, 0.08f, 0.14f };
            float[] freqs = { 850f, 1250f, 1050f };
            for (int c = 0; c < numChirps; c++)
            {
                float dt = t - delays[c];
                if (dt >= 0 && dt < 0.08f)
                {
                    float chirpEnv = Mathf.Exp(-dt * 45f);
                    float freq = freqs[c] + dt * 3500f; // Upward sweep
                    chirpVal += Mathf.Sin(2f * Mathf.PI * freq * dt) * chirpEnv * 0.22f;
                }
            }

            samples[i] = Mathf.Clamp(noise + chirpVal, -1.0f, 1.0f) * 0.45f;
        }

        AudioClip clip = AudioClip.Create("SplashSFX", numSamples, 1, samplerate, false);
        clip.SetData(samples, 0);

        AudioSource audio = GetComponent<AudioSource>();
        if (audio == null)
            audio = gameObject.AddComponent<AudioSource>();
        audio.PlayOneShot(clip, 0.8f);
    }

    private void PlayScanningSound()
    {
        int samplerate = 44100;
        float duration = 1.2f;
        int numSamples = (int)(samplerate * duration);
        float[] samples = new float[numSamples];

        for (int i = 0; i < numSamples; i++)
        {
            float t = (float)i / samplerate;
            
            // Rapid high-pitch sweeps (like a digital scanner)
            float sweepFreq = 800f + Mathf.PingPong(t * 3000f, 1500f);
            float wave = Mathf.Sin(2f * Mathf.PI * sweepFreq * t);
            
            // Pulsing envelope
            float envelope = Mathf.Abs(Mathf.Sin(2f * Mathf.PI * 10f * t));
            
            // Add some digital noise
            float noise = (Random.value * 2f - 1f) * 0.1f;
            
            samples[i] = (wave * envelope + noise) * 0.3f;
        }

        AudioClip clip = AudioClip.Create("ScanningSFX", numSamples, 1, samplerate, false);
        clip.SetData(samples, 0);

        AudioSource audio = GetComponent<AudioSource>();
        if (audio == null)
            audio = gameObject.AddComponent<AudioSource>();
        audio.PlayOneShot(clip, 0.6f);
    }
}
