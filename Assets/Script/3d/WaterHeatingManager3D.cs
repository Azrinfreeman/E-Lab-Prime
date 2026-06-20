using UnityEngine;
using TMPro;

public class WaterHeatingManager3D : MonoBehaviour
{
    [Header("2D UI Canvas References (Overlay)")]
    public TextMeshProUGUI tempTextA;
    public TextMeshProUGUI tempTextB;
    public TextMeshProUGUI stateTextA;
    public TextMeshProUGUI stateTextB;
    public TextMeshProUGUI observationTextA;
    public TextMeshProUGUI observationTextB;
    public GameObject warningTextObject;
    public GameObject successPanel;
    public QuizManager quizManager;

    [Header("3D Object References")]
    public Transform waterAPivot; // Beaker A water level scale pivot
    public Transform waterBPivot; // Beaker B water level scale pivot
    public Transform thermoPivotA; // Thermometer A liquid pivot
    public Transform thermoPivotB; // Thermometer B liquid pivot
    public GameObject flameA; // Bunsen burner flame A
    public GameObject flameB; // Bunsen burner flame B
    public GameObject bubblesA; // Boiling bubbles A
    public GameObject bubblesB; // Boiling bubbles B
    public GameObject steamA; // Steam particles A
    public GameObject steamB; // Steam particles B
    public DragGaram3D saltBottle;
    public GameObject safetyGoggles;

    [Header("Advanced Interactive Features")]
    public BunsenCollar3D collarA;
    public BunsenCollar3D collarB;
    public Renderer condensationA;
    public Renderer condensationB;
    public InteractiveNotebook3D notebook;
    public Stopwatch3D stopwatch;
    public Lab3TutorialManager tutorialManager;

    private WaterRipple3D rippleA;
    private WaterRipple3D rippleB;
    private MaterialPropertyBlock propBlockA;
    private MaterialPropertyBlock propBlockB;
    private MeshRenderer mercuryRendererA;
    private MeshRenderer mercuryRendererB;

    private bool isExperimentFinished = false;
    public bool IsExperimentFinished => isExperimentFinished;
    private bool isNotebookLoggingDone = false;

    private Vector3 initialBeakerPosA;
    private Vector3 initialBeakerPosB;
    private Vector3 initialThermoPosA;
    private Vector3 initialThermoPosB;

    [Header("Hologram UI (Floating Display)")]
    public TextMesh hologramText;

    private bool areGogglesEquipped = false;
    public bool AreGogglesEquipped => areGogglesEquipped;

    [Header("Simulation Settings")]
    public float heatingRate = 3.0f; // Speed of temperature rising
    public float evaporationRate = 0.008f; // Speed of water level dropping (slower, more realistic)
    public float minWaterScaleY = 0.02f; // Almost completely empty

    private float tempA = 27f;
    private float tempB = 27f;
    private bool isHeating = false;
    private bool hasImpurities = false;
    private bool isBoilingA = false;
    private bool isBoilingB = false;

    private float boilingPointA = 100f;
    private float boilingPointB = 104f;

    private Vector3 originalWaterScaleA;
    private Vector3 originalWaterScaleB;

    private RealTimeTempGraph3D realTimeGraph;
    private float elapsedTime = 0f;
    private bool quizTriggered = false;

    private AudioSource boilingAudioSource;

    public bool IsHeating => isHeating;
    public bool IsGasOn { get; private set; } = false;

    void Start()
    {
        if (waterAPivot != null) originalWaterScaleA = waterAPivot.localScale;
        if (waterBPivot != null) originalWaterScaleB = waterBPivot.localScale;

        Transform waterA = waterAPivot != null ? waterAPivot.Find("Water") : null;
        if (waterA != null) rippleA = waterA.GetComponent<WaterRipple3D>();

        Transform waterB = waterBPivot != null ? waterBPivot.Find("Water") : null;
        if (waterB != null) rippleB = waterB.GetComponent<WaterRipple3D>();

        propBlockA = new MaterialPropertyBlock();
        propBlockB = new MaterialPropertyBlock();

        if (thermoPivotA != null)
        {
            Transform mercuryA = thermoPivotA.Find("MercuryLevel");
            if (mercuryA != null) mercuryRendererA = mercuryA.GetComponent<MeshRenderer>();
        }
        if (thermoPivotB != null)
        {
            Transform mercuryB = thermoPivotB.Find("MercuryLevel");
            if (mercuryB != null) mercuryRendererB = mercuryB.GetComponent<MeshRenderer>();
        }

        // Store original positions for beaker and thermometer shaking
        GameObject beakerAObj = GameObject.Find("BeakerA_3D");
        GameObject beakerBObj = GameObject.Find("BeakerB_3D");
        if (beakerAObj != null) initialBeakerPosA = beakerAObj.transform.position;
        if (beakerBObj != null) initialBeakerPosB = beakerBObj.transform.position;

        if (thermoPivotA != null && thermoPivotA.parent != null) initialThermoPosA = thermoPivotA.parent.position;
        if (thermoPivotB != null && thermoPivotB.parent != null) initialThermoPosB = thermoPivotB.parent.position;

        if (flameA != null) flameA.SetActive(false);
        if (flameB != null) flameB.SetActive(false);

        if (bubblesA != null) bubblesA.SetActive(false);
        if (bubblesB != null) bubblesB.SetActive(false);

        if (steamA != null) steamA.SetActive(false);
        if (steamB != null) steamB.SetActive(false);

        if (warningTextObject != null) warningTextObject.SetActive(false);

        // Initialize condensation overlays to completely transparent/invisible on start
        if (condensationA != null)
        {
            Color col = condensationA.material.color;
            col.a = 0f;
            condensationA.material.color = col;
        }
        if (condensationB != null)
        {
            Color col = condensationB.material.color;
            col.a = 0f;
            condensationB.material.color = col;
        }

        realTimeGraph = Object.FindAnyObjectByType<RealTimeTempGraph3D>();

        // Set up AudioSource for boiling water sounds
        boilingAudioSource = gameObject.AddComponent<AudioSource>();
        boilingAudioSource.clip = CreateBoilingSound();
        boilingAudioSource.loop = true;
        boilingAudioSource.playOnAwake = false;
        boilingAudioSource.volume = 0.08f;
        boilingAudioSource.spatialBlend = 0.5f;

        // Dynamically bind the Reset button on the tablet
        GameObject resetBtnObj = GameObject.Find("Btn_ResetExperiment");
        if (resetBtnObj != null)
        {
            UnityEngine.UI.Button btn = resetBtnObj.GetComponent<UnityEngine.UI.Button>();
            if (btn != null)
            {
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(ReloadScene);
            }
        }

        // Dynamically bind the Gas button on the tablet
        GameObject startHeatBtnObj = GameObject.Find("Btn_StartHeating");
        if (startHeatBtnObj != null)
        {
            UnityEngine.UI.Button btn = startHeatBtnObj.GetComponent<UnityEngine.UI.Button>();
            if (btn != null)
            {
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(ToggleGas);
                TextMeshProUGUI btnText = startHeatBtnObj.GetComponentInChildren<TextMeshProUGUI>();
                if (btnText != null) btnText.text = "BUKA GAS";
                UnityEngine.UI.Text legacyText = startHeatBtnObj.GetComponentInChildren<UnityEngine.UI.Text>();
                if (legacyText != null) legacyText.text = "BUKA GAS";
            }
        }

        // Dynamically bind POV buttons
        string[] buttonNames = { "Btn_FrontView", "Btn_RightView", "Btn_LeftView", "Btn_BehindView", "Btn_TopView", "Btn_TextbookView", "Btn_ThermoA", "Btn_ThermoB" };
        for (int i = 0; i < buttonNames.Length; i++)
        {
            GameObject povBtnObj = GameObject.Find(buttonNames[i]);
            if (povBtnObj != null)
            {
                UnityEngine.UI.Button btn = povBtnObj.GetComponent<UnityEngine.UI.Button>();
                if (btn != null)
                {
                    btn.onClick.RemoveAllListeners();
                    int index = i; // capture index for lambda
                    btn.onClick.AddListener(() => TransitionPOV(index));
                }
            }
        }

        ResetUI();
    }

    void Update()
    {
        if (isHeating)
        {
            elapsedTime += Time.deltaTime;

            // --- BEAKER A (PURE WATER) ---
            float speedMultA = (collarA != null && collarA.IsAirHoleOpen) ? 1.0f : 0.2f;
            if (tempA < boilingPointA)
            {
                tempA += heatingRate * speedMultA * Time.deltaTime;
                if (stateTextA != null) 
                    stateTextA.text = (collarA != null && !collarA.IsAirHoleOpen) ? "State: Heating (Slow - Buka Lubang Udara!)" : "State: Heating...";
            }
            else
            {
                tempA = boilingPointA;
                if (!isBoilingA)
                {
                    isBoilingA = true;
                    if (stateTextA != null) stateTextA.text = "State: Boiling!";
                    if (observationTextA != null) observationTextA.text = "Boiling Point A: " + boilingPointA.ToString("F1") + " °C";
                    PlayBoilingSound();
                }

                // Evaporate 3D water (Pure water evaporates faster)
                EvaporateWater(waterAPivot, originalWaterScaleA, bubblesA, steamA, 1.0f);
            }

            // --- BEAKER B (IMPURE WATER) ---
            float speedMultB = (collarB != null && collarB.IsAirHoleOpen) ? 1.0f : 0.2f;
            if (tempB < boilingPointB)
            {
                tempB += heatingRate * speedMultB * Time.deltaTime;
                if (stateTextB != null) 
                    stateTextB.text = (collarB != null && !collarB.IsAirHoleOpen) ? "State: Heating (Slow - Buka Lubang Udara!)" : "State: Heating...";
            }
            else
            {
                tempB = boilingPointB;
                if (!isBoilingB)
                {
                    isBoilingB = true;
                    if (stateTextB != null) stateTextB.text = "State: Boiling!";
                    if (observationTextB != null) observationTextB.text = "Boiling Point B: " + boilingPointB.ToString("F1") + " °C";
                    PlayBoilingSound();
                }

                // Evaporate 3D water (Salt water evaporates slower)
                EvaporateWater(waterBPivot, originalWaterScaleB, bubblesB, steamB, 0.5f);
            }

            // Update UI
            UpdateTempUI();
            UpdateHologramText();

            // Update Water Ripple speeds
            if (rippleA != null) rippleA.currentTemp = tempA;
            if (rippleB != null) rippleB.currentTemp = tempB;

            // Update Thermometer Mercury Glow
            UpdateThermometerGlow(tempA, mercuryRendererA, propBlockA);
            UpdateThermometerGlow(tempB, mercuryRendererB, propBlockB);

            // Dynamic progression of bubbles, steam, condensation, and shaking
            UpdateBoilingProgression(tempA, bubblesA, steamA, condensationA, "BeakerA_3D", thermoPivotA);
            UpdateBoilingProgression(tempB, bubblesB, steamB, condensationB, "BeakerB_3D", thermoPivotB);

            // Scale boiling sound volume dynamically based on maximum temperature
            float maxTemp = Mathf.Max(tempA, tempB);
            if (maxTemp > 50f && boilingAudioSource != null && boilingAudioSource.isPlaying)
            {
                boilingAudioSource.volume = Mathf.Clamp01((maxTemp - 50f) / 50f) * 0.08f;
            }

            // Plot data points to graph
            if (realTimeGraph != null)
            {
                realTimeGraph.AddDataPoint(elapsedTime, tempA, tempB);
            }

            // Trigger Experiment Finished State (Wait for Notebook Data Logging instead of instantly starting quiz)
            if (isBoilingA && isBoilingB && !isExperimentFinished)
            {
                isExperimentFinished = true;
                if (stopwatch != null) stopwatch.StopStopwatch();
                Debug.Log("Both beakers boiled. Experiment finished. Awaiting notebook data logging.");
                UpdateHologramText();
            }
        }
    }

    public void ResetExperiment()
    {
        isExperimentFinished = false;
        isNotebookLoggingDone = false;
        quizTriggered = false;

        if (stopwatch != null) stopwatch.ResetStopwatch();

        tempA = 27f;
        tempB = 27f;
        isHeating = false;
        hasImpurities = false;
        isBoilingA = false;
        isBoilingB = false;
        elapsedTime = 0f;
        IsGasOn = false;

        if (waterAPivot != null) waterAPivot.localScale = originalWaterScaleA;
        if (waterBPivot != null) waterBPivot.localScale = originalWaterScaleB;

        if (flameA != null) flameA.SetActive(false);
        if (flameB != null) flameB.SetActive(false);

        if (bubblesA != null) bubblesA.SetActive(false);
        if (bubblesB != null) bubblesB.SetActive(false);

        if (steamA != null) steamA.SetActive(false);
        if (steamB != null) steamB.SetActive(false);

        if (boilingAudioSource != null) boilingAudioSource.Stop();

        UpdateTempUI();
        UpdateHologramText();
        
        if (realTimeGraph != null) realTimeGraph.ResetGraph();
        
        // Reset button states
        GameObject startHeatBtnObj = GameObject.Find("Btn_StartHeating");
        if (startHeatBtnObj != null)
        {
            UnityEngine.UI.Text legacyText = startHeatBtnObj.GetComponentInChildren<UnityEngine.UI.Text>();
            if (legacyText != null) legacyText.text = "BUKA GAS";
            UnityEngine.UI.Image btnImage = startHeatBtnObj.GetComponent<UnityEngine.UI.Image>();
            if (btnImage != null) btnImage.color = new Color(0.15f, 0.7f, 0.15f);
        }
    }

    private void EvaporateWater(Transform waterPivot, Vector3 originalScale, GameObject bubbles, GameObject steam, float evaporationMultiplier)
    {
        if (waterPivot != null && waterPivot.localScale.y > minWaterScaleY)
        {
            Vector3 scale = waterPivot.localScale;
            scale.y -= (evaporationRate * evaporationMultiplier) * Time.deltaTime;
            if (scale.y < minWaterScaleY) scale.y = minWaterScaleY;
            waterPivot.localScale = scale;

            // Calculate ratio of remaining water (from 1.0 down to 0.0)
            float range = originalScale.y - minWaterScaleY;
            float currentOffset = scale.y - minWaterScaleY;
            float ratio = (range > 0f) ? Mathf.Clamp01(currentOffset / range) : 0f;

            // Scale particle emission based on remaining water
            if (bubbles != null)
            {
                var ps = bubbles.GetComponent<ParticleSystem>();
                if (ps != null)
                {
                    var emission = ps.emission;
                    emission.rateOverTime = 80f * ratio; // Fades from 80 down to 0
                }
            }
            if (steam != null)
            {
                var ps = steam.GetComponent<ParticleSystem>();
                if (ps != null)
                {
                    var emission = ps.emission;
                    emission.rateOverTime = 12f * ratio; // Fades from 12 down to 0
                }
            }
        }
        else if (waterPivot != null && waterPivot.localScale.y <= minWaterScaleY)
        {
            // If water is at minimum, stop bubbles and steam completely
            if (bubbles != null)
            {
                var ps = bubbles.GetComponent<ParticleSystem>();
                if (ps != null)
                {
                    var emission = ps.emission;
                    emission.rateOverTime = 0f;
                }
            }
            if (steam != null)
            {
                var ps = steam.GetComponent<ParticleSystem>();
                if (ps != null)
                {
                    var emission = ps.emission;
                    emission.rateOverTime = 0f;
                }
            }
        }
    }

    public void AddImpurities()
    {
        hasImpurities = true;
        HideWarning();
        PlaySplashSound();
        if (tutorialManager != null) tutorialManager.CompleteStep1();
        Debug.Log("Bendasing (garam) ditambahkan ke Beaker B.");
    }

    public void ToggleGas()
    {
        if (isHeating) return; // Can't toggle if already heating/ignited

        IsGasOn = !IsGasOn;

        GameObject startHeatBtnObj = GameObject.Find("Btn_StartHeating");
        if (startHeatBtnObj != null)
        {
            TMPro.TextMeshProUGUI btnText = startHeatBtnObj.GetComponentInChildren<TMPro.TextMeshProUGUI>();
            if (btnText != null) btnText.text = IsGasOn ? "TUTUP GAS" : "BUKA GAS";
            UnityEngine.UI.Text legacyText = startHeatBtnObj.GetComponentInChildren<UnityEngine.UI.Text>();
            if (legacyText != null) legacyText.text = IsGasOn ? "TUTUP GAS" : "BUKA GAS";
            UnityEngine.UI.Image btnImage = startHeatBtnObj.GetComponent<UnityEngine.UI.Image>();
            if (btnImage != null) btnImage.color = IsGasOn ? new Color(0.8f, 0.2f, 0.2f) : new Color(0.15f, 0.7f, 0.15f);
        }

        // Toggle Lighter visual guide indicator
        GameObject lighter = GameObject.Find("Lighter_3D");
        if (lighter != null)
        {
            DragLighter3D dragLighter = lighter.GetComponent<DragLighter3D>();
            if (dragLighter != null)
            {
                dragLighter.SetIndicatorActive(IsGasOn);
            }
        }
        
        if (IsGasOn && tutorialManager != null) tutorialManager.CompleteStep3();
    }

    public bool StartHeating()
    {
        Debug.Log("Start Heating clicked! hasImpurities (Salt Added) = " + hasImpurities);

        if (!areGogglesEquipped)
        {
            Debug.Log("Cannot start heating! Safety goggles not equipped.");
            ShowWarning();
            if (safetyGoggles != null)
            {
                GogglesInteraction3D gogglesScript = safetyGoggles.GetComponent<GogglesInteraction3D>();
                if (gogglesScript != null)
                {
                    gogglesScript.SetIndicatorActive(true);
                }
            }
            if (hologramText != null)
            {
                hologramText.color = Color.red;
                hologramText.text = "RALAT KESELAMATAN:\nSila pakai cermin mata\nkeselamatan dahulu!";
                Invoke("ResetHologramColor", 3.0f);
            }
            PlayWarningSound();
            return false;
        }

        if (!IsGasOn)
        {
            Debug.Log("Cannot start heating! Gas is not on.");
            ShowWarning();
            if (hologramText != null)
            {
                hologramText.color = Color.red;
                hologramText.text = "RALAT:\nSila BUKA GAS pada tablet dahulu!";
                Invoke("ResetHologramColor", 3.0f);
            }
            return false;
        }

        if (hasImpurities && !isHeating)
        {
            isHeating = true;
            if (flameA != null) flameA.SetActive(true);
            if (flameB != null) flameB.SetActive(true);
            if (stopwatch != null) stopwatch.StartStopwatch();
            if (tutorialManager != null) tutorialManager.CompleteStep4();
            HideWarning();

            if (realTimeGraph == null)
                realTimeGraph = Object.FindAnyObjectByType<RealTimeTempGraph3D>();
            if (realTimeGraph != null) realTimeGraph.ResetGraph();

            Debug.Log("Heating started successfully!");
            UpdateHologramText();
            return true;
        }
        else if (!hasImpurities)
        {
            Debug.Log("Cannot start heating! Salt not added yet.");
            ShowWarning();
            if (hologramText != null)
            {
                hologramText.color = Color.red;
                hologramText.text = "RALAT: Sila tambah garam\n(botol putih) ke Beaker B dahulu!";
                Invoke("ResetHologramColor", 3.0f);
            }
            return false;
        }
        return false;
    }

    private void ResetHologramColor()
    {
        if (hologramText != null)
        {
            hologramText.color = new Color(0f, 0.9f, 1f, 1f); // Default cyan
            UpdateHologramText();
        }
    }

    private void ShowWarning()
    {
        if (warningTextObject != null)
        {
            warningTextObject.SetActive(true);
            Invoke("HideWarning", 2.5f);
        }
    }

    private void HideWarning()
    {
        if (warningTextObject != null)
        {
            warningTextObject.SetActive(false);
        }
    }

    private void TriggerQuizPopup()
    {
        if (quizManager != null)
        {
            quizManager.ShowQuiz();
        }
        else if (successPanel != null)
        {
            successPanel.SetActive(true);
        }
    }

    private void UpdateTempUI()
    {
        if (tempTextA != null) tempTextA.text = tempA.ToString("F1") + " °C";
        if (tempTextB != null) tempTextB.text = tempB.ToString("F1") + " °C";

        // Update physical thermometer liquid height
        float maxTemp = 110f;
        float maxScale = 0.85f;
        
        if (thermoPivotA != null)
        {
            Vector3 s = thermoPivotA.localScale;
            s.y = (tempA / maxTemp) * maxScale;
            thermoPivotA.localScale = s;
        }
        if (thermoPivotB != null)
        {
            Vector3 s = thermoPivotB.localScale;
            s.y = (tempB / maxTemp) * maxScale;
            thermoPivotB.localScale = s;
        }
    }

    private void UpdateHologramText()
    {
        if (hologramText != null)
        {
            string stateA = isBoilingA ? "MENDIDIH!" : (isHeating ? (((collarA != null && !collarA.IsAirHoleOpen) ? "LEMAH (Tutup)" : "PANAS (Buka)")) : "SEDIA");
            string stateB = isBoilingB ? "MENDIDIH!" : (isHeating ? (((collarB != null && !collarB.IsAirHoleOpen) ? "LEMAH (Tutup)" : "PANAS (Buka)")) : "SEDIA");
            
            if (isExperimentFinished && !isNotebookLoggingDone)
            {
                hologramText.text = $"EKSPERIMEN SELESAI!\n\nBeaker A: {tempA:F1}°C (MENDIDIH)\nBeaker B: {tempB:F1}°C (MENDIDIH)\n\n[KLIK BUKU NOTA DI ATAS MEJA\nUNTUK CATAT TAKAT DIDIH]";
            }
            else if (isNotebookLoggingDone)
            {
                hologramText.text = $"REKOD DATA SELESAI!\n\nTakat Didih A: 100°C\nTakat Didih B: 104°C\n\nSila jawab kuiz pada tablet!";
            }
            else if (isHeating && ((collarA != null && !collarA.IsAirHoleOpen) || (collarB != null && !collarB.IsAirHoleOpen)))
            {
                hologramText.text = $"STATUS PEMANASAN\n\nBeaker A: {tempA:F1}°C ({stateA})\nBeaker B: {tempB:F1}°C ({stateB})\n\n[SILA BUKA LUBANG UDARA\nPENUNU BUNSEN!]";
            }
            else
            {
                hologramText.text = $"STATUS PEMANASAN\n\nBeaker A: {tempA:F1}°C ({stateA})\nBeaker B: {tempB:F1}°C ({stateB})";
            }
        }
    }

    public void ShowHologramTemporaryMessage(string message, float duration, Color color)
    {
        if (hologramText != null)
        {
            CancelInvoke("ResetHologramColor");
            hologramText.color = color;
            hologramText.text = message;
            Invoke("ResetHologramColor", duration);
        }
    }

    public void CompleteNotebookLogging()
    {
        isNotebookLoggingDone = true;
        UpdateHologramText();
        
        // Trigger the quiz popup after a short delay
        if (!quizTriggered)
        {
            quizTriggered = true;
            Invoke("TriggerQuizPopup", 1.5f);
        }
    }

    private void UpdateBoilingProgression(float temp, GameObject bubbles, GameObject steam, Renderer condensation, string beakerName, Transform thermoPivot)
    {
        // 1. Bubbles and Steam Particle system adjustments
        if (bubbles != null)
        {
            var ps = bubbles.GetComponent<ParticleSystem>();
            if (ps != null)
            {
                var emission = ps.emission;
                var main = ps.main;
                
                if (temp < 50f)
                {
                    emission.rateOverTime = 0f;
                    if (bubbles.activeSelf) bubbles.SetActive(false);
                }
                else
                {
                    if (!bubbles.activeSelf) bubbles.SetActive(true);
                    
                    if (temp >= 50f && temp < 80f)
                    {
                        // Tiny bubbles (nucleation)
                        emission.rateOverTime = 15f;
                        main.startSize = new ParticleSystem.MinMaxCurve(0.006f, 0.012f);
                        main.startSpeed = new ParticleSystem.MinMaxCurve(0.4f, 0.6f);
                    }
                    else if (temp >= 80f && temp < 100f)
                    {
                        // Moderate boiling
                        emission.rateOverTime = 45f;
                        main.startSize = new ParticleSystem.MinMaxCurve(0.012f, 0.024f);
                        main.startSpeed = new ParticleSystem.MinMaxCurve(0.8f, 1.2f);
                    }
                    else
                    {
                        // Full boil
                        emission.rateOverTime = 80f;
                        main.startSize = new ParticleSystem.MinMaxCurve(0.025f, 0.04f);
                        main.startSpeed = new ParticleSystem.MinMaxCurve(1.2f, 2.0f);
                    }
                }
            }
        }

        if (steam != null)
        {
            var ps = steam.GetComponent<ParticleSystem>();
            if (ps != null)
            {
                var emission = ps.emission;
                
                if (temp < 80f)
                {
                    emission.rateOverTime = 0f;
                    if (steam.activeSelf) steam.SetActive(false);
                }
                else
                {
                    if (!steam.activeSelf) steam.SetActive(true);
                    
                    if (temp >= 80f && temp < 100f)
                    {
                        emission.rateOverTime = 4f;
                    }
                    else
                    {
                        emission.rateOverTime = 12f;
                    }
                }
            }
        }

        // 2. Condensation fade-in (70 degC to 100 degC)
        if (condensation != null)
        {
            float tAlpha = 0f;
            if (temp >= 70f)
            {
                tAlpha = Mathf.Clamp01((temp - 70f) / 30f) * 0.35f; // Max 0.35 transparency (subtle and realistic)
            }
            
            // Accessing material locally instantiates a unique clone so A and B don't share alpha
            Color col = condensation.material.color;
            col.a = tAlpha;
            condensation.material.color = col;
        }

        // 3. Beaker & Thermometer micro-shaking (above 90 degC)
        GameObject beaker = GameObject.Find(beakerName);
        if (beaker != null)
        {
            Vector3 originalBeakerPos = (beakerName == "BeakerA_3D") ? initialBeakerPosA : initialBeakerPosB;
            Vector3 originalThermoPos = (beakerName == "BeakerA_3D") ? initialThermoPosA : initialThermoPosB;

            if (temp >= 90f)
            {
                // Intensity grows as temp goes from 90 to 100+
                float intensity = Mathf.Clamp01((temp - 90f) / 10f);
                float shakeScale = 0.0007f * intensity;
                
                Vector3 shakeOffset = new Vector3(
                    (Random.value * 2f - 1f) * shakeScale,
                    (Random.value * 2f - 1f) * shakeScale * 0.3f, // less shaking vertically
                    (Random.value * 2f - 1f) * shakeScale
                );
                
                beaker.transform.position = originalBeakerPos + shakeOffset;
                
                if (thermoPivot != null && thermoPivot.parent != null)
                {
                    thermoPivot.parent.position = originalThermoPos + shakeOffset;
                }
            }
            else
            {
                // Reset positions
                beaker.transform.position = originalBeakerPos;
                if (thermoPivot != null && thermoPivot.parent != null)
                {
                    thermoPivot.parent.position = originalThermoPos;
                }
            }
        }
    }

    private void ResetUI()
    {
        UpdateTempUI();
        UpdateHologramText();
        if (stateTextA != null) stateTextA.text = "State: Ready";
        if (stateTextB != null) stateTextB.text = "State: Ready";
        if (observationTextA != null) observationTextA.text = "Boiling Point A: ___ °C";
        if (observationTextB != null) observationTextB.text = "Boiling Point B: ___ °C";
    }

    public void ReloadScene()
    {
        Debug.Log("Reset Simulasi button clicked! Reloading current scene...");
        UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
    }

    private void PlayBoilingSound()
    {
        if (boilingAudioSource != null && !boilingAudioSource.isPlaying)
        {
            boilingAudioSource.Play();
        }
    }

    private AudioClip CreateBoilingSound()
    {
        int samplerate = 44100;
        float duration = 1.0f;
        int numSamples = (int)(samplerate * duration);
        float[] samples = new float[numSamples];
        
        for (int i = 0; i < numSamples; i++)
        {
            float t = (float)i / samplerate;
            float noise = (Random.value * 2f - 1f) * 0.05f;
            float sine = Mathf.Sin(2f * Mathf.PI * 80f * t) * 0.05f; // Low rumble
            samples[i] = noise + sine;
        }
        
        AudioClip clip = AudioClip.Create("BoilingSFX", numSamples, 1, samplerate, false);
        clip.SetData(samples, 0);
        return clip;
    }

    private void PlaySplashSound()
    {
        int samplerate = 44100;
        float duration = 0.3f;
        int numSamples = (int)(samplerate * duration);
        float[] samples = new float[numSamples];
        for (int i = 0; i < numSamples; i++)
        {
            float t = (float)i / samplerate;
            float noise = (Random.value * 2f - 1f) * 0.1f * Mathf.Exp(-t * 20f);
            samples[i] = noise;
        }
        AudioClip clip = AudioClip.Create("SplashSFX", numSamples, 1, samplerate, false);
        clip.SetData(samples, 0);
        
        AudioSource audio = GetComponent<AudioSource>();
        if (audio == null) audio = gameObject.AddComponent<AudioSource>();
        audio.PlayOneShot(clip, 0.8f);
    }

    public void TransitionPOV(int index)
    {
        CameraDragArea dragArea = Object.FindAnyObjectByType<CameraDragArea>();
        if (dragArea != null)
        {
            if (index == 0) dragArea.SetFrontView();
            else if (index == 1) dragArea.SetRightView();
            else if (index == 2) dragArea.SetLeftView();
            else if (index == 3) dragArea.SetBehindView();
            else if (index == 4) dragArea.SetTopView();
            else if (index == 5) dragArea.SetTextbookView();
            else if (index == 6) dragArea.SetThermoAView();
            else if (index == 7) dragArea.SetThermoBView();
        }
        else
        {
            Debug.LogWarning("CameraDragArea not found in the scene to transition camera POV.");
        }
    }



    private void UpdateThermometerGlow(float temp, MeshRenderer renderer, MaterialPropertyBlock propBlock)
    {
        if (renderer == null || propBlock == null) return;
        
        // Glow gets stronger as it approaches 100
        float intensity = Mathf.Clamp01((temp - 30f) / 70f);
        if (intensity > 0)
        {
            Color glowColor = new Color(1f, 0.1f, 0.1f) * intensity * 2.5f; // Bright red glow
            renderer.GetPropertyBlock(propBlock);
            propBlock.SetColor("_EmissionColor", glowColor);
            renderer.SetPropertyBlock(propBlock);
        }
    }
    public void EquipGoggles()
    {
        areGogglesEquipped = true;
        Debug.Log("Safety Goggles equipped!");
        
        if (hologramText != null)
        {
            hologramText.color = new Color(0f, 1f, 0.5f); // Soft green
            hologramText.text = "INFO KESELAMATAN:\nCermin mata keselamatan\ntelah dipakai.";
            Invoke("ResetHologramColor", 2.0f);
        }
    }

    private void PlayWarningSound()
    {
        int samplerate = 44100;
        float duration = 0.4f;
        int numSamples = (int)(samplerate * duration);
        float[] samples = new float[numSamples];
        for (int i = 0; i < numSamples; i++)
        {
            float t = (float)i / samplerate;
            float buzz = Mathf.Sign(Mathf.Sin(2f * Mathf.PI * 180f * t));
            float noise = (Random.value * 2f - 1f) * 0.15f;
            samples[i] = (buzz + noise) * 0.25f * Mathf.Exp(-t * 4f);
        }
        AudioClip clip = AudioClip.Create("WarningSFX", numSamples, 1, samplerate, false);
        clip.SetData(samples, 0);
        
        AudioSource audio = GetComponent<AudioSource>();
        if (audio == null) audio = gameObject.AddComponent<AudioSource>();
        audio.PlayOneShot(clip, 1.0f);
    }
}
