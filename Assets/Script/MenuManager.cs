using System.Collections;
using UnityEngine;
using UnityEngine.UI; // PENTING: Wajib ada untuk mengawal komponen UI Image
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    [Header("Rujukan Panel (Pop-up)")]
    public GameObject panelPengaturan; 
    public GameObject panelPencapaian; 
    public GameObject panelAmali; 

    [Header("Rujukan Butang Navigasi Sisi (Untuk Efek Menyala)")]
    public Image butangNavAmali;
    public Image butangNavPengaturan;
    public Image butangNavPencapaian;

    [Header("Tetapan Warna Lampu Butang")]
    public Color warnaBiasa = new Color(0.10f, 0.16f, 0.28f, 0.85f); // Warna asal butang (frosted dark)
    public Color warnaMenyala = new Color(0.18f, 0.77f, 0.71f, 1f); // Warna cyan menyala

    [Header("Tetapan Animasi")]
    public float masaDelayButang = 0.15f; 

    void Start()
    {
        // Tutup semua panel pada permulaan
        if (panelPengaturan != null) panelPengaturan.SetActive(false);
        if (panelPencapaian != null) panelPencapaian.SetActive(false);
        if (panelAmali != null) panelAmali.SetActive(false);

        // Pastikan semua butang dalam keadaan warna biasa
        ResetLampuNavigasi();
    }

    // ==========================================
    // SISTEM LAMPU NAVIGASI
    // ==========================================
    
    private void ResetLampuNavigasi()
    {
        // Kembalikan semua butang kepada warna asal
        if (butangNavAmali != null) butangNavAmali.color = warnaBiasa;
        if (butangNavPengaturan != null) butangNavPengaturan.color = warnaBiasa;
        if (butangNavPencapaian != null) butangNavPencapaian.color = warnaBiasa;
    }

    // --- FUNGSI PANEL AMALI ---
    
    public void BukaAmali()
    {
        ResetLampuNavigasi(); // Padam lampu lain
        if (butangNavAmali != null) butangNavAmali.color = warnaMenyala; // Nyalakan butang Amali
        
        StartCoroutine(BukaPanelDenganDelay(panelAmali, masaDelayButang));
    }

    public void TutupAmali()
    {
        if (butangNavAmali != null) butangNavAmali.color = warnaBiasa; // Padam lampu bila tutup
        if (panelAmali != null) panelAmali.SetActive(false);
    }

    // --- FUNGSI PANEL PENGATURAN ---

    public void BukaPengaturan()
    {
        ResetLampuNavigasi();
        if (butangNavPengaturan != null) butangNavPengaturan.color = warnaMenyala;

        StartCoroutine(BukaPanelDenganDelay(panelPengaturan, masaDelayButang));
    }

    public void TutupPengaturan()
    {
        if (butangNavPengaturan != null) butangNavPengaturan.color = warnaBiasa;
        if (panelPengaturan != null) panelPengaturan.SetActive(false);
    }

    // --- FUNGSI PANEL PENCAPAIAN ---

    public void BukaPencapaian()
    {
        ResetLampuNavigasi();
        if (butangNavPencapaian != null) butangNavPencapaian.color = warnaMenyala;

        StartCoroutine(BukaPanelDenganDelay(panelPencapaian, masaDelayButang));
    }

    public void TutupPencapaian()
    {
        if (butangNavPencapaian != null) butangNavPencapaian.color = warnaBiasa;
        if (panelPencapaian != null) panelPencapaian.SetActive(false);
    }

    // --- FUNGSI NAVIGASI SCENE ---
    
    public void LoadAmali(string namaScene)
    {
        StartCoroutine(MuatSceneDenganDelay(namaScene, masaDelayButang));
    }

    // --- FUNGSI KELUAR ---

    public void KeluarAplikasi()
    {
        StartCoroutine(KeluarDenganDelay(masaDelayButang));
    }

    // ==========================================
    // ENJIN COROUTINE 
    // ==========================================

    private IEnumerator BukaPanelDenganDelay(GameObject panel, float masa)
    {
        yield return new WaitForSeconds(masa);
        
        // Tutup panel lain supaya tidak bertindih jika pemain tekan laju-laju
        if (panelPengaturan != null && panel != panelPengaturan) panelPengaturan.SetActive(false);
        if (panelPencapaian != null && panel != panelPencapaian) panelPencapaian.SetActive(false);
        if (panelAmali != null && panel != panelAmali) panelAmali.SetActive(false);

        if (panel != null) panel.SetActive(true);
    }

    private IEnumerator MuatSceneDenganDelay(string namaScene, float masa)
    {
        yield return new WaitForSeconds(masa);
        SceneManager.LoadScene(namaScene);
    }

    private IEnumerator KeluarDenganDelay(float masa)
    {
        yield return new WaitForSeconds(masa);
        Application.Quit();
        Debug.Log("Keluar dari E-Lab.");
    }
}