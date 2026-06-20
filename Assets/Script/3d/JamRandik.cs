using TMPro;
using UnityEngine;

public class JamRandik : MonoBehaviour
{
    [Header("Paparan UI")]
    public TextMeshProUGUI teksMasa;

    [Tooltip("Masukkan teks 'COMPLETE SWINGS: 0/10' di sini")]
    public TextMeshProUGUI teksKiraanAyunan;

    [Header("Rujukan Objek (Untuk Pengiraan)")]
    public Transform pemberatBandul;
    public Transform pivotTiang;

    [Header("Grafik Jarum Jam (Pilihan)")]
    [Tooltip("Masukkan imej UI Jarum Saat yang panjang")]
    public RectTransform jarumSaat;

    [Tooltip("Masukkan imej UI Jarum Minit yang kecil (biarkan kosong jika tiada)")]
    public RectTransform jarumMinit;

    // Pembolehubah Masa
    private float masaSemasa = 0f;
    private bool sedangBerjalan = false;

    // Pembolehubah Pengiraan Ayunan Automatik
    private int jumlahAyunan = 0;
    private bool diSebelahKanan = false;
    private int lintasanTengah = 0;

    void Start()
    {
        ResetMasa();
    }

    void Update()
    {
        if (sedangBerjalan)
        {
            // 1. Jalankan masa
            masaSemasa += Time.deltaTime;
            KemaskiniPaparanMasa();

            // 2. Jalankan pengesan ayunan automatik
            KiraAyunanAutomatik();
        }
    }

    private void KiraAyunanAutomatik()
    {
        if (pemberatBandul == null || pivotTiang == null)
            return;

        float bezaX = pemberatBandul.position.x - pivotTiang.position.x;
        bool sekarangDiKanan = bezaX > 0;

        if (sekarangDiKanan != diSebelahKanan)
        {
            lintasanTengah++;
            diSebelahKanan = sekarangDiKanan;

            if (lintasanTengah >= 2)
            {
                jumlahAyunan++;
                lintasanTengah = 0;
                KemaskiniPaparanAyunan();

                if (jumlahAyunan >= 10)
                {
                    HentiMasa();
                }
            }
        }
    }

    private void KemaskiniPaparanMasa()
    {
        int minit = Mathf.FloorToInt(masaSemasa / 60f);
        int saat = Mathf.FloorToInt(masaSemasa % 60f);
        int milisaat = Mathf.FloorToInt((masaSemasa * 100f) % 100f);

        if (teksMasa != null)
            teksMasa.text = string.Format("{0:00}:{1:00}.{2:00}", minit, saat, milisaat);

        // ==========================================
        // ANIMASI JARUM JAM RANDIK
        // ==========================================

        // 1 putaran penuh (360 darjah) = 60 saat. Jadi, 1 saat = 6 darjah.
        if (jarumSaat != null)
        {
            float sudutSaat = (masaSemasa % 60f) * 6f;
            // Guna -sudutSaat kerana pusingan jam adalah ke arah negatif Z
            jarumSaat.localRotation = Quaternion.Euler(0, 0, -sudutSaat);
        }

        // Jika jam randik anda ada dial minit kecil (biasanya 1 putaran = 60 minit)
        if (jarumMinit != null)
        {
            float sudutMinit = (masaSemasa / 60f) * 6f;
            jarumMinit.localRotation = Quaternion.Euler(0, 0, -sudutMinit);
        }
    }

    private void KemaskiniPaparanAyunan()
    {
        if (teksKiraanAyunan != null)
        {
            teksKiraanAyunan.text = "COMPLETE SWINGS: " + jumlahAyunan + "/10";
        }
    }

    // ==========================================
    // FUNGSI BUTANG UI
    // ==========================================

    public void MulaMasa()
    {
        // --- KOD BAHARU: Reset masa dan ayunan ke 0 setiap kali mula ---
        masaSemasa = 0f;
        jumlahAyunan = 0;
        lintasanTengah = 0;
        KemaskiniPaparanMasa();
        KemaskiniPaparanAyunan();
        // ----------------------------------------------------------------

        sedangBerjalan = true;

        // Baca kedudukan awal bola ketika butang MULA ditekan supaya kiraan sangat tepat
        if (pemberatBandul != null && pivotTiang != null)
        {
            diSebelahKanan = (pemberatBandul.position.x - pivotTiang.position.x) > 0;
        }
    }

    public void HentiMasa()
    {
        sedangBerjalan = false;
    }

    public void ResetMasa()
    {
        sedangBerjalan = false;
        masaSemasa = 0f;
        jumlahAyunan = 0;
        lintasanTengah = 0;

        KemaskiniPaparanMasa();
        KemaskiniPaparanAyunan();
    } // Tambah fungsi ini di bawah sekali dalam JamRandik.cs

    public float DapatkanMasaSemasa()
    {
        return masaSemasa;
    }

    public int DapatkanJumlahAyunan()
    {
        return jumlahAyunan;
    }
}
