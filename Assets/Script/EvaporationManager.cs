using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class EvaporationManager : MonoBehaviour
{
    [Header("Visual References")]
    public RectTransform waterVisual; 
    public RectTransform fanVisual;   
    
    [Header("Particle System References")]
    public ParticleSystem particleWap;   
    public ParticleSystem particleAngin; 

    // [BARU] Rujukan untuk lampu pemanas
    [Header("Visual Pemanas")]
    public Image visualLampuPemanas; 

    [Header("Control Sliders")]
    public Slider tempSlider;
    public Slider windSlider;
    public Slider areaSlider;
    public Slider humiditySlider;

    [Header("UI Text References")]
    public TextMeshProUGUI evaporationRateText;

    [Header("Engine Settings")]
    public float baseSpeed = 0.05f; 

    private bool isEvaporationStarted = false;
    private float currentRate = 0f;

    void Start()
    {
        if (particleWap != null) particleWap.Stop();
    }

    void Update()
    {
        // 1. KIRA KADAR PENYEJATAN
        float positiveFactors = tempSlider.value + windSlider.value + areaSlider.value;
        float negativeFactors = humiditySlider.value;
        currentRate = (positiveFactors - (negativeFactors * 0.8f)) * baseSpeed;

        if (currentRate < 0.01f) currentRate = 0.01f;

        if (evaporationRateText != null)
        {
            evaporationRateText.text = "Kadar Penyejatan: " + currentRate.ToString("F3") + " unit/s";
        }

        // 2. KAWAL KIPAS & PARTICLE ANGIN
        if (fanVisual != null)
        {
            float putaranKipas = windSlider.value * 500f * Time.deltaTime;
            fanVisual.Rotate(0, 0, -putaranKipas); 
        }

        if (particleAngin != null)
        {
            var anginEmission = particleAngin.emission;
            anginEmission.rateOverTime = windSlider.value * 30f; 
            
            if (windSlider.value > 0.05f && !particleAngin.isPlaying) particleAngin.Play();
            else if (windSlider.value <= 0.05f && particleAngin.isPlaying) particleAngin.Stop();
        }

        // [BARU] 3. KAWAL LAMPU PEMANAS
        if (visualLampuPemanas != null)
        {
            Color warnaLampu = visualLampuPemanas.color;
            // Jadikan nilai Alpha (lutsinar) sama dengan nilai slider suhu (0 hingga 1)
            warnaLampu.a = tempSlider.value; 
            visualLampuPemanas.color = warnaLampu;
        }

        // 4. PROSES PENYEJATAN & PARTICLE WAP
        if (isEvaporationStarted)
        {
            if (waterVisual.localScale.y > 0)
            {
                Vector3 newSize = waterVisual.localScale;
                newSize.y -= currentRate * Time.deltaTime;
                if (newSize.y < 0) newSize.y = 0;
                waterVisual.localScale = newSize;

                if (particleWap != null)
                {
                    var wapEmission = particleWap.emission;
                    wapEmission.rateOverTime = tempSlider.value * 20f; 
                    
                    if (!particleWap.isPlaying) particleWap.Play();
                }
            }
            else
            {
                isEvaporationStarted = false;
                evaporationRateText.text = "Eksperimen Selesai (Kering)";
                
                if (particleWap != null) particleWap.Stop();
            }
        }
    }

    public void StartExperiment()
    {
        if (waterVisual.localScale.y > 0)
        {
            isEvaporationStarted = true;
        }
    }

    public void ResetEksperimen()
    {
        isEvaporationStarted = false;

        Vector3 saizAsal = waterVisual.localScale;
        saizAsal.y = 1f; 
        waterVisual.localScale = saizAsal;

        if (particleWap != null) 
        {
            particleWap.Stop();
            particleWap.Clear(); 
        }

        if (evaporationRateText != null)
        {
            evaporationRateText.text = "Kadar Penyejatan: " + currentRate.ToString("F3") + " unit/s";
        }
    }
}