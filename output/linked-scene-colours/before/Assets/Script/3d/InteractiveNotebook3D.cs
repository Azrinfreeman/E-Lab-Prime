using UnityEngine;
using UnityEngine.UI;

public class InteractiveNotebook3D : MonoBehaviour
{
    [Header("References")]
    public WaterHeatingManager3D heatingManager;
    
    [Header("UI Overlay Panel")]
    public GameObject uiPanel;
    public InputField inputA;
    public InputField inputB;
    public Text feedbackText;

    [Header("3D Pages Texts")]
    public TextMesh leftPageTextMesh;
    public TextMesh rightPageTextMesh;

    private AudioSource audioSource;
    private AudioClip submitSuccessSFX;
    private AudioClip errorSFX;
    private bool isLoggingCompleted = false;
    private bool buttonsAreBound = false;

    private MeshRenderer coverRenderer;
    private Material coverMaterialInstance;
    private Color originalCoverColor;

    void Start()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0.5f;
        audioSource.volume = 0.6f;

        submitSuccessSFX = CreatePencilScratchSound();
        errorSFX = CreateErrorBuzzerSound();

        // Clear input values
        if (inputA != null) inputA.text = "";
        if (inputB != null) inputB.text = "";
        if (feedbackText != null) feedbackText.text = "";

        // Resolve InputField references from uiPanel hierarchy if not serialized
        if (uiPanel != null)
        {
            if (inputA == null)
            {
                Transform tA = uiPanel.transform.Find("InputA");
                if (tA != null) inputA = tA.GetComponent<InputField>();
            }
            if (inputB == null)
            {
                Transform tB = uiPanel.transform.Find("InputB");
                if (tB != null) inputB = tB.GetComponent<InputField>();
            }
            if (feedbackText == null)
            {
                Transform tF = uiPanel.transform.Find("FeedbackText");
                if (tF != null) feedbackText = tF.GetComponent<Text>();
            }
        }

        // NOTE: We do NOT use GameObject.Find for button binding here because the panel
        // starts inactive and GameObject.Find cannot locate inactive GameObjects.
        // Instead, we bind buttons lazily in BindButtonsIfNeeded() when the panel opens.

        // Find 3D TextMesh components on start if not assigned
        if (leftPageTextMesh == null)
        {
            Transform t = transform.Find("LeftPageText");
            if (t != null) leftPageTextMesh = t.GetComponent<TextMesh>();
        }
        if (rightPageTextMesh == null)
        {
            Transform t = transform.Find("RightPageText");
            if (t != null) rightPageTextMesh = t.GetComponent<TextMesh>();
        }

        // Initially hide UI panel
        if (uiPanel != null) uiPanel.SetActive(false);

        // Make sure the 3D right page does not display final values yet
        if (rightPageTextMesh != null)
        {
            rightPageTextMesh.text = "PEMERHATIAN\n\nTakat Didih:\nBeaker A (Tulen): ___ °C\nBeaker B (Garam): ___ °C\n\nSila rekodkan takat didih\nke dalam buku nota\nsesudah air mendidih.";
        }

        // Setup cover highlight material
        Transform coverT = transform.Find("NotebookCover");
        if (coverT != null)
        {
            coverRenderer = coverT.GetComponent<MeshRenderer>();
            if (coverRenderer != null && coverRenderer.sharedMaterial != null)
            {
                coverMaterialInstance = new Material(coverRenderer.sharedMaterial);
                coverRenderer.material = coverMaterialInstance;
                originalCoverColor = coverMaterialInstance.color;
            }
        }
    }

    void Update()
    {
        bool isPanelOpen = uiPanel != null && uiPanel.activeSelf;
        if (heatingManager != null && heatingManager.IsExperimentFinished && !isLoggingCompleted && !isPanelOpen)
        {
            if (coverMaterialInstance != null)
            {
                float pulse = Mathf.PingPong(Time.time * 2.5f, 1f);
                // Pulse between original color and a bright yellowish highlight
                coverMaterialInstance.color = Color.Lerp(originalCoverColor, new Color(1.0f, 0.95f, 0.4f), pulse);
            }
        }
        else if (coverMaterialInstance != null && coverMaterialInstance.color != originalCoverColor)
        {
            // Reset to original color once clicked/logged or if not yet finished
            coverMaterialInstance.color = originalCoverColor;
        }
    }

    /// <summary>
    /// Binds button onClick listeners by walking the uiPanel transform hierarchy.
    /// Transform.Find works on inactive children (unlike GameObject.Find).
    /// Called once when the panel is first opened.
    /// </summary>
    private void BindButtonsIfNeeded()
    {
        if (buttonsAreBound) return;
        if (uiPanel == null) return;

        // Find the Buttons container inside the panel, then locate each button
        Transform btnContainer = uiPanel.transform.Find("Buttons");
        if (btnContainer != null)
        {
            Transform submitT = btnContainer.Find("Btn_SubmitLog");
            if (submitT != null)
            {
                Button btn = submitT.GetComponent<Button>();
                if (btn != null)
                {
                    btn.onClick.RemoveAllListeners();
                    btn.onClick.AddListener(SubmitRecords);
                }
            }

            Transform closeT = btnContainer.Find("Btn_CloseLog");
            if (closeT != null)
            {
                Button btn = closeT.GetComponent<Button>();
                if (btn != null)
                {
                    btn.onClick.RemoveAllListeners();
                    btn.onClick.AddListener(ClosePanel);
                }
            }
        }
        else
        {
            // Fallback: search all children recursively
            BindButtonInChildren(uiPanel.transform, "Btn_SubmitLog", SubmitRecords);
            BindButtonInChildren(uiPanel.transform, "Btn_CloseLog", ClosePanel);
        }

        buttonsAreBound = true;
    }

    private void BindButtonInChildren(Transform parent, string buttonName, UnityEngine.Events.UnityAction callback)
    {
        foreach (Transform child in parent)
        {
            if (child.name == buttonName)
            {
                Button btn = child.GetComponent<Button>();
                if (btn != null)
                {
                    btn.onClick.RemoveAllListeners();
                    btn.onClick.AddListener(callback);
                }
                return;
            }
            // Recurse
            BindButtonInChildren(child, buttonName, callback);
        }
    }

    void OnMouseDown()
    {
        // If the panel is already open, don't re-trigger (prevents clearing typed input)
        if (uiPanel != null && uiPanel.activeSelf) return;

        if (heatingManager != null && !heatingManager.IsExperimentFinished)
        {
            // Experiment not finished, show warning on hologram
            heatingManager.ShowHologramTemporaryMessage("INFO: Sila tunggu sehingga\nair di kedua-dua beaker mendidih!", 3f, Color.yellow);
            return;
        }

        // Transition camera to Textbook View
        CameraDragArea dragArea = Object.FindAnyObjectByType<CameraDragArea>();
        if (dragArea != null)
        {
            dragArea.SetTextbookView();
        }

        // Show UI Overlay Panel and bind buttons
        if (uiPanel != null)
        {
            uiPanel.SetActive(true);

            // Bind button listeners now that the panel is active
            BindButtonsIfNeeded();

            // Toggle visibility of input fields and labels based on completion status
            if (inputA != null) inputA.gameObject.SetActive(!isLoggingCompleted);
            if (inputB != null) inputB.gameObject.SetActive(!isLoggingCompleted);

            Transform labelAT = uiPanel.transform.Find("LabelA");
            if (labelAT != null) labelAT.gameObject.SetActive(!isLoggingCompleted);

            Transform labelBT = uiPanel.transform.Find("LabelB");
            if (labelBT != null) labelBT.gameObject.SetActive(!isLoggingCompleted);

            Transform descT = uiPanel.transform.Find("DescText");
            if (descT != null) descT.gameObject.SetActive(!isLoggingCompleted);

            // Toggle Submit button and reposition Close button
            Transform btnContainer = uiPanel.transform.Find("Buttons");
            Transform submitT = btnContainer != null ? btnContainer.Find("Btn_SubmitLog") : uiPanel.transform.Find("Btn_SubmitLog");
            if (submitT != null) submitT.gameObject.SetActive(!isLoggingCompleted);

            Transform closeT = btnContainer != null ? btnContainer.Find("Btn_CloseLog") : uiPanel.transform.Find("Btn_CloseLog");
            if (closeT != null)
            {
                RectTransform closeRect = closeT.GetComponent<RectTransform>();
                if (closeRect != null)
                {
                    if (isLoggingCompleted)
                    {
                        // Center the Close button
                        closeRect.anchorMin = new Vector2(0.5f, 0.5f);
                        closeRect.anchorMax = new Vector2(0.5f, 0.5f);
                        closeRect.pivot = new Vector2(0.5f, 0.5f);
                        closeRect.anchoredPosition = new Vector2(0f, 0f);
                    }
                    else
                    {
                        // Restore Close button to the right side
                        closeRect.anchorMin = new Vector2(1f, 0.5f);
                        closeRect.anchorMax = new Vector2(1f, 0.5f);
                        closeRect.pivot = new Vector2(1f, 0.5f);
                        closeRect.anchoredPosition = new Vector2(0f, 0f);
                    }
                }
            }

            if (feedbackText != null)
            {
                feedbackText.text = isLoggingCompleted ? "Pemerhatian telah direkodkan." : "Sila masukkan suhu takat didih air tulen dan air garam.";
                feedbackText.color = isLoggingCompleted ? new Color(0.1f, 0.6f, 0.2f) : new Color(0.2f, 0.2f, 0.2f);
            }

            if (!isLoggingCompleted)
            {
                if (inputA != null)
                {
                    inputA.text = "";
                    inputA.ForceLabelUpdate();
                    inputA.ActivateInputField();
                }
                if (inputB != null)
                {
                    inputB.text = "";
                    inputB.ForceLabelUpdate();
                }
            }
        }
    }

    public void ClosePanel()
    {
        if (uiPanel != null) uiPanel.SetActive(false);
        
        // Snap back camera to front view
        CameraDragArea dragArea = Object.FindAnyObjectByType<CameraDragArea>();
        if (dragArea != null)
        {
            dragArea.SetFrontView();
        }
    }

    public void SubmitRecords()
    {
        if (inputA == null || inputB == null)
        {
            Debug.LogWarning("[InteractiveNotebook3D] inputA or inputB is null! Cannot submit.");
            return;
        }

        string textA = inputA.text != null ? inputA.text.Trim() : "";
        string textB = inputB.text != null ? inputB.text.Trim() : "";

        float valA, valB;
        bool isAValid = float.TryParse(textA, out valA);
        bool isBValid = float.TryParse(textB, out valB);

        if (!isAValid || !isBValid)
        {
            PlaySound(errorSFX);
            ShowFeedback("Sila masukkan nilai nombor sahaja!", Color.red);
            return;
        }

        // Tolerance: +/- 0.5 degC around 100 and 104
        bool isACorrect = Mathf.Abs(valA - 100f) <= 0.5f;
        bool isBCorrect = Mathf.Abs(valB - 104f) <= 0.5f;

        if (isACorrect && isBCorrect)
        {
            isLoggingCompleted = true;
            PlaySound(submitSuccessSFX);
            
            // Update 3D Notebook physical text
            if (rightPageTextMesh != null)
            {
                rightPageTextMesh.text = $"PEMERHATIAN\n\nTakat Didih:\nBeaker A (Tulen): {valA:F1} °C\nBeaker B (Garam): {valB:F1} °C\n\nRekod disahkan betul!\nHipotesis DITERIMA.";
            }

            ShowFeedback("Rekod Berjaya! Menyalakan kuiz...", new Color(0.1f, 0.6f, 0.2f));
            
            // Close panel and notify manager after a delay
            Invoke("CompleteAndTriggerQuiz", 2.0f);
        }
        else
        {
            PlaySound(errorSFX);
            if (!isACorrect && !isBCorrect)
            {
                ShowFeedback("Kedua-dua bacaan tidak tepat! Semak semula termometer.", Color.red);
            }
            else if (!isACorrect)
            {
                ShowFeedback("Suhu Beaker A tidak tepat!", Color.red);
            }
            else
            {
                ShowFeedback("Suhu Beaker B tidak tepat!", Color.red);
            }
        }
    }

    private void CompleteAndTriggerQuiz()
    {
        if (uiPanel != null) uiPanel.SetActive(false);

        // Snap back camera to front view
        CameraDragArea dragArea = Object.FindAnyObjectByType<CameraDragArea>();
        if (dragArea != null)
        {
            dragArea.SetFrontView();
        }

        if (heatingManager != null)
        {
            heatingManager.CompleteNotebookLogging();
        }
    }

    private void ShowFeedback(string msg, Color col)
    {
        if (feedbackText != null)
        {
            feedbackText.text = msg;
            feedbackText.color = col;
        }
    }

    private void PlaySound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
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
            // Pencil scratching noise (band-pass filtered style white noise with envelopes)
            float noise1 = (Random.value * 2f - 1f) * 0.12f * Mathf.Sin(2f * Mathf.PI * 400f * t);
            float noise2 = (Random.value * 2f - 1f) * 0.08f * Mathf.Sin(2f * Mathf.PI * 800f * t);
            // Add a rhythmic envelope to simulate writing strokes
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
            // Short buzzer sound
            float buzz = Mathf.Sign(Mathf.Sin(2f * Mathf.PI * 150f * t));
            samples[i] = buzz * 0.15f * (1.0f - t);
        }
        AudioClip clip = AudioClip.Create("ErrorBuzzSFX", numSamples, 1, samplerate, false);
        clip.SetData(samples, 0);
        return clip;
    }

    public void ResetNotebook()
    {
        isLoggingCompleted = false;
        buttonsAreBound = false;
        if (inputA != null) inputA.text = "";
        if (inputB != null) inputB.text = "";
        if (feedbackText != null) feedbackText.text = "";
        if (uiPanel != null) uiPanel.SetActive(false);

        if (rightPageTextMesh != null)
        {
            rightPageTextMesh.text = "PEMERHATIAN\n\nTakat Didih:\nBeaker A (Tulen): ___ °C\nBeaker B (Garam): ___ °C\n\nSila rekodkan takat didih\nke dalam buku nota\nsesudah air mendidih.";
        }
    }
}
