using UnityEngine;
using UnityEngine.UI;

public class InteractiveNotebookLab4 : MonoBehaviour
{
    [Header("References")]
    public EvaporationManager3D evaporationManager;
    public Lab4TutorialManager tutorialManager;
    public QuizManagerLab4 quizManager;
    
    [Header("UI Overlay Panel")]
    public GameObject uiPanel;
    public Button submitButton;
    public Button closeButton;
    public Text feedbackText;

    [Header("3D Pages Texts")]
    public TextMesh leftPageTextMesh;
    public TextMesh rightPageTextMesh;

    private AudioSource audioSource;
    private AudioClip submitSuccessSFX;
    private AudioClip errorSFX;
    private bool isLoggingCompleted = false;

    void Start()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0.5f;
        audioSource.volume = 0.6f;

        submitSuccessSFX = CreatePencilScratchSound();
        errorSFX = CreateErrorBuzzerSound();

        Invoke("PlayStartScribble", 0.5f);

        if (feedbackText != null) feedbackText.text = "";

        if (uiPanel != null) uiPanel.SetActive(false);

        if (submitButton != null)
        {
            submitButton.onClick.RemoveAllListeners();
            submitButton.onClick.AddListener(SubmitRecords);
        }

        if (closeButton != null)
        {
            closeButton.onClick.RemoveAllListeners();
            closeButton.onClick.AddListener(ClosePanel);
        }

        if (leftPageTextMesh != null)
        {
            leftPageTextMesh.text = "OBJEKTIF\n\nMengkaji faktor yang\nmempengaruhi kadar\npenyejatan air.";
        }
        if (rightPageTextMesh != null)
        {
            rightPageTextMesh.text = "PEMERHATIAN\n\nSila jalankan eksperimen\ndan perhatikan perubahan.\n\nKlik di sini untuk\nmerekod pemerhatian\ndan menjawab kuiz.";
        }
    }

    void OnMouseDown()
    {
        if (uiPanel != null && uiPanel.activeSelf) return;

        CameraDragArea dragArea = Object.FindAnyObjectByType<CameraDragArea>();
        if (dragArea != null)
        {
            dragArea.SetTextbookView();
        }

        if (uiPanel != null)
        {
            uiPanel.SetActive(true);
            PlaySound(submitSuccessSFX);

            if (submitButton != null)
            {
                submitButton.gameObject.SetActive(!isLoggingCompleted);
            }

            if (closeButton != null)
            {
                closeButton.gameObject.SetActive(true);
                RectTransform closeRect = closeButton.GetComponent<RectTransform>();
                if (closeRect != null)
                {
                    closeRect.anchoredPosition = isLoggingCompleted ? new Vector2(0f, 0f) : new Vector2(110f, 0f);
                }
            }

            if (feedbackText != null)
            {
                feedbackText.text = isLoggingCompleted ? "" : "Adakah anda telah memerhatikan kesan suhu, luas permukaan, kelajuan angin dan kelembapan?";
                feedbackText.color = KidFriendlySceneTheme.Feedback(this, new Color(0.2f, 0.2f, 0.2f));
            }
        }
    }

    public void ClosePanel()
    {
        if (uiPanel != null) uiPanel.SetActive(false);
        
        CameraDragArea dragArea = Object.FindAnyObjectByType<CameraDragArea>();
        if (dragArea != null)
        {
            dragArea.SetFrontView();
        }
    }

    public void SubmitRecords()
    {
        isLoggingCompleted = true;
        PlaySound(submitSuccessSFX);
        
        if (rightPageTextMesh != null)
        {
            rightPageTextMesh.text = "PEMERHATIAN DIREKOD";
        }

        if (feedbackText != null)
        {
            feedbackText.text = "Rekod Berjaya! Menyalakan kuiz...";
            feedbackText.color = KidFriendlySceneTheme.Feedback(this, new Color(0.1f, 0.6f, 0.2f));
        }
        
        if (tutorialManager != null) tutorialManager.CompleteStep2();

        Invoke("CompleteAndTriggerQuiz", 2.0f);
    }

    private void CompleteAndTriggerQuiz()
    {
        if (uiPanel != null) uiPanel.SetActive(false);

        CameraDragArea dragArea = Object.FindAnyObjectByType<CameraDragArea>();
        if (dragArea != null)
        {
            dragArea.SetFrontView();
        }

        if (quizManager != null)
        {
            quizManager.ShowQuiz();
        }
    }

    private void PlaySound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }

    private void PlayStartScribble()
    {
        PlaySound(submitSuccessSFX);
    }

    private AudioClip CreatePencilScratchSound()
    {
        int samplerate = 44100;
        float duration = 0.8f;
        int numSamples = (int)(samplerate * duration);
        float[] samples = new float[numSamples];
        for (int i = 0; i < numSamples; i++)
        {
            float t = (float)i / samplerate;
            float noise1 = (Random.value * 2f - 1f) * 0.12f * Mathf.Sin(2f * Mathf.PI * 400f * t);
            float noise2 = (Random.value * 2f - 1f) * 0.08f * Mathf.Sin(2f * Mathf.PI * 800f * t);
            float envelope = Mathf.Max(0, Mathf.Sin(2f * Mathf.PI * 4f * t));
            samples[i] = (noise1 + noise2) * envelope * (1.0f - t);
        }
        AudioClip clip = AudioClip.Create("PencilScratchSFX", numSamples, 1, samplerate, false);
        clip.SetData(samples, 0);
        return clip;
    }

    private AudioClip CreateErrorBuzzerSound()
    {
        int samplerate = 44100;
        float duration = 0.3f;
        int numSamples = (int)(samplerate * duration);
        float[] samples = new float[numSamples];
        for (int i = 0; i < numSamples; i++)
        {
            float t = (float)i / samplerate;
            float buzz = Mathf.Sign(Mathf.Sin(2f * Mathf.PI * 150f * t));
            samples[i] = buzz * 0.15f * (1.0f - t);
        }
        AudioClip clip = AudioClip.Create("ErrorBuzzSFX", numSamples, 1, samplerate, false);
        clip.SetData(samples, 0);
        return clip;
    }
}
