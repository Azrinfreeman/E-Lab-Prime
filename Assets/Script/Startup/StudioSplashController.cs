using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class StudioSplashController : MonoBehaviour
{
    private const string SeenPrefsPrefix = "StudioSplashSeen_";

    [Header("Profile")]
    public StudioSplashProfile profile;

    [Header("UI References")]
    public CanvasGroup fadeCurtain;
    public CanvasGroup logoGroup;
    public Image backgroundImage;
    public Image logoImage;
    public RectTransform lightSweep;
    public RectTransform logoSheen;
    public RectTransform scanLineA;
    public RectTransform scanLineB;
    public RectTransform[] backgroundParticles = new RectTransform[0];
    public Image progressFill;
    public Image[] progressSegments = new Image[0];
    public TextMeshProUGUI studioNameText;
    public TextMeshProUGUI presentsText;
    public TextMeshProUGUI skipHintText;
    public TextMeshProUGUI loadingText;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip bootChime;
    public AudioClip scanSfx;
    public AudioClip progressTickSfx;

    [Header("Boot Status")]
    public string[] statusMessages =
    {
        "Initializing lab systems...",
        "Loading experiment modules...",
        "Preparing student interface...",
        "Calibrating virtual instruments...",
        "Launching E-Lab Prime..."
    };


    private AsyncOperation loadOperation;
    private bool canSkip;
    private bool skipRequested;
    private float elapsed;
    private int activeStatusIndex = -1;
    private bool scanSfxPlayed;


    private void Awake()
    {
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }

        if (profile != null)
        {
            if (backgroundImage != null)
            {
                backgroundImage.color = profile.backgroundColor;
            }

            if (logoImage != null && profile.logo != null)
            {
                logoImage.sprite = profile.logo;
                logoImage.preserveAspect = true;
            }

            if (audioSource != null && profile.stinger != null)
            {
                audioSource.clip = profile.stinger;
            }
        }

        SetCanvasAlpha(logoGroup, 0f);
        SetCanvasAlpha(fadeCurtain, 1f);
        SetTextAlpha(skipHintText, 0f);

        if (progressFill != null)
        {
            progressFill.fillAmount = 0f;
        }

        SetSegmentedProgress(0f);
        UpdateBootStatus(0f, true);

    }

    private IEnumerator Start()
    {
        ApplyStartupSettings();

        string targetSceneName = GetTargetSceneName();
        if (string.IsNullOrWhiteSpace(targetSceneName))
        {
            Debug.LogError("StudioSplashController has no target scene.");
            yield break;
        }

        if (ShouldBypassSplash())
        {
            SceneManager.LoadScene(targetSceneName);
            yield break;
        }

        if (!Application.CanStreamedLevelBeLoaded(targetSceneName))
        {
            Debug.LogError($"StudioSplashController cannot load scene '{targetSceneName}'. Add it to Build Settings.");
            yield break;
        }

        loadOperation = SceneManager.LoadSceneAsync(targetSceneName);
        if (loadOperation == null)
        {
            Debug.LogError($"StudioSplashController failed to start async load for scene '{targetSceneName}'.");
            yield break;
        }

        loadOperation.allowSceneActivation = false;

        if (bootChime != null)
        {
            PlayOneShotSafe(bootChime, 0.78f);
        }
        else if (audioSource != null && audioSource.clip != null)
        {
            audioSource.Play();
        }

        yield return PlaySplashRoutine();
        MarkSeenIfNeeded();

        loadOperation.allowSceneActivation = true;
    }

    private void Update()
    {
        if (!canSkip || skipRequested)
        {
            return;
        }

        if (WasSkipPressed())
        {
            skipRequested = true;
        }
    }

    private IEnumerator PlaySplashRoutine()
    {
        float fadeIn = GetTiming(profile != null ? profile.fadeInSeconds : 0.6f, 0.1f);
        float hold = GetTiming(profile != null ? profile.holdSeconds : 2.2f, 0f);
        float fadeOut = GetTiming(profile != null ? profile.fadeOutSeconds : 0.7f, 0.1f);
        float minimumDuration = GetTiming(profile != null ? profile.minimumDuration : 4.2f, 1f);
        float targetDuration = Mathf.Max(minimumDuration, fadeIn + hold + fadeOut);
        float skipDelay = GetTiming(profile != null ? profile.skippableAfterSeconds : 1f, 0f);

        while (elapsed < targetDuration && !skipRequested)
        {
            elapsed += Time.unscaledDeltaTime;
            canSkip = elapsed >= skipDelay;
            AnimateSplash(elapsed, targetDuration, fadeIn, fadeOut);
            yield return null;
        }

        canSkip = false;
        SetTextAlpha(skipHintText, 0f);

        while (loadOperation != null && loadOperation.progress < 0.9f)
        {
            if (loadingText != null)
            {
                loadingText.text = "Preparing lab...";
            }

            UpdateProgress(0.92f);
            yield return null;
        }

        yield return FadeOut(fadeOut);
    }

private void AnimateSplash(float time, float totalDuration, float fadeIn, float fadeOut)
    {
        float normalized = Mathf.Clamp01(time / Mathf.Max(0.01f, totalDuration));
        float logoAlpha = Mathf.Clamp01(time / fadeIn);
        float tailAlpha = Mathf.Clamp01((totalDuration - time) / fadeOut);
        float alpha = EaseOut(Mathf.Min(logoAlpha, tailAlpha));

        SetCanvasAlpha(logoGroup, alpha);

        if (time <= fadeIn)
        {
            SetCanvasAlpha(fadeCurtain, 1f - EaseOut(logoAlpha));
        }
        else
        {
            SetCanvasAlpha(fadeCurtain, 0f);
        }

        if (logoGroup != null)
        {
            float scale = Mathf.Lerp(0.94f, 1f, EaseOut(logoAlpha));
            float breathe = Mathf.Sin(time * 1.7f) * 0.006f;
            logoGroup.transform.localScale = new Vector3(scale + breathe, scale + breathe, 1f);
        }

        AnimateScanLine(lightSweep, time, 0.34f, 0.18f);
        AnimateScanLine(scanLineA, time + 0.18f, 0.24f, 0.16f);
        AnimateScanLine(scanLineB, time + 0.72f, 0.18f, 0.10f);
        AnimateLogoSheen(time, logoAlpha, alpha);
        AnimateBackgroundParticles(time, alpha);

        if (!scanSfxPlayed && time > fadeIn + 0.18f)
        {
            scanSfxPlayed = true;
            PlayOneShotSafe(scanSfx, 0.38f);
        }

        if (canSkip)
        {
            SetTextAlpha(skipHintText, Mathf.Clamp01((time - (profile != null ? profile.skippableAfterSeconds : 1f)) / 0.25f));
        }

        UpdateBootStatus(normalized, false);
        UpdateProgress(normalized);
    }

    private IEnumerator FadeOut(float duration)
    {
        if (audioSource != null && audioSource.isPlaying)
        {
            StartCoroutine(FadeAudio(duration));
        }

        float time = 0f;
        while (time < duration)
        {
            time += Time.unscaledDeltaTime;
            SetCanvasAlpha(fadeCurtain, Mathf.Clamp01(time / duration));
            yield return null;
        }

        SetCanvasAlpha(fadeCurtain, 1f);
        UpdateProgress(1f);
    }

    private IEnumerator FadeAudio(float duration)
    {
        float startVolume = audioSource.volume;
        float time = 0f;

        while (time < duration)
        {
            time += Time.unscaledDeltaTime;
            audioSource.volume = Mathf.Lerp(startVolume, 0f, Mathf.Clamp01(time / duration));
            yield return null;
        }

        audioSource.Stop();
        audioSource.volume = startVolume;
    }

    private void ApplyStartupSettings()
    {
        AudioListener.volume = Mathf.Clamp01(PlayerPrefs.GetFloat("MasterVolume", 1f));

        if (PlayerPrefs.HasKey("QualityLevel"))
        {
            int qualityLevel = Mathf.Clamp(PlayerPrefs.GetInt("QualityLevel"), 0, QualitySettings.names.Length - 1);
            QualitySettings.SetQualityLevel(qualityLevel, true);
        }
    }

    private bool ShouldBypassSplash()
    {
        if (profile == null)
        {
            return false;
        }

        if (profile.showEveryLaunch)
        {
            return false;
        }

        if (!profile.showOncePerVersion)
        {
            return true;
        }

        return PlayerPrefs.GetInt(GetSeenPrefsKey(), 0) == 1;
    }

    private void MarkSeenIfNeeded()
    {
        if (profile == null || !profile.showOncePerVersion)
        {
            return;
        }

        PlayerPrefs.SetInt(GetSeenPrefsKey(), 1);
        PlayerPrefs.Save();
    }

    private string GetSeenPrefsKey()
    {
        return SeenPrefsPrefix + Application.version;
    }

    private string GetTargetSceneName()
    {
        return profile != null ? profile.nextSceneName : "MainMenu";
    }

    private bool WasSkipPressed()
    {
        try
        {
            return Input.anyKeyDown || Input.GetMouseButtonDown(0) || Input.touchCount > 0;
        }
        catch (InvalidOperationException)
        {
            return false;
        }
    }

private void UpdateProgress(float animationProgress)
    {
        float loadProgress = loadOperation != null ? Mathf.Clamp01(loadOperation.progress / 0.9f) : 1f;
        float combinedProgress = Mathf.Clamp01((animationProgress * 0.7f) + (loadProgress * 0.3f));

        if (progressFill != null)
        {
            progressFill.fillAmount = combinedProgress;
        }

        SetSegmentedProgress(combinedProgress);
    }

    private static float EaseOut(float value)
    {
        value = Mathf.Clamp01(value);
        return 1f - Mathf.Pow(1f - value, 3f);
    }

    private static float GetTiming(float value, float fallback)
    {
        return value > 0f ? value : fallback;
    }

    private static void SetCanvasAlpha(CanvasGroup group, float alpha)
    {
        if (group == null)
        {
            return;
        }

        group.alpha = Mathf.Clamp01(alpha);
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


private void AnimateScanLine(RectTransform scanLine, float time, float speed, float alpha)
    {
        if (scanLine == null)
        {
            return;
        }

        float x = Mathf.Lerp(-0.18f, 1.18f, Mathf.Repeat(time * speed, 1f));
        float width = 0.0028f;
        scanLine.anchorMin = new Vector2(x - width, scanLine.anchorMin.y);
        scanLine.anchorMax = new Vector2(x + width, scanLine.anchorMax.y);

        Image image = scanLine.GetComponent<Image>();
        if (image != null)
        {
            Color color = image.color;
            color.a = alpha;
            image.color = color;
        }
    }

    private void AnimateLogoSheen(float time, float logoAlpha, float groupAlpha)
    {
        if (logoSheen == null)
        {
            return;
        }

        float sweep = Mathf.Clamp01((time - 0.62f) / 1.3f);
        float x = Mathf.Lerp(-0.34f, 1.34f, sweep);
        logoSheen.anchorMin = new Vector2(x - 0.045f, logoSheen.anchorMin.y);
        logoSheen.anchorMax = new Vector2(x + 0.045f, logoSheen.anchorMax.y);

        Image image = logoSheen.GetComponent<Image>();
        if (image != null)
        {
            Color color = image.color;
            color.a = Mathf.Sin(Mathf.Clamp01(sweep) * Mathf.PI) * 0.34f * logoAlpha * groupAlpha;
            image.color = color;
        }
    }

    private void AnimateBackgroundParticles(float time, float alpha)
    {
        if (backgroundParticles == null)
        {
            return;
        }

        for (int i = 0; i < backgroundParticles.Length; i++)
        {
            RectTransform particle = backgroundParticles[i];
            if (particle == null)
            {
                continue;
            }

            float offset = Mathf.Sin((time * (0.55f + i * 0.03f)) + i * 1.7f) * 9f;
            particle.anchoredPosition = new Vector2(particle.anchoredPosition.x, offset);

            Image image = particle.GetComponent<Image>();
            if (image != null)
            {
                Color color = image.color;
                color.a = (0.05f + Mathf.Abs(Mathf.Sin(time * 0.9f + i)) * 0.12f) * alpha;
                image.color = color;
            }
        }
    }

    private void UpdateBootStatus(float normalizedProgress, bool force)
    {
        if (loadingText == null || statusMessages == null || statusMessages.Length == 0)
        {
            return;
        }

        int nextIndex = Mathf.Clamp(Mathf.FloorToInt(normalizedProgress * statusMessages.Length), 0, statusMessages.Length - 1);
        if (!force && nextIndex == activeStatusIndex)
        {
            return;
        }

        activeStatusIndex = nextIndex;
        loadingText.text = statusMessages[activeStatusIndex];

        if (!force)
        {
            PlayOneShotSafe(progressTickSfx, 0.18f);
        }
    }

    private void SetSegmentedProgress(float progress)
    {
        if (progressSegments == null || progressSegments.Length == 0)
        {
            return;
        }

        float active = progress * progressSegments.Length;
        for (int i = 0; i < progressSegments.Length; i++)
        {
            Image segment = progressSegments[i];
            if (segment == null)
            {
                continue;
            }

            Color color = segment.color;
            color.a = i < active ? 0.95f : 0.16f;
            segment.color = color;
        }
    }

    private void PlayOneShotSafe(AudioClip clip, float volume)
    {
        if (audioSource == null || clip == null)
        {
            return;
        }

        audioSource.PlayOneShot(clip, Mathf.Clamp01(volume));
    }
}
