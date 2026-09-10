using System.Collections;
using UnityEngine;
using UnityEngine.UI; // PENTING: Wajib ada untuk mengawal komponen UI Image
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    [Header("Rujukan Panel (Pop-up)")]
    public GameObject panelUtama; // PanelLogin or Main Menu container
    public GameObject panelPengaturan; 
    public GameObject panelPencapaian; 
    public GameObject panelAmali; 
    public GameObject panelAkaun;

    [Header("Rujukan Butang Navigasi Sisi (Untuk Efek Menyala)")]
    public Image butangNavAmali;
    public Image butangNavPengaturan;
    public Image butangNavPencapaian;
    public Image butangNavAkaun;
    public Image butangNavUtama;

    [Header("Tetapan Warna Lampu Butang")]
    public Color warnaBiasa = new Color(0.10f, 0.16f, 0.28f, 0.85f); // Warna asal butang (frosted dark)
    public Color warnaMenyala = new Color(0.18f, 0.77f, 0.71f, 1f); // Warna cyan menyala

    [Header("Tetapan Animasi")]
    public float masaDelayButang = 0.15f;

    private bool waitingForOpeningSequence; 

void Start()
    {
        PrepareMenuForOpeningSequence();
        StartCoroutine(ShowHomeAfterOpeningSequence());
    }

private void PrepareMenuForOpeningSequence()
    {
        ResetLampuNavigasi();

        if (panelUtama != null) panelUtama.SetActive(false);
        if (panelPengaturan != null) panelPengaturan.SetActive(false);
        if (panelPencapaian != null) panelPencapaian.SetActive(false);
        if (panelAmali != null) panelAmali.SetActive(false);
        if (panelAkaun != null) panelAkaun.SetActive(false);
    }

private IEnumerator ShowHomeAfterOpeningSequence()
    {
        waitingForOpeningSequence = FindAnyObjectByType<OpeningSequenceController>() != null;
        if (!waitingForOpeningSequence)
        {
            BukaUtama();
            yield break;
        }

        OpeningSequenceController.SequenceFinished -= HandleOpeningSequenceFinished;
        OpeningSequenceController.SequenceFinished += HandleOpeningSequenceFinished;

        while (FindAnyObjectByType<OpeningSequenceController>() != null)
        {
            yield return null;
        }

        ShowHomeFromOpeningSequence();
    }

private void HandleOpeningSequenceFinished()
    {
        ShowHomeFromOpeningSequence();
    }

    private void ShowHomeFromOpeningSequence()
    {
        if (!waitingForOpeningSequence)
        {
            return;
        }

        waitingForOpeningSequence = false;
        OpeningSequenceController.SequenceFinished -= HandleOpeningSequenceFinished;
        BukaUtama();
    }

    private void OnDestroy()
    {
        OpeningSequenceController.SequenceFinished -= HandleOpeningSequenceFinished;
    }



    // ==========================================
    // SISTEM LAMPU NAVIGASI
    // ==========================================
    
private void SetNavigationColour(Image image, Color colour)
    {
        image.color = colour;
        if (gameObject.scene.name == "MainMenu")
        {
            var outline = image.GetComponent<Outline>();
            if (outline != null) outline.enabled = colour == warnaMenyala;
        }
    }

private void ResetLampuNavigasi()
    {
        // Kembalikan semua butang kepada warna asal
        if (butangNavAmali != null) SetNavigationColour(butangNavAmali, warnaBiasa);
        if (butangNavPengaturan != null) SetNavigationColour(butangNavPengaturan, warnaBiasa);
        if (butangNavPencapaian != null) SetNavigationColour(butangNavPencapaian, warnaBiasa);
        if (butangNavAkaun != null) SetNavigationColour(butangNavAkaun, warnaBiasa);
        if (butangNavUtama != null) SetNavigationColour(butangNavUtama, warnaBiasa);
    }

    // --- FUNGSI PANEL AMALI ---
    
    public void BukaAmali()
    {
        ResetLampuNavigasi(); // Padam lampu lain
        if (butangNavAmali != null) SetNavigationColour(butangNavAmali, warnaMenyala); // Nyalakan butang Amali
        
        StartCoroutine(BukaPanelDenganDelay(panelAmali, masaDelayButang));
    }

    public void TutupAmali()
    {
        if (butangNavAmali != null) SetNavigationColour(butangNavAmali, warnaBiasa); // Padam lampu bila tutup
        if (panelAmali != null) panelAmali.SetActive(false);
        if (panelUtama != null) panelUtama.SetActive(true); // Tunjuk balik menu utama
    }

    // --- FUNGSI PANEL PENGATURAN ---

    public void BukaPengaturan()
    {
        ResetLampuNavigasi();
        if (butangNavPengaturan != null) SetNavigationColour(butangNavPengaturan, warnaMenyala);

        StartCoroutine(BukaPanelDenganDelay(panelPengaturan, masaDelayButang));
    }

    public void TutupPengaturan()
    {
        if (butangNavPengaturan != null) SetNavigationColour(butangNavPengaturan, warnaBiasa);
        if (panelPengaturan != null) panelPengaturan.SetActive(false);
        if (panelUtama != null) panelUtama.SetActive(true); // Tunjuk balik menu utama
    }

    // --- FUNGSI PANEL PENCAPAIAN ---

    public void BukaPencapaian()
    {
        ResetLampuNavigasi();
        if (butangNavPencapaian != null) SetNavigationColour(butangNavPencapaian, warnaMenyala);

        StartCoroutine(BukaPanelDenganDelay(panelPencapaian, masaDelayButang));
    }

    public void TutupPencapaian()
    {
        if (butangNavPencapaian != null) SetNavigationColour(butangNavPencapaian, warnaBiasa);
        if (panelPencapaian != null) panelPencapaian.SetActive(false);
        if (panelUtama != null) panelUtama.SetActive(true); // Tunjuk balik menu utama
    }

    // --- FUNGSI PANEL AKAUN ---

    public void BukaAkaun()
    {
        ResetLampuNavigasi();
        if (butangNavAkaun != null) SetNavigationColour(butangNavAkaun, warnaMenyala);

        StartCoroutine(BukaPanelDenganDelay(panelAkaun, masaDelayButang));
    }

    public void TutupAkaun()
    {
        if (butangNavAkaun != null) SetNavigationColour(butangNavAkaun, warnaBiasa);
        if (panelAkaun != null) panelAkaun.SetActive(false);
        if (panelUtama != null) panelUtama.SetActive(true);
    }

public void BukaUtama()
    {
        ResetLampuNavigasi();
        if (butangNavUtama != null) SetNavigationColour(butangNavUtama, warnaMenyala);

        if (panelPengaturan != null) panelPengaturan.SetActive(false);
        if (panelPencapaian != null) panelPencapaian.SetActive(false);
        if (panelAmali != null) panelAmali.SetActive(false);
        if (panelAkaun != null) panelAkaun.SetActive(false);
        if (panelUtama != null) panelUtama.SetActive(true);
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
        if (panelAkaun != null && panel != panelAkaun) panelAkaun.SetActive(false);

        // Sembunyikan panel utama jika membuka panel lain
        if (panelUtama != null && panel != null) panelUtama.SetActive(false);

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
