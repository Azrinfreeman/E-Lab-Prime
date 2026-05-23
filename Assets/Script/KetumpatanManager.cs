using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class KetumpatanManager : MonoBehaviour
{
    [Header("Rujukan Objek")]
    public RectTransform batuVisual;
    public RectTransform kayuVisual;
    public RectTransform airVisual; 
    
    [Header("Rujukan Kesan Zarah")]
    public ParticleSystem particlePercikan; 

    [Header("Teks UI")]
    public TextMeshProUGUI teksStatus;

    [Header("Tetapan Jatuhan")]
    public float kelajuanJatuh = 200f;

    [Header("Rujukan Audio")]
    public AudioSource pembesarSuara;
    public AudioClip bunyiPercikan;

    private Vector2 posisiAsalBatu;
    private Vector2 posisiAsalKayu;
    private Vector3 saizAsalAir; 

    private float targetYBatu;
    private float targetYKayu;

    private bool batuSedangJatuh = false;
    private bool kayuSedangJatuh = false;
    
    private bool batuDahBunyi = false;
    private bool kayuDahBunyi = false;

    void Start()
    {
        if (batuVisual != null) posisiAsalBatu = batuVisual.anchoredPosition;
        if (kayuVisual != null) posisiAsalKayu = kayuVisual.anchoredPosition;
        if (airVisual != null) saizAsalAir = airVisual.localScale;

        // Tetapkan baki jarak jatuhan
        targetYBatu = posisiAsalBatu.y - 300f; 
        targetYKayu = posisiAsalKayu.y - 120f; 
    }

void Update()
    {
        // LOGIK JATUHKAN BATU
        if (batuSedangJatuh && batuVisual != null)
        {
            Vector2 posSemasa = batuVisual.anchoredPosition;
            if (posSemasa.y > targetYBatu)
            {
                posSemasa.y -= kelajuanJatuh * Time.deltaTime;
                batuVisual.anchoredPosition = posSemasa;

                // [LOGIK BAHARU]: Bunyi percikan jika batu melepasi permukaan air semasa (targetYKayu)
                if (!batuDahBunyi && posSemasa.y <= targetYKayu) 
                {
                    MainkanKesanSplat(batuVisual.anchoredPosition);
                    SesarkanAir(0.05f); 
                    batuDahBunyi = true;
                }
            }
            else
            {
                batuSedangJatuh = false;
                if (teksStatus != null) teksStatus.text = "Status: Batu tenggelam (Ketumpatan > 1.0 g/cm³)";
            }
        }

        // LOGIK JATUHKAN KAYU
        if (kayuSedangJatuh && kayuVisual != null)
        {
            Vector2 posSemasa = kayuVisual.anchoredPosition;
            if (posSemasa.y > targetYKayu)
            {
                posSemasa.y -= kelajuanJatuh * 0.8f * Time.deltaTime; 
                kayuVisual.anchoredPosition = posSemasa;

                // [LOGIK BAHARU]: Bunyi percikan sejurus sebelum kayu mendarat di permukaan (targetYKayu + 5f)
                if (!kayuDahBunyi && posSemasa.y <= targetYKayu + 5f)
                {
                    MainkanKesanSplat(kayuVisual.anchoredPosition);
                    kayuDahBunyi = true;
                }
            }
            else
            {
                // [FALLBACK KESELAMATAN]: Jika sistem terlepas pandang, paksa ia berbunyi waktu ia berhenti
                if (!kayuDahBunyi)
                {
                    MainkanKesanSplat(kayuVisual.anchoredPosition);
                    kayuDahBunyi = true;
                }
                
                kayuSedangJatuh = false;
                if (teksStatus != null) teksStatus.text = "Status: Kayu terapung (Ketumpatan < 1.0 g/cm³)";
            }
        }
    }

    // [DIKEMAS KINI] Fungsi sekarang menerima data posisi objek yang jatuh
    void MainkanKesanSplat(Vector2 posisiImpak)
    {
        if (pembesarSuara != null && bunyiPercikan != null)
        {
            pembesarSuara.PlayOneShot(bunyiPercikan);
        }
        
        if (particlePercikan != null)
        {
            // Ubah posisi bulatan zarah mengikut paksi X objek, tapi kekalkan ketinggian Y air
            Vector3 posisiZarahBaru = particlePercikan.transform.parent != null ? 
                particlePercikan.transform.parent.TransformPoint(new Vector3(posisiImpak.x, particlePercikan.rectTransform().anchoredPosition.y, 0)) : 
                new Vector3(posisiImpak.x, particlePercikan.transform.position.y, 0);
            
            // Cara paling mudah untuk UI Canvas: ubah anchoredPosition zarah secara langsung
            RectTransform particleRect = particlePercikan.GetComponent<RectTransform>();
            if(particleRect != null)
            {
                // Zarah akan meletup tepat di paksi X objek yang jatuh pada ketinggian permukaan air semasa
                particleRect.anchoredPosition = new Vector2(posisiImpak.x, particleRect.anchoredPosition.y);
            }

            particlePercikan.Play();
        }
    }

    void SesarkanAir(float peratus)
    {
        if (airVisual != null)
        {
            Vector3 saizSemasa = airVisual.localScale;
            saizSemasa.y += peratus; 
            airVisual.localScale = saizSemasa;

            float pikselNaik = 12f; 
            targetYKayu += pikselNaik;

            if (kayuVisual != null && !kayuSedangJatuh && kayuVisual.anchoredPosition.y < posisiAsalKayu.y - 50f)
            {
                Vector2 posKayu = kayuVisual.anchoredPosition;
                posKayu.y += pikselNaik;
                kayuVisual.anchoredPosition = posKayu;
            }
        }
    }

    public void JatuhkanBatu()
    {
        batuSedangJatuh = true;
    }

    public void JatuhkanKayu()
    {
        kayuSedangJatuh = true;
    }

    public void ResetEksperimen()
    {
        batuSedangJatuh = false;
        kayuSedangJatuh = false;
        batuDahBunyi = false;
        kayuDahBunyi = false;

        if (batuVisual != null) batuVisual.anchoredPosition = posisiAsalBatu;
        if (kayuVisual != null) kayuVisual.anchoredPosition = posisiAsalKayu;
        if (airVisual != null) airVisual.localScale = saizAsalAir;

        targetYKayu = posisiAsalKayu.y - 120f; 

        if (teksStatus != null) teksStatus.text = "Status: Sedia untuk diuji.";
    }
}

// Helah sambungan extension untuk membaca RectTransform dengan cepat
public static class RectTransformExtension {
    public static RectTransform rectTransform(this Component cp) {
        return cp.transform as RectTransform;
    }
}