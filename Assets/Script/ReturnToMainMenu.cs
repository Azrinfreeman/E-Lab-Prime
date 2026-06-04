using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ReturnToMainMenu : MonoBehaviour
{
    [Header("Tetapan Navigasi")]
    [SerializeField]
    private string namaSceneMenuUtama = "MainMenu";

    [Header("Tetapan Skrin Loading")]
    [SerializeField]
    private float masaMinimumLoading = 2.0f;

    [SerializeField]
    private float masaFadeIn = 0.5f; // Masa yang diambil untuk panel muncul sepenuhnya

    [Header("Rujukan UI Loading Screen")]
    public GameObject loadingPanel;
    public Slider progressBar;
    public TextMeshProUGUI progressText;
    public CanvasGroup panelCanvasGroup; // Komponen baharu untuk kesan transparan (Fade)

    void Start()
    {
        if (loadingPanel != null)
        {
            loadingPanel.SetActive(false);
        }
    }

    public void Kembali()
    {
        StartCoroutine(LoadSceneAsync(namaSceneMenuUtama));
    }

    private IEnumerator LoadSceneAsync(string sceneName)
    {
        // 1. HIDUPKAN PANEL & MULAKAN ANIMASI FADE-IN
        if (loadingPanel != null)
            loadingPanel.SetActive(true);

        if (panelCanvasGroup != null)
        {
            panelCanvasGroup.alpha = 0f; // Mula dari halimunan
            float masaFading = 0f;

            // Loop untuk perlahan-lahan menaikkan Alpha dari 0 ke 1
            while (masaFading < masaFadeIn)
            {
                masaFading += Time.deltaTime;
                panelCanvasGroup.alpha = Mathf.Lerp(0f, 1f, masaFading / masaFadeIn);
                yield return null;
            }
            panelCanvasGroup.alpha = 1f; // Pastikan ia 100% gelap di akhir animasi
        }

        // 2. MULA MEMUAT TURUN SCENE
        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName);
        operation.allowSceneActivation = false;

        float fakeProgress = 0f;
        float masaBerlalu = 0f;

        // 3. ANIMASI BAR PERATUSAN (Seperti sebelum ini)
        while (masaBerlalu < masaMinimumLoading || operation.progress < 0.9f)
        {
            masaBerlalu += Time.deltaTime;
            fakeProgress = Mathf.MoveTowards(fakeProgress, 1f, Time.deltaTime / masaMinimumLoading);

            if (progressBar != null)
                progressBar.value = fakeProgress;
            if (progressText != null)
                progressText.text = Mathf.RoundToInt(fakeProgress * 100f) + "%";

            if (fakeProgress >= 1f && operation.progress >= 0.9f)
            {
                yield return new WaitForSeconds(0.2f);
                operation.allowSceneActivation = true; // Tukar scene
            }
            yield return null;
        }
    }
}
