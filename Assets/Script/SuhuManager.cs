using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SuhuManager : MonoBehaviour
{
    [Header("Rujukan Visual")]
    public RectTransform aisVisual;      // TARIK: Ais_Visual (Kotak putih/gula)
    public RectTransform airVisual;      // TARIK: Air_Visual (Bikar cecair)
    public Image lampuPemanas;           // TARIK: Lampu merah pemanas

    [Header("Rujukan Particle (Zarah)")]
    public ParticleSystem particleBuih;  // TARIK: Zarah buih
    public ParticleSystem particleWap;   // TARIK: Zarah wap/asap

    [Header("Kawalan & Teks UI")]
    public Slider tempSlider;            // TARIK: Slider Suhu (-20 hingga 120)
    public TextMeshProUGUI teksSuhu;     // TARIK: Teks paparan suhu semasa
    public TextMeshProUGUI teksStatus;   // TARIK: Teks paparan status keadaan

    private bool isRunning = false;
    private Vector3 saizAsalAis;
    private Vector3 saizAsalAir;

    void Start()
    {
        // Simpan saiz asal untuk butang Reset
        if (aisVisual != null) saizAsalAis = aisVisual.localScale;
        if (airVisual != null) saizAsalAir = airVisual.localScale;

        HentikanZarah();
    }

    void Update()
    {
        float suhu = tempSlider.value;

        // 1. KEMAS KINI TEKS PAPARAN
        if (teksSuhu != null) teksSuhu.text = "Suhu Semasa: " + suhu.ToString("F0") + "°C";

        // 2. TENTUKAN STATUS JIRIM & ZARAH
        string status = "";
        
        if (suhu < 0)
        {
            status = "Pepejal (Ais Beku)";
            HentikanZarah();
        }
        else if (suhu >= 0 && suhu < 100)
        {
            // Jika ais masih ada (belum cair sepenuhnya)
            if (aisVisual != null && aisVisual.localScale.x > 0)
                status = "Ais Sedang Melebur...";
            else
                status = "Cecair (Air)";
            
            HentikanZarah();
        }
        else if (suhu >= 100)
        {
            status = "Air Mendidih & Meruap!";
            // Hanya keluar wap/buih jika masih ada air
            if (airVisual != null && airVisual.localScale.y > 0)
            {
                AktifkanZarahMendidih(suhu);
            }
            else
            {
                HentikanZarah();
                status = "Bikar Kering (Wap Sepenuhnya)";
            }
        }

        if (teksStatus != null) teksStatus.text = "Status: " + status;

        // 3. LOGIK CAHAYA PEMANAS
        if (lampuPemanas != null)
        {
            Color warna = lampuPemanas.color;
            // Lampu makin terang bila suhu makin tinggi dari 0
            warna.a = Mathf.Clamp01(suhu / 120f); 
            lampuPemanas.color = warna;
        }

        // 4. PROSES PERUBAHAN FIZIKAL (Hanya berlaku lepas butang Mula ditekan)
        if (isRunning)
        {
            // A. Proses Melebur (Suhu > 0)
            if (suhu > 0 && aisVisual != null && aisVisual.localScale.x > 0)
            {
                float kadarLebur = suhu * 0.005f * Time.deltaTime;
                Vector3 saizBaruAis = aisVisual.localScale;
                saizBaruAis.x -= kadarLebur;
                saizBaruAis.y -= kadarLebur;
                
                if (saizBaruAis.x < 0) saizBaruAis = Vector3.zero;
                aisVisual.localScale = saizBaruAis;
            }
            // B. Proses Mendidih (Suhu >= 100)
            else if (suhu >= 100 && airVisual != null && airVisual.localScale.y > 0)
            {
                // Pastikan ais dah cair habis baru air boleh meruap
                if (aisVisual == null || aisVisual.localScale.x <= 0)
                {
                    float kadarDidih = (suhu - 90f) * 0.002f * Time.deltaTime;
                    Vector3 saizBaruAir = airVisual.localScale;
                    saizBaruAir.y -= kadarDidih;
                    
                    if (saizBaruAir.y < 0) saizBaruAir = Vector3.zero;
                    airVisual.localScale = saizBaruAir;
                }
            }
        }
    }

    void HentikanZarah()
    {
        if (particleBuih != null && particleBuih.isPlaying) particleBuih.Stop();
        if (particleWap != null && particleWap.isPlaying) particleWap.Stop();
    }

    void AktifkanZarahMendidih(float suhu)
    {
        if (particleBuih != null)
        {
            if (!particleBuih.isPlaying) particleBuih.Play();
            var buihEmission = particleBuih.emission;
            buihEmission.rateOverTime = (suhu - 95f) * 2f; 
        }
        if (particleWap != null)
        {
            if (!particleWap.isPlaying) particleWap.Play();
            var wapEmission = particleWap.emission;
            wapEmission.rateOverTime = (suhu - 95f) * 1.5f;
        }
    }

    // Sambungkan ke butang MULA
    public void MulaEksperimen()
    {
        isRunning = true;
    }

    // Sambungkan ke butang MULA SEMULA
    public void ResetEksperimen()
    {
        isRunning = false;
        
        if (aisVisual != null) aisVisual.localScale = saizAsalAis;
        if (airVisual != null) airVisual.localScale = saizAsalAir;
        
        HentikanZarah();
        
        if (tempSlider != null) tempSlider.value = -20f; 
    }
}