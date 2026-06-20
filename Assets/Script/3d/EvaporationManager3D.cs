using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class EvaporationManager3D : MonoBehaviour
{
    [Header("3D Object References")]
    [Tooltip("Pivot Transform for the water inside the beaker/petri dish. Should scale on Y axis.")]
    public Transform waterPivot; 
    
    [Tooltip("The Beaker object itself, which will scale horizontally when the area slider is adjusted.")]
    public Transform beakerTransform;

    [Tooltip("Pivot Transform for the fan blades to rotate.")]
    public Transform fanBladesPivot;   

    [Header("Heater Hot Plate 3D References")]
    [Tooltip("The Point or Spot Light representing the heat source.")]
    public Light heatLampLight; 
    [Tooltip("The Renderer of the hot plate heating surface to change its emission color/intensity.")]
    public Renderer heatLampBulb;
    [Tooltip("The rotating dial knob on the heater.")]
    public Transform heaterKnob;
    [Tooltip("The LED indicator light on the heater.")]
    public Renderer ledIndicator;

    [Tooltip("Maximum intensity of the heat light.")]
    public float maxLightIntensity = 5.0f;
    [ColorUsage(true, true)]
    public Color heatEmissionColor = new Color(1f, 0.4f, 0f);
    [ColorUsage(true, true)]
    public Color ledOnColor = new Color(2.0f, 0f, 0f);

    [Header("Particle System References")]
    public ParticleSystem particleWap;   
    public ParticleSystem particleAngin; 

    [Header("Control Sliders (UI)")]
    public Slider tempSlider;
    public Slider windSlider;
    public Slider areaSlider;
    public Slider humiditySlider;

    [Header("UI Text References")]
    public TextMeshProUGUI evaporationRateText;
    [Tooltip("Optional 3D Hologram Text to display status")]
    public TextMesh hologramText;

    [Header("Interactive Elements")]
    public Lab4TutorialManager tutorialManager;
    public bool IsExperimentFinished { get; private set; } = false;

    [Tooltip("Reference to the 3D Evaporation Graph on the notebook page.")]
    public EvaporationGraph3D graph3D;

    [Header("Power Cables")]
    public WirePulseAnimator heaterCableAnimator;
    public WirePulseAnimator fanCableAnimator;

    [Header("Physical Gauges")]
    public Thermometer3D thermometer;

    [Header("Audio Settings")]
    public AudioSource dialClickAudio;
    public HeaterPowerSwitch3D powerSwitch;

    [Header("Heat Haze & Condensation")]
    public ParticleSystem particleHaze;
    public BeakerCondensation3D beakerCondensation;

    [Header("Container Swapping")]
    public GameObject cylinderGroup;
    public GameObject beakerGroup;
    public GameObject petriGroup;
    public Thermometer3D[] thermometers;

    [Header("Engine Settings")]
    public float baseSpeed = 0.05f; 
    
    // To allow some customization on which axis the fan rotates
    public Vector3 fanRotationAxis = new Vector3(0, 0, 1); 

    private bool isEvaporationStarted = false;
    private float elapsedTime = 0f;
    private float currentRate = 0f;
    private MaterialPropertyBlock bulbPropBlock;
    private Vector3 initialWaterScale;
    private Vector3 initialBeakerScale;

    private AudioSource fanAudio;
    private AudioSource boilingAudio;

    private float waterVolumePercentage = 1f;
    private Transform activeWaterPivot;
    private BeakerCondensation3D activeCondensation;
    private float activeInitialWaterScaleY = 1.0f;
    private float activeWaterVolumeLocalY = 0.2f;
    private TMPro.TextMeshProUGUI areaLabelText;
    private float lastSnappedTemp = 0.0f;
    private bool isHeaterPowerOn = false;

    void Start()
    {
        if (particleWap != null) particleWap.Stop();
        if (graph3D != null) graph3D.ResetGraph();
        
        bulbPropBlock = new MaterialPropertyBlock();
        
        // Find Area slider's ValueText child
        if (areaSlider != null)
        {
            areaLabelText = areaSlider.transform.Find("ValueText")?.GetComponent<TMPro.TextMeshProUGUI>();
        }

        UpdateContainerSelection();

        if (activeWaterPivot != null)
        {
            initialWaterScale = activeWaterPivot.localScale;
        }
        
        if (beakerTransform != null)
        {
            initialBeakerScale = beakerTransform.localScale;
        }

        if (beakerGroup != null)
        {
            boilingAudio = beakerGroup.GetComponent<AudioSource>();
        }

        // Dynamically bind Start and Reset buttons at runtime
        GameObject tablet = GameObject.Find("PhysicalControlBox");
        if (tablet == null) tablet = GameObject.Find("Lab4_3DTablet"); // Fallback
        
        if (tablet != null)
        {
            Transform rightTrans = tablet.transform.Find("TabletCanvas/Right");
            if (rightTrans != null)
            {
                Button startBtn = rightTrans.Find("Bottom/Button")?.GetComponent<Button>();
                if (startBtn != null)
                {
                    startBtn.onClick.RemoveAllListeners();
                    startBtn.onClick.AddListener(StartExperiment);
                }

                Button resetBtn = rightTrans.Find("Bottom/ResetBtn")?.GetComponent<Button>();
                if (resetBtn != null)
                {
                    resetBtn.onClick.RemoveAllListeners();
                    resetBtn.onClick.AddListener(ResetEksperimen);
                }
            }
        }
    }

    private void UpdateContainerSelection()
    {
        if (areaSlider == null) return;

        // Snap area slider to 0, 0.5, or 1.0
        float snappedArea = Mathf.Round(areaSlider.value / 0.5f) * 0.5f;

        if (cylinderGroup != null) cylinderGroup.SetActive(snappedArea == 0f);
        if (beakerGroup != null) beakerGroup.SetActive(snappedArea == 0.5f);
        if (petriGroup != null) petriGroup.SetActive(snappedArea == 1.0f);

        // Update labels
        if (areaLabelText != null)
        {
            if (snappedArea == 0f) areaLabelText.text = "Silinder Penyukat";
            else if (snappedArea == 0.5f) areaLabelText.text = "Bikar";
            else areaLabelText.text = "Piring Petri";
        }

        // Cache active references
        GameObject activeGroup = null;
        if (snappedArea == 0f) activeGroup = cylinderGroup;
        else if (snappedArea == 0.5f) activeGroup = beakerGroup;
        else activeGroup = petriGroup;

        if (activeGroup != null)
        {
            // Find Water_Pivot in the active group
            activeWaterPivot = activeGroup.transform.Find("Water_Pivot");
            if (activeWaterPivot != null)
            {
                waterPivot = activeWaterPivot;
                // Find WaterVolume inside Water_Pivot
                Transform waterVol = activeWaterPivot.Find("WaterVolume");
                if (waterVol != null)
                {
                    activeWaterVolumeLocalY = waterVol.localScale.y;
                }
            }
            
            beakerTransform = activeGroup.transform;
            
            // Find BeakerCondensation in active model
            activeCondensation = activeGroup.GetComponentInChildren<BeakerCondensation3D>(true);
            beakerCondensation = activeCondensation;

            // Update boiling audio reference
            boilingAudio = activeGroup.GetComponent<AudioSource>();
            if (boilingAudio == null) boilingAudio = activeGroup.GetComponentInChildren<AudioSource>();
        }
    }

    void Update()
    {
        if (tempSlider == null || windSlider == null || areaSlider == null || humiditySlider == null) return;

        // Snap temperature slider to mechanical increments of 0.2 (0 to 5)
        float snappedTemp = Mathf.Round(tempSlider.value / 0.2f) * 0.2f;
        if (Mathf.Abs(tempSlider.value - snappedTemp) > 0.001f)
        {
            tempSlider.value = snappedTemp;
        }

        if (Mathf.Abs(lastSnappedTemp - snappedTemp) > 0.001f)
        {
            lastSnappedTemp = snappedTemp;
            if (dialClickAudio != null)
            {
                dialClickAudio.PlayOneShot(dialClickAudio.clip);
            }
        }

        // Snap area slider to discrete steps of 0.5 (cylinder, beaker, petri dish)
        float snappedArea = Mathf.Round(areaSlider.value / 0.5f) * 0.5f;
        if (Mathf.Abs(areaSlider.value - snappedArea) > 0.001f)
        {
            areaSlider.value = snappedArea;
            UpdateContainerSelection();
        }

        if (bulbPropBlock == null)
        {
            bulbPropBlock = new MaterialPropertyBlock();
        }

        // 1. KIRA KADAR PENYEJATAN
        // Use a physical surface area factor based on the container type
        float areaFactor = 0.15f; // Cylinder (very small)
        if (snappedArea == 0.5f) areaFactor = 0.6f; // Beaker (medium)
        else if (snappedArea == 1.0f) areaFactor = 2.2f; // Petri Dish (very wide)

        float tempFactor = isHeaterPowerOn ? tempSlider.value : 0f;
        float positiveFactors = tempFactor + windSlider.value + areaFactor;
        float negativeFactors = humiditySlider.value;
        currentRate = (positiveFactors - (negativeFactors * 0.8f)) * baseSpeed;

        if (currentRate < 0.01f) currentRate = 0.01f;

        if (evaporationRateText != null)
        {
            evaporationRateText.text = "Kadar Penyejatan: " + currentRate.ToString("F3") + " unit/s";
        }
        
        if (hologramText != null)
        {
            hologramText.text = "Kadar Penyejatan:\n" + currentRate.ToString("F3") + " unit/s";
        }

        // 2. KAWAL KIPAS (3D) & PARTICLE ANGIN
        if (fanBladesPivot != null)
        {
            float putaranKipas = windSlider.value * 500f * Time.deltaTime;
            fanBladesPivot.Rotate(fanRotationAxis * -putaranKipas, Space.Self); 
        }

        if (fanAudio != null)
        {
            fanAudio.volume = windSlider.value * 0.25f;
            fanAudio.pitch = 1f + (windSlider.value * 1.5f);
        }

        if (particleAngin != null)
        {
            var anginEmission = particleAngin.emission;
            anginEmission.rateOverTime = windSlider.value * 30f; 
            
            if (windSlider.value > 0.05f && !particleAngin.isPlaying) particleAngin.Play();
            else if (windSlider.value <= 0.05f && particleAngin.isPlaying) particleAngin.Stop();
        }

        if (particleHaze != null)
        {
            var hazeEmission = particleHaze.emission;
            float targetHaze = isHeaterPowerOn ? tempSlider.value : 0f;
            hazeEmission.rateOverTime = targetHaze * 25f;
            
            if (targetHaze > 0.05f && !particleHaze.isPlaying) particleHaze.Play();
            else if (targetHaze <= 0.05f && particleHaze.isPlaying) particleHaze.Stop();
        }

        // 3. KAWAL LAMPU PEMANAS (3D HOT PLATE)
        if (heatLampLight != null)
        {
            heatLampLight.intensity = isHeaterPowerOn ? (tempSlider.value * maxLightIntensity) : 0f;
        }

        if (heatLampBulb != null)
        {
            heatLampBulb.GetPropertyBlock(bulbPropBlock);
            Color currentEmission = (isHeaterPowerOn && tempSlider.value > 0.01f) ? Color.Lerp(Color.black, heatEmissionColor, tempSlider.value) : Color.black;
            bulbPropBlock.SetColor("_EmissionColor", currentEmission);
            heatLampBulb.SetPropertyBlock(bulbPropBlock);
        }

        // Animate Knob (dial turns even if power is off, but doesn't heat)
        if (heaterKnob != null)
        {
            heaterKnob.localEulerAngles = new Vector3(0f, 0f, -tempSlider.value * 270f);
        }

        // Animate LED Indicator (black if off, green standby if powered, red if active heating)
        if (ledIndicator != null)
        {
            ledIndicator.GetPropertyBlock(bulbPropBlock);
            Color currentLedColor = Color.black;
            if (isHeaterPowerOn)
            {
                if (isEvaporationStarted && tempSlider.value > 0.05f)
                {
                    currentLedColor = ledOnColor; // Active red
                }
                else
                {
                    currentLedColor = new Color(0f, 1.5f, 0f); // Standby green
                }
            }
            bulbPropBlock.SetColor("_EmissionColor", currentLedColor);
            ledIndicator.SetPropertyBlock(bulbPropBlock);
        }

        // Animate Power Cables based on Slider values
        if (heaterCableAnimator != null)
        {
            float tempVal = isHeaterPowerOn ? tempSlider.value : 0f;
            if (tempVal > 0.01f)
            {
                heaterCableAnimator.SetActive(true);
                heaterCableAnimator.speed = 1.0f + tempVal * 4.0f;
            }
            else
            {
                heaterCableAnimator.SetActive(false);
            }
        }

        if (fanCableAnimator != null)
        {
            float windVal = windSlider.value;
            if (windVal > 0.01f)
            {
                fanCableAnimator.SetActive(true);
                fanCableAnimator.speed = 1.0f + windVal * 4.0f;
            }
            else
            {
                fanCableAnimator.SetActive(false);
            }
        }

        // Update all Thermometer target temperatures
        if (thermometers != null)
        {
            foreach (var t in thermometers)
            {
                if (t != null) t.targetTemperature = isHeaterPowerOn ? tempSlider.value : 0f;
            }
        }

        // Position steam particle system dynamically at the water surface level
        if (particleWap != null && activeWaterPivot != null)
        {
            float surfaceY = activeWaterPivot.position.y + activeWaterVolumeLocalY * 2f * activeWaterPivot.localScale.y;
            particleWap.transform.position = new Vector3(activeWaterPivot.position.x, surfaceY, activeWaterPivot.position.z);
        }

        // 4. PROSES PENYEJATAN & PARTICLE WAP (3D)
        if (isEvaporationStarted && activeWaterPivot != null)
        {
            if (waterVolumePercentage > 0)
            {
                elapsedTime += Time.deltaTime;
                
                // Adjust evaporation speed based on container capacity/volume
                float volumeCapacity = 1.0f;
                if (snappedArea == 0f) volumeCapacity = 0.25f; // cylinder evaporates percentage quicker for same rate, but rate is very small
                else if (snappedArea == 1.0f) volumeCapacity = 3.0f; // petri dish has more total volume capacity

                // Loss rate of volume is currentRate. Loss rate of percentage is currentRate / volumeCapacity.
                // Scaled slightly so it is at a good educational speed.
                waterVolumePercentage -= (currentRate / volumeCapacity) * 0.08f * Time.deltaTime;
                if (waterVolumePercentage < 0) waterVolumePercentage = 0;

                // Scale the active water pivot
                Vector3 newSize = activeWaterPivot.localScale;
                newSize.y = waterVolumePercentage;
                activeWaterPivot.localScale = newSize;

                if (graph3D != null)
                {
                    float currentVolume = waterVolumePercentage * 100f;
                    graph3D.AddDataPoint(elapsedTime, currentVolume);
                }

                if (particleWap != null)
                {
                    var wapEmission = particleWap.emission;
                    float targetWap = isHeaterPowerOn ? tempSlider.value : 0f;
                    wapEmission.rateOverTime = targetWap * 20f; 
                    
                    if (targetWap > 0.05f && !particleWap.isPlaying) particleWap.Play();
                    else if (targetWap <= 0.05f && particleWap.isPlaying) particleWap.Stop();
                }

                // Animate condensation opacity over time (petri dish has none)
                if (activeCondensation != null)
                {
                    float maxOpacity = 0.45f;
                    if (snappedArea == 1.0f) maxOpacity = 0f; // Petri dish
                    else if (snappedArea == 0f) maxOpacity = 0.65f; // Cylinder
                    
                    activeCondensation.opacity = Mathf.Min(maxOpacity, elapsedTime * 0.015f);
                }
            }
            else
            {
                isEvaporationStarted = false;
                IsExperimentFinished = true;
                if (evaporationRateText != null) evaporationRateText.text = "Eksperimen Selesai (Kering)";
                if (hologramText != null) hologramText.text = "Eksperimen Selesai\n(Kering)\n\n[Klik Buku Nota\nuntuk Rekod]";
                
                if (particleWap != null) particleWap.Stop();

                if (tutorialManager != null) tutorialManager.CompleteStep1();
            }
        }

        if (boilingAudio != null)
        {
            if (isEvaporationStarted && waterVolumePercentage > 0 && isHeaterPowerOn)
            {
                boilingAudio.volume = Mathf.MoveTowards(boilingAudio.volume, tempSlider.value * 0.3f, Time.deltaTime);
            }
            else
            {
                boilingAudio.volume = Mathf.MoveTowards(boilingAudio.volume, 0f, Time.deltaTime);
            }
        }
    }

    public void SetHeaterPower(bool isOn)
    {
        isHeaterPowerOn = isOn;
        if (!isOn)
        {
            isEvaporationStarted = false;
            if (particleWap != null) particleWap.Stop();
        }
    }

    public void StartExperiment()
    {
        if (!isHeaterPowerOn)
        {
            if (evaporationRateText != null) evaporationRateText.text = "Sila Hidupkan Suis Pemanas!";
            if (hologramText != null) hologramText.text = "Ralat: Suis Pemanas\nMATI!\n\n[Sila Hidupkan\nSuis Merah]";
            return;
        }

        if (waterVolumePercentage > 0)
        {
            isEvaporationStarted = true;
        }
    }

    public void ResetEksperimen()
    {
        isEvaporationStarted = false;
        IsExperimentFinished = false;
        isHeaterPowerOn = false;
        if (powerSwitch != null)
        {
            powerSwitch.ResetSwitch();
        }

        elapsedTime = 0f;
        waterVolumePercentage = 1.0f;
        if (graph3D != null) graph3D.ResetGraph();

        // Reset all water pivots to full Y scale
        if (cylinderGroup != null)
        {
            Transform wp = cylinderGroup.transform.Find("Water_Pivot");
            if (wp != null) wp.localScale = new Vector3(wp.localScale.x, 1.0f, wp.localScale.z);
        }
        if (beakerGroup != null)
        {
            Transform wp = beakerGroup.transform.Find("Water_Pivot");
            if (wp != null) wp.localScale = new Vector3(wp.localScale.x, 1.0f, wp.localScale.z);
        }
        if (petriGroup != null)
        {
            Transform wp = petriGroup.transform.Find("Water_Pivot");
            if (wp != null) wp.localScale = new Vector3(wp.localScale.x, 1.0f, wp.localScale.z);
        }

        if (particleWap != null) 
        {
            particleWap.Stop();
            particleWap.Clear(); 
        }

        // Reset condensation on all
        if (cylinderGroup != null)
        {
            var cond = cylinderGroup.GetComponentInChildren<BeakerCondensation3D>(true);
            if (cond != null) cond.opacity = 0f;
        }
        if (beakerGroup != null)
        {
            var cond = beakerGroup.GetComponentInChildren<BeakerCondensation3D>(true);
            if (cond != null) cond.opacity = 0f;
        }
        if (petriGroup != null)
        {
            var cond = petriGroup.GetComponentInChildren<BeakerCondensation3D>(true);
            if (cond != null) cond.opacity = 0f;
        }

        if (evaporationRateText != null)
        {
            evaporationRateText.text = "Kadar Penyejatan: " + currentRate.ToString("F3") + " unit/s";
        }
        
        if (hologramText != null)
        {
            hologramText.text = "Kadar Penyejatan:\n" + currentRate.ToString("F3") + " unit/s";
        }
    }
}
