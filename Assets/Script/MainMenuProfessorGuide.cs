using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuProfessorGuide : MonoBehaviour
{
    [Header("UI")]
    public CanvasGroup rootGroup;
    public Image professorImage;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI malayText;
    public TextMeshProUGUI englishSubtitleText;
    public Button continueButton;
    public Button skipButton;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip appearSfx;
    public AudioClip textBlipSfx;
    public AudioClip highlightSfx;

    [Header("Dialogue")]
    public string professorName = "PROF. HAKIM";
    public float typewriterCharactersPerSecond = 36f;
    public float introFadeSeconds = 0.35f;
    public float hoverPromptCooldown = 1.2f;
    public float startupDelaySeconds = 0.35f;
    public float avatarBreathScale = 0.018f;
    public List<ProfessorDialogueLine> dialogueLines = new List<ProfessorDialogueLine>();
    public List<ProfessorGuideTarget> targets = new List<ProfessorGuideTarget>();

    [Header("Avatar Poses")]
    public Sprite idleSprite;
    public Sprite talkSprite;
    public Sprite pointSprite;
    public Sprite smileSprite;


    private static bool startupGuideConsumed;
    private static readonly HashSet<ProfessorGuideContext> playedPromptsThisSession = new HashSet<ProfessorGuideContext>();

    private readonly Queue<ProfessorDialogueLine> queuedLines = new Queue<ProfessorDialogueLine>();

    private Coroutine activeRoutine;
    private Coroutine fadeRoutine;
    private string activeMalayText = string.Empty;
    private bool isTyping;
    private bool isIntroSequence;
    private float lastPromptTime;
    private ProfessorAvatarPose activePose = ProfessorAvatarPose.Idle;
    private RectTransform professorRect;
    private Vector3 professorBaseScale = Vector3.one;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetSessionState()
    {
        startupGuideConsumed = false;
        playedPromptsThisSession.Clear();
    }

    private void Awake()
    {
        EnsureDefaultDialogue();

        if (nameText != null)
        {
            nameText.text = professorName;
        }

        if (professorImage != null)
        {
            professorRect = professorImage.GetComponent<RectTransform>();
            if (professorRect != null)
            {
                professorBaseScale = professorRect.localScale;
            }

            if (idleSprite != null)
            {
                professorImage.sprite = idleSprite;
            }
        }


        if (continueButton != null)
        {
            continueButton.onClick.RemoveListener(Continue);
            continueButton.onClick.AddListener(Continue);
        }

        if (skipButton != null)
        {
            skipButton.onClick.RemoveListener(SkipGuide);
            skipButton.onClick.AddListener(SkipGuide);
        }

        SetRootVisible(false, true);
        HideAllHighlights();
    }

    private void Start()
    {
        StartCoroutine(BeginAfterStartupDelay());
    }

    private IEnumerator BeginAfterStartupDelay()
    {
        while (FindAnyObjectByType<OpeningSequenceController>() != null)
        {
            yield return null;
        }

        if (startupGuideConsumed)
        {
            SetRootVisible(false, true);
            HideAllHighlights();
            yield break;
        }

        startupGuideConsumed = true;

        if (startupDelaySeconds > 0f)
        {
            yield return new WaitForSecondsRealtime(startupDelaySeconds);
        }

        PlayIntroSequence();
    }

    private void Update()
    {
        if (professorRect == null || rootGroup == null || rootGroup.alpha <= 0.01f)
        {
            return;
        }

        float pulse = 1f + Mathf.Sin(Time.unscaledTime * 1.6f) * avatarBreathScale;
        professorRect.localScale = professorBaseScale * pulse;
    }

    
public void ShowStartExperimentsHint()
    {
        PlayPrompt(ProfessorGuideContext.StartExperiments);
    }

    public void ShowSettingsHint()
    {
        PlayPrompt(ProfessorGuideContext.Settings);
    }

    public void ShowAchievementsHint()
    {
        PlayPrompt(ProfessorGuideContext.Achievements);
    }

    public void ShowSignInHint()
    {
        PlayPrompt(GetSignedInContext());
    }

    public void PlayPrompt(ProfessorGuideContext context)
    {
        if (isIntroSequence || playedPromptsThisSession.Contains(context))
        {
            return;
        }

        if (lastPromptTime > 0f && Time.unscaledTime - lastPromptTime < hoverPromptCooldown)
        {
            return;
        }

        ProfessorDialogueLine line = FindLine(context);
        if (line == null)
        {
            return;
        }

        lastPromptTime = Time.unscaledTime;
        playedPromptsThisSession.Add(context);
        PlaySingleLine(context);
    }

    private void PlayIntroSequence()
    {
        isIntroSequence = true;
        queuedLines.Clear();

        Enqueue(ProfessorGuideContext.FirstLaunchWelcome);
        Enqueue(GetSignedInContext());
        Enqueue(ProfessorGuideContext.StartExperiments);

        PlayNextQueuedLine();
    }

    private ProfessorGuideContext GetSignedInContext()
    {
        bool signedIn = PlayerPrefs.GetInt("ELabPrime_AccountSignedIn", 0) == 1;
        bool guest = PlayerPrefs.GetInt("ELabPrime_AccountGuest", 0) == 1;
        return signedIn || guest ? ProfessorGuideContext.SignedInWelcome : ProfessorGuideContext.SignedOutPrompt;
    }

    private void Enqueue(ProfessorGuideContext context)
    {
        ProfessorDialogueLine line = FindLine(context);
        if (line != null)
        {
            queuedLines.Enqueue(line);
        }
    }

    private void PlaySingleLine(ProfessorGuideContext context)
    {
        ProfessorDialogueLine line = FindLine(context);
        if (line == null)
        {
            return;
        }

        queuedLines.Clear();
        isIntroSequence = false;
        ShowLine(line);
    }

    private void PlayNextQueuedLine()
    {
        if (queuedLines.Count == 0)
        {
            FinishIntroSequence();
            return;
        }

        ShowLine(queuedLines.Dequeue());
    }

private void ShowLine(ProfessorDialogueLine line)
    {
        if (line == null)
        {
            return;
        }

        if (activeRoutine != null)
        {
            StopCoroutine(activeRoutine);
        }

        activeMalayText = line.malayText ?? string.Empty;
        activePose = line.avatarPose;
        ApplyPose(activePose);

        if (englishSubtitleText != null)
        {
            englishSubtitleText.text = line.englishSubtitle ?? string.Empty;
        }

        if (skipButton != null)
        {
            skipButton.gameObject.SetActive(line.skippable);
        }

        HighlightTarget(line.targetKey);
        SetRootVisible(true, false);

        if (audioSource != null && appearSfx != null)
        {
            audioSource.PlayOneShot(appearSfx, 0.45f);
        }

        if (audioSource != null && line.voiceClip != null && line.voiceClip.length > 0.2f)
        {
            audioSource.PlayOneShot(line.voiceClip, 0.8f);
        }

        activeRoutine = StartCoroutine(TypeLine(activeMalayText));
    }

private IEnumerator TypeLine(string text)
    {
        isTyping = true;

        if (malayText != null)
        {
            malayText.text = string.Empty;
        }

        if (!string.IsNullOrEmpty(text) && professorImage != null && talkSprite != null)
        {
            professorImage.sprite = talkSprite;
        }

        float delay = 1f / Mathf.Max(1f, typewriterCharactersPerSecond);
        int blipStep = 0;

        for (int i = 0; i < text.Length; i++)
        {
            if (malayText != null)
            {
                malayText.text = text.Substring(0, i + 1);
            }

            if (!char.IsWhiteSpace(text[i]))
            {
                blipStep++;
                if (blipStep % 3 == 0 && audioSource != null && textBlipSfx != null)
                {
                    audioSource.PlayOneShot(textBlipSfx, 0.14f);
                }
            }

            yield return new WaitForSecondsRealtime(delay);
        }

        isTyping = false;
        ApplyPose(activePose);
    }

    private void Continue()
    {
        if (isTyping)
        {
            CompleteTyping();
            return;
        }

        if (isIntroSequence)
        {
            PlayNextQueuedLine();
            return;
        }

        SetRootVisible(false, false);
        HideAllHighlights();
    }

    private void SkipGuide()
    {
        if (activeRoutine != null)
        {
            StopCoroutine(activeRoutine);
            activeRoutine = null;
        }

        isTyping = false;
        queuedLines.Clear();
        FinishIntroSequence();
        SetRootVisible(false, false);
        HideAllHighlights();
    }

private void CompleteTyping()
    {
        if (activeRoutine != null)
        {
            StopCoroutine(activeRoutine);
            activeRoutine = null;
        }

        if (malayText != null)
        {
            malayText.text = activeMalayText;
        }

        isTyping = false;
        ApplyPose(activePose);
    }

    private void FinishIntroSequence()
    {
        isIntroSequence = false;
    }

    private void SetRootVisible(bool visible, bool immediate)
    {
        if (rootGroup == null)
        {
            return;
        }

        if (fadeRoutine != null)
        {
            StopCoroutine(fadeRoutine);
        }

        if (immediate)
        {
            rootGroup.alpha = visible ? 1f : 0f;
            rootGroup.blocksRaycasts = visible;
            rootGroup.interactable = visible;
            return;
        }

        fadeRoutine = StartCoroutine(FadeRoot(visible));
    }

    private IEnumerator FadeRoot(bool visible)
    {
        rootGroup.blocksRaycasts = visible;
        rootGroup.interactable = visible;

        float start = rootGroup.alpha;
        float end = visible ? 1f : 0f;
        float duration = Mathf.Max(0.01f, introFadeSeconds);
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            rootGroup.alpha = Mathf.Lerp(start, end, elapsed / duration);
            yield return null;
        }

        rootGroup.alpha = end;
        rootGroup.blocksRaycasts = visible;
        rootGroup.interactable = visible;
    }

    private ProfessorDialogueLine FindLine(ProfessorGuideContext context)
    {
        for (int i = 0; i < dialogueLines.Count; i++)
        {
            if (dialogueLines[i] != null && dialogueLines[i].context == context)
            {
                return dialogueLines[i];
            }
        }

        return null;
    }

private void HighlightTarget(string targetKey)
    {
        HideAllHighlights();

        if (string.IsNullOrEmpty(targetKey))
        {
            return;
        }

        for (int i = 0; i < targets.Count; i++)
        {
            ProfessorGuideTarget target = targets[i];
            if (target != null && target.key == targetKey && target.highlight != null)
            {
                target.highlight.gameObject.SetActive(true);
                target.highlight.CrossFadeAlpha(0.82f, 0.1f, true);
                if (audioSource != null && highlightSfx != null)
                {
                    audioSource.PlayOneShot(highlightSfx, 0.36f);
                }

                StartCoroutine(PulseHighlight(target.highlight));
                return;
            }
        }
    }

    private IEnumerator PulseHighlight(Image highlight)
    {
        if (highlight == null)
        {
            yield break;
        }

        RectTransform rect = highlight.GetComponent<RectTransform>();
        float elapsed = 0f;

        while (highlight != null && highlight.gameObject.activeSelf && elapsed < 3.5f)
        {
            elapsed += Time.unscaledDeltaTime;
            float pulse = 1f + Mathf.Sin(elapsed * 5f) * 0.035f;
            if (rect != null)
            {
                rect.localScale = new Vector3(pulse, pulse, 1f);
            }

            yield return null;
        }
    }

    private void HideAllHighlights()
    {
        for (int i = 0; i < targets.Count; i++)
        {
            if (targets[i] != null && targets[i].highlight != null)
            {
                targets[i].highlight.gameObject.SetActive(false);
                targets[i].highlight.rectTransform.localScale = Vector3.one;
            }
        }
    }

    private void EnsureDefaultDialogue()
    {
        if (dialogueLines != null && dialogueLines.Count > 0)
        {
            return;
        }

        dialogueLines = new List<ProfessorDialogueLine>
        {
            new ProfessorDialogueLine
            {
                context = ProfessorGuideContext.FirstLaunchWelcome,
                malayText = "Selamat datang ke Makmal Sains Digital.",
                englishSubtitle = "[Welcome to the Digital Science Lab.]"
            },
            new ProfessorDialogueLine
            {
                context = ProfessorGuideContext.SignedOutPrompt,
                malayText = "Log masuk dahulu supaya kemajuan eksperimen anda dapat disimpan.",
                englishSubtitle = "[Sign in first so your experiment progress can be saved.]",
                targetKey = "SignIn"
            },
            new ProfessorDialogueLine
            {
                context = ProfessorGuideContext.SignedInWelcome,
                malayText = "Selamat kembali. Mari sambung penerokaan sains anda.",
                englishSubtitle = "[Welcome back. Let us continue your science exploration.]"
            },
            new ProfessorDialogueLine
            {
                context = ProfessorGuideContext.StartExperiments,
                malayText = "Tekan Mulakan Amali untuk memilih eksperimen sains anda.",
                englishSubtitle = "[Press Start Experiments to choose your science activity.]",
                targetKey = "StartExperiments"
            },
            new ProfessorDialogueLine
            {
                context = ProfessorGuideContext.Settings,
                malayText = "Laraskan bunyi, paparan, dan pengalaman pembelajaran di sini.",
                englishSubtitle = "[Adjust sound, display, and learning experience here.]",
                targetKey = "Settings"
            },
            new ProfessorDialogueLine
            {
                context = ProfessorGuideContext.Achievements,
                malayText = "Di sini anda boleh melihat pencapaian dan kemajuan setiap amali.",
                englishSubtitle = "[Here you can view achievements and progress for each experiment.]",
                targetKey = "Achievements"
            }
        };
    }


private void ApplyPose(ProfessorAvatarPose pose)
    {
        if (professorImage == null)
        {
            return;
        }

        Sprite sprite = idleSprite;
        if (pose == ProfessorAvatarPose.Talk && talkSprite != null)
        {
            sprite = talkSprite;
        }
        else if (pose == ProfessorAvatarPose.Point && pointSprite != null)
        {
            sprite = pointSprite;
        }
        else if (pose == ProfessorAvatarPose.Smile && smileSprite != null)
        {
            sprite = smileSprite;
        }

        if (sprite != null)
        {
            professorImage.sprite = sprite;
        }
    }
}
