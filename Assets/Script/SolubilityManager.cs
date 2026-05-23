using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SolubilityManager : MonoBehaviour
{
    [Header("Visual References")]
    public RectTransform rodVisual;     
    public RectTransform sugarVisual;   
    public Image heatingPlateLED;       
    
    // [BARU] Rujukan untuk Particle Buih
    public ParticleSystem particleBuih; 

    [Header("Sugar Sprite Settings")]
    public Sprite spriteGulaKetul; 
    public Sprite spriteGulaHalus; 
    private Image sugarImageComponent;

    [Header("Control Sliders")]
    public Slider tempSlider;   
    public Slider stirSlider;   
    public Slider sizeSlider;   

    [Header("UI Text References")]
    public TextMeshProUGUI solubilityRateText; 

    [Header("Engine Settings")]
    public float baseSpeed = 0.1f; 

    private bool isDissolving = false;
    private float currentRate = 0f;
    private Vector3 saizAsalGula;

    void Start()
    {
        if (sugarVisual != null)
        {
            saizAsalGula = sugarVisual.localScale;
            sugarImageComponent = sugarVisual.GetComponent<Image>();
        }

        // [BARU] Pastikan buih tidak keluar pada awal eksperimen
        if (particleBuih != null) particleBuih.Stop();
    }

    void Update()
    {
        // 1. KIRA KADAR KETERLARUTAN
        float faktorSuhu = tempSlider.value;
        float faktorKacauan = stirSlider.value;
        float faktorSaiz = sizeSlider.value; 

        currentRate = (faktorSuhu + faktorKacauan + (faktorSaiz * 0.5f) + 0.05f) * baseSpeed;

        if (solubilityRateText != null)
        {
            solubilityRateText.text = "Kadar Keterlarutan: " + currentRate.ToString("F3") + " unit/s";
        }

        // 2. TUKAR GAMBAR GULA SECARA LIVE
        if (!isDissolving && sugarImageComponent != null)
        {
            if (sizeSlider.value < 0.5f)
            {
                if (spriteGulaKetul != null) sugarImageComponent.sprite = spriteGulaKetul;
            }
            else
            {
                if (spriteGulaHalus != null) sugarImageComponent.sprite = spriteGulaHalus;
            }
        }

        // 3. ANIMASI ROD PENGACAU
        if (rodVisual != null)
        {
            if (stirSlider.value > 0)
            {
                float kelajuanGoyang = stirSlider.value * 15f;
                float sudutGoyang = Mathf.Sin(Time.time * kelajuanGoyang) * 8f; 
                rodVisual.localRotation = Quaternion.Euler(0, 0, sudutGoyang - 15f); 
            }
            else
            {
                rodVisual.localRotation = Quaternion.Euler(0, 0, -15f);
            }
        }

        // 4. ANIMASI LAMPU PEMANAS & BUIH MENDIDIH
        if (heatingPlateLED != null)
        {
            Color warnaLampu = heatingPlateLED.color;
            warnaLampu.a = tempSlider.value; 
            heatingPlateLED.color = warnaLampu;
        }

        // [BARU] Logik Buih Mendidih
        if (particleBuih != null)
        {
            var buihEmission = particleBuih.emission;
            
            // Jika slider suhu lebih dari 0.5 (50%), air mula mendidih
            if (tempSlider.value > 0.5f)
            {
                if (!particleBuih.isPlaying) particleBuih.Play();
                // Semakin tinggi suhu, semakin banyak buih (maksimum 40 buih sesaat)
                buihEmission.rateOverTime = (tempSlider.value - 0.5f) * 80f; 
            }
            else
            {
                if (particleBuih.isPlaying) particleBuih.Stop();
                buihEmission.rateOverTime = 0f;
            }
        }

        // 5. PROSES KETERLARUTAN
        if (isDissolving)
        {
            if (sugarVisual.localScale.x > 0)
            {
                Vector3 newSize = sugarVisual.localScale;
                newSize.x -= currentRate * Time.deltaTime;
                newSize.y -= currentRate * Time.deltaTime;
                newSize.z -= currentRate * Time.deltaTime;
                if (newSize.x < 0) newSize = Vector3.zero; 
                sugarVisual.localScale = newSize;

                if (sugarImageComponent != null)
                {
                    Color warnaGula = sugarImageComponent.color;
                    warnaGula.a = sugarVisual.localScale.x / saizAsalGula.x; 
                    sugarImageComponent.color = warnaGula;
                }
            }
            else
            {
                isDissolving = false;
                solubilityRateText.text = "Eksperimen Selesai (Larut Sepenuhnya)";
            }
        }
    }

    public void MulaPelarutan()
    {
        if (sugarVisual.localScale.x > 0)
        {
            isDissolving = true;
        }
    }

    public void ResetEksperimen()
    {
        isDissolving = false;
        
        if (sugarVisual != null)
        {
            sugarVisual.localScale = saizAsalGula;
            if (sugarImageComponent != null)
            {
                Color warnaGula = sugarImageComponent.color;
                warnaGula.a = 1f; 
                sugarImageComponent.color = warnaGula;
                
                // Set gambar kembali mengikut nilai slider semasa reset
                if (sizeSlider.value < 0.5f && spriteGulaKetul != null) sugarImageComponent.sprite = spriteGulaKetul;
                else if (sizeSlider.value >= 0.5f && spriteGulaHalus != null) sugarImageComponent.sprite = spriteGulaHalus;
            }
        }

        if (solubilityRateText != null)
        {
            solubilityRateText.text = "Kadar Keterlarutan: " + currentRate.ToString("F3") + " unit/s";
        }
    }
}