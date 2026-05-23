using UnityEngine;

public class ElectrolysisManager : MonoBehaviour
{   [Header("Quiz Completion")]
public GameObject successPanel; // Tarik SuccessPanel_Amali2 ke sini
    private int correctLabelsCount = 0;
    [Header("Test Tube Water Visuals")]
    public RectTransform waterAnode;   // Letak imej air tabung uji kiri (Oksigen)
    public RectTransform waterCathode; // Letak imej air tabung uji kanan (Hidrogen)

    [Header("Particle Systems")]
    public GameObject bubblesAnode;
    public GameObject bubblesCathode;

    [Header("Simulation Settings")]
    public float oxygenRate = 0.05f; 
    public float hydrogenRate = 0.10f; // Nisbah 2:1 (2x lebih laju dari Oksigen)
    public float minWaterLevel = 0.2f; // Had paling rendah air boleh turun

    private bool isRunning = false;
    private Vector3 originalAnodeScale;
    private Vector3 originalCathodeScale;

    void Start()
    {
        // Simpan saiz asal untuk tujuan Reset
        if (waterAnode != null) originalAnodeScale = waterAnode.localScale;
        if (waterCathode != null) originalCathodeScale = waterCathode.localScale;

        if (bubblesAnode != null) bubblesAnode.SetActive(false);
        if (bubblesCathode != null) bubblesCathode.SetActive(false);
    }

    void Update()
    {
        if (isRunning)
        {
            RunElectrolysis();
        }
    }

    // Sambungkan fungsi ini pada butang "TURN ON SWITCH"
    public void StartElectrolysis()
    {
        if (!isRunning)
        {
            isRunning = true;
            if (bubblesAnode != null) bubblesAnode.SetActive(true);
            if (bubblesCathode != null) bubblesCathode.SetActive(true);
        }
    }

    private void RunElectrolysis()
    {
        // Turunkan paras air Anod (Oksigen)
        // Kita gandakan nilai minWaterLevel untuk Oksigen supaya ia berhenti pada paras yang lebih tinggi
        if (waterAnode != null && waterAnode.localScale.y > (minWaterLevel * 2f)) 
        {
            Vector3 scaleA = waterAnode.localScale;
            scaleA.y -= oxygenRate * Time.deltaTime;
            waterAnode.localScale = scaleA;
        }

        // Turunkan paras air Katod (Hidrogen)
        if (waterCathode != null && waterCathode.localScale.y > minWaterLevel)
        {
            Vector3 scaleC = waterCathode.localScale;
            scaleC.y -= hydrogenRate * Time.deltaTime;
            waterCathode.localScale = scaleC;
        }
        else
        {
            // Hentikan simulasi apabila gas Hidrogen penuh (air di tahap paling minimum)
            isRunning = false;
            if (bubblesAnode != null) bubblesAnode.SetActive(false);
            if (bubblesCathode != null) bubblesCathode.SetActive(false);
        }
    }

    // Fungsi tambahan jika anda letak butang Reset
    public void ResetExperiment()
    {
        isRunning = false;
        
        if (waterAnode != null) waterAnode.localScale = originalAnodeScale;
        if (waterCathode != null) waterCathode.localScale = originalCathodeScale;

        if (bubblesAnode != null) bubblesAnode.SetActive(false);
        if (bubblesCathode != null) bubblesCathode.SetActive(false);
    }
    public void LabelPlacedCorrectly()
    {
        correctLabelsCount++;
        
        if (correctLabelsCount >= 2)
        {
            // Panggil pop-up dengan sedikit lengah (delay) supaya label sempat snap
            Invoke("ShowSuccessPopup", 1.2f);
        }
    }
    private void ShowSuccessPopup()
    {
        if (successPanel != null)
        {
            successPanel.SetActive(true);
            
            // Matikan gelembung gas (jika masih ada)
            if (bubblesAnode != null) bubblesAnode.SetActive(false);
            if (bubblesCathode != null) bubblesCathode.SetActive(false);
        }
    }

    // Fungsi untuk Butang "Teruskan" (Boleh digunakan untuk tukar Scene)
    public void GoToNextExperiment()
    {
        Debug.Log("Loading Next Experiment...");
        // Di sini anda boleh masukkan kod SceneManager.LoadScene() nanti
    }
}