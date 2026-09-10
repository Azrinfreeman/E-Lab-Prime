using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class OpeningSequenceController : MonoBehaviour
{
    public static event System.Action SequenceFinished;

    private static bool playedThisSession;

    [Header("Overlay")]
    public CanvasGroup overlayGroup;
    public RectTransform scanlineSweep;
    public GameObject statusStamp;
    public Button skipButton;

    [Header("Text")]
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI subtitleText;
    public TextMeshProUGUI identityText;
    public TextMeshProUGUI buildText;
    public List<TextMeshProUGUI> checklistItems = new List<TextMeshProUGUI>();

    [Header("Progress")]
    public Image progressFill;

    [Header("Timing")]
    public float firstRunDuration = 4.8f;
    public float repeatDuration = 1.8f;
    public float skipDelay = 1f;
    public float fadeDuration = 0.28f;
    public float scanlineSpeed = 0.35f;

    private bool skipRequested;
    private Coroutine sequenceRoutine;

    private void Awake()
    {
        if (buildText != null)
        {
            buildText.text = "v" + Application.version;
        }

        if (skipButton != null)
        {
            skipButton.onClick.RemoveListener(RequestSkip);
            skipButton.onClick.AddListener(RequestSkip);
            skipButton.gameObject.SetActive(false);
        }
    }

    private void OnEnable()
    {
        sequenceRoutine = StartCoroutine(PlaySequence());
    }

    private void OnDisable()
    {
        if (sequenceRoutine != null)
        {
            StopCoroutine(sequenceRoutine);
            sequenceRoutine = null;
        }
    }

    private void RequestSkip()
    {
        skipRequested = true;
    }

    private IEnumerator PlaySequence()
    {
        skipRequested = false;

        float duration = playedThisSession ? repeatDuration : firstRunDuration;
        duration = Mathf.Max(duration, fadeDuration * 2f + 0.25f);

        if (overlayGroup != null)
        {
            overlayGroup.alpha = 1f;
            overlayGroup.interactable = true;
            overlayGroup.blocksRaycasts = true;
        }

        if (statusStamp != null)
        {
            statusStamp.SetActive(false);
        }

        if (progressFill != null)
        {
            progressFill.fillAmount = 0f;
        }

        SetChecklistAlpha(0f);

        float elapsed = 0f;
        while (elapsed < duration && !skipRequested)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / duration);

            if (progressFill != null)
            {
                progressFill.fillAmount = t;
            }

            AnimateScanline(elapsed);
            RevealChecklist(t);

            if (elapsed >= skipDelay && skipButton != null && !skipButton.gameObject.activeSelf)
            {
                skipButton.gameObject.SetActive(true);
            }

            if (statusStamp != null && t > 0.72f && !statusStamp.activeSelf)
            {
                statusStamp.SetActive(true);
            }

            yield return null;
        }

        playedThisSession = true;

        if (skipButton != null)
        {
            skipButton.gameObject.SetActive(false);
        }

        yield return FadeOverlayOut();
        gameObject.SetActive(false);
        SequenceFinished?.Invoke();
    }

    private IEnumerator FadeOverlayOut()
    {
        if (overlayGroup == null)
        {
            yield break;
        }

        float startAlpha = overlayGroup.alpha;
        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            overlayGroup.alpha = Mathf.Lerp(startAlpha, 0f, Mathf.Clamp01(elapsed / fadeDuration));
            yield return null;
        }

        overlayGroup.alpha = 0f;
        overlayGroup.interactable = false;
        overlayGroup.blocksRaycasts = false;
    }

    private void AnimateScanline(float elapsed)
    {
        if (scanlineSweep == null)
        {
            return;
        }

        float x = Mathf.Lerp(-0.15f, 1.15f, Mathf.Repeat(elapsed * scanlineSpeed, 1f));
        scanlineSweep.anchorMin = new Vector2(x - 0.1f, scanlineSweep.anchorMin.y);
        scanlineSweep.anchorMax = new Vector2(x + 0.1f, scanlineSweep.anchorMax.y);
    }

    private void RevealChecklist(float progress)
    {
        if (checklistItems == null || checklistItems.Count == 0)
        {
            return;
        }

        for (int i = 0; i < checklistItems.Count; i++)
        {
            TextMeshProUGUI item = checklistItems[i];
            if (item == null)
            {
                continue;
            }

            float threshold = (i + 1f) / (checklistItems.Count + 1f);
            SetTextAlpha(item, progress >= threshold ? 1f : 0.25f);
        }
    }

    private void SetChecklistAlpha(float alpha)
    {
        if (checklistItems == null)
        {
            return;
        }

        foreach (TextMeshProUGUI item in checklistItems)
        {
            SetTextAlpha(item, alpha);
        }
    }

    private static void SetTextAlpha(TextMeshProUGUI text, float alpha)
    {
        if (text == null)
        {
            return;
        }

        Color color = text.color;
        color.a = Mathf.Clamp01(alpha);
        text.color = color;
    }
}
