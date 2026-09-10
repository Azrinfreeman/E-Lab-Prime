using System;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class StudioSplashBootSetup
{
    private const string BootScenePath = "Assets/Scenes/Boot.unity";
    private const string PrimaryLogoSourcePath = "Logo/pelangi_logo.png";
    private const string SecondaryLogoSourcePath = "Logo/USM APEX-Hor-R.png";
    private const string PrimaryLogoAssetPath = "Assets/Branding/StudioSplash/Textures/pelangi_logo.png";
    private const string SecondaryLogoAssetPath = "Assets/Branding/StudioSplash/Textures/USM APEX-Hor-R.png";
    private const string ProfileAssetPath = "Assets/Branding/StudioSplash/Profiles/StudioSplashProfile.asset";
    private const string FontAssetPath = "Assets/Fonts/FredokaOne SDF.asset";
    private const string DefaultStingerPath = "Assets/Branding/StudioSplash/Audio/splash_boot_chime.wav";
    private const string ScanSfxPath = "Assets/Branding/StudioSplash/Audio/splash_scan.wav";
    private const string ProgressTickSfxPath = "Assets/Branding/StudioSplash/Audio/splash_progress_tick.wav";

    private static readonly Color BackgroundColor = new Color(0.015f, 0.032f, 0.055f, 1f);
    private static readonly Color AccentColor = new Color(0.18f, 0.77f, 0.71f, 1f);
    private static readonly Color SoftWhite = new Color(0.92f, 0.97f, 1f, 1f);

    [MenuItem("E-Lab Prime/Setup Studio Splash Boot")]
    public static void SetupStudioSplashBoot()
    {
        EnsureFolders();
        CopyBootLogosIntoAssets();
        AssetDatabase.Refresh();
        ConfigureLogoImporter(PrimaryLogoAssetPath);
        ConfigureLogoImporter(SecondaryLogoAssetPath);

        StudioSplashProfile profile = CreateOrUpdateProfile();

        EditorSceneManager.SaveOpenScenes();
        Scene bootScene = File.Exists(BootScenePath)
            ? EditorSceneManager.OpenScene(BootScenePath, OpenSceneMode.Single)
            : EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        RebuildBootScene(bootScene, profile);
        EditorSceneManager.SaveScene(bootScene, BootScenePath);
        UpdateBuildSettings();

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("Studio Splash Boot scene created and build settings updated.");
    }

    private static void EnsureFolders()
    {
        Directory.CreateDirectory("Assets/Branding/StudioSplash/Textures");
        Directory.CreateDirectory("Assets/Branding/StudioSplash/Profiles");
        Directory.CreateDirectory("Assets/Branding/StudioSplash/Audio");
        Directory.CreateDirectory("Assets/Branding/StudioSplash/Materials");
        Directory.CreateDirectory("Assets/Script/Startup");
    }

private static void CopyBootLogosIntoAssets()
    {
        CopyLogoIntoAssets(PrimaryLogoSourcePath, PrimaryLogoAssetPath);
        CopyLogoIntoAssets(SecondaryLogoSourcePath, SecondaryLogoAssetPath);
    }

private static void CopyLogoIntoAssets(string sourcePath, string assetPath)
    {
        if (!IsReadableLogoImage(sourcePath))
        {
            Debug.LogWarning($"Studio Splash logo source was not found or is not readable: {sourcePath}");
            return;
        }

        File.Copy(sourcePath, assetPath, true);
    }

private static void ConfigureLogoImporter(string assetPath)
    {
        if (!IsReadableLogoImage(assetPath))
        {
            return;
        }

        TextureImporter importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
        if (importer == null)
        {
            return;
        }

        bool changed = false;
        if (importer.textureType != TextureImporterType.Sprite)
        {
            importer.textureType = TextureImporterType.Sprite;
            changed = true;
        }

        if (importer.spriteImportMode != SpriteImportMode.Single)
        {
            importer.spriteImportMode = SpriteImportMode.Single;
            changed = true;
        }

        if (!importer.alphaIsTransparency)
        {
            importer.alphaIsTransparency = true;
            changed = true;
        }

        if (importer.mipmapEnabled)
        {
            importer.mipmapEnabled = false;
            changed = true;
        }

        if (changed)
        {
            importer.SaveAndReimport();
        }
    }

private static StudioSplashProfile CreateOrUpdateProfile()
    {
        StudioSplashProfile profile = AssetDatabase.LoadAssetAtPath<StudioSplashProfile>(ProfileAssetPath);
        if (profile == null)
        {
            profile = ScriptableObject.CreateInstance<StudioSplashProfile>();
            profile.nextSceneName = "MainMenu";
            profile.showEveryLaunch = true;
            profile.minimumDuration = 4.2f;
            profile.skippableAfterSeconds = 1f;
            profile.fadeInSeconds = 0.6f;
            profile.holdSeconds = 2.2f;
            profile.fadeOutSeconds = 0.7f;
            profile.backgroundColor = BackgroundColor;
            profile.accentColor = AccentColor;
            AssetDatabase.CreateAsset(profile, ProfileAssetPath);
        }

        profile.logo = IsReadableLogoImage(PrimaryLogoAssetPath)
            ? AssetDatabase.LoadAssetAtPath<Sprite>(PrimaryLogoAssetPath)
            : null;

        AudioClip defaultStinger = AssetDatabase.LoadAssetAtPath<AudioClip>(DefaultStingerPath);
        if (defaultStinger != null)
        {
            profile.stinger = defaultStinger;
        }

        EditorUtility.SetDirty(profile);
        return profile;
    }

private static void RebuildBootScene(Scene scene, StudioSplashProfile profile)
    {
        foreach (GameObject root in scene.GetRootGameObjects())
        {
            UnityEngine.Object.DestroyImmediate(root);
        }

        GameObject cameraObject = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener));
        cameraObject.tag = "MainCamera";
        cameraObject.transform.position = new Vector3(0f, 0f, -10f);
        Camera camera = cameraObject.GetComponent<Camera>();
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = BackgroundColor;
        camera.orthographic = true;

        GameObject lightObject = new GameObject("Directional Light", typeof(Light));
        lightObject.transform.rotation = Quaternion.Euler(45f, -30f, 0f);
        Light light = lightObject.GetComponent<Light>();
        light.type = LightType.Directional;
        light.intensity = 0.65f;

        GameObject eventSystemObject = new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
        eventSystemObject.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);

        GameObject bootRoot = new GameObject("BootFlowRoot", typeof(AudioSource));
        AudioSource audioSource = bootRoot.GetComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.volume = 0.78f;
        audioSource.spatialBlend = 0f;

        GameObject canvasObject = new GameObject(
            "StudioSplashCanvas",
            typeof(Canvas),
            typeof(CanvasScaler),
            typeof(GraphicRaycaster),
            typeof(StudioSplashController));
        SetUiLayer(canvasObject);

        Canvas canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceCamera;
        canvas.worldCamera = camera;
        canvas.planeDistance = 1f;
        canvas.sortingOrder = 1000;

        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        TMP_FontAsset font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FontAssetPath);
        Sprite primaryLogo = AssetDatabase.LoadAssetAtPath<Sprite>(PrimaryLogoAssetPath);
        Sprite secondaryLogo = AssetDatabase.LoadAssetAtPath<Sprite>(SecondaryLogoAssetPath);
        bool hasLogo = primaryLogo != null;
        bool hasSecondaryLogo = secondaryLogo != null;
        AudioClip bootChime = AssetDatabase.LoadAssetAtPath<AudioClip>(DefaultStingerPath);
        AudioClip scanSfx = AssetDatabase.LoadAssetAtPath<AudioClip>(ScanSfxPath);
        AudioClip tickSfx = AssetDatabase.LoadAssetAtPath<AudioClip>(ProgressTickSfxPath);

        Image background = CreateImage("Background", canvasObject.transform, BackgroundColor);
        Stretch(background.rectTransform, 0f, 0f, 0f, 0f);

        Image backgroundGlow = CreateImage("BackgroundGlow", canvasObject.transform, new Color(AccentColor.r, AccentColor.g, AccentColor.b, 0.006f));
        RectTransform glowRect = backgroundGlow.rectTransform;
        glowRect.anchorMin = new Vector2(0.5f, 0.5f);
        glowRect.anchorMax = new Vector2(0.5f, 0.5f);
        glowRect.pivot = new Vector2(0.5f, 0.5f);
        glowRect.anchoredPosition = new Vector2(0f, 80f);
        glowRect.sizeDelta = new Vector2(820f, 300f);

        for (int i = 0; i < 9; i++)
        {
            Image line = CreateImage("ScienceGridVertical_" + i, canvasObject.transform, new Color(AccentColor.r, AccentColor.g, AccentColor.b, 0.035f));
            RectTransform rect = line.rectTransform;
            float x = 0.1f + i * 0.1f;
            rect.anchorMin = new Vector2(x, 0.14f);
            rect.anchorMax = new Vector2(x, 0.86f);
            rect.sizeDelta = new Vector2(1.4f, 0f);
            rect.anchoredPosition = Vector2.zero;
        }

        for (int i = 0; i < 5; i++)
        {
            Image line = CreateImage("ScienceGridHorizontal_" + i, canvasObject.transform, new Color(AccentColor.r, AccentColor.g, AccentColor.b, 0.03f));
            RectTransform rect = line.rectTransform;
            float y = 0.22f + i * 0.13f;
            rect.anchorMin = new Vector2(0.12f, y);
            rect.anchorMax = new Vector2(0.88f, y);
            rect.sizeDelta = new Vector2(0f, 1.4f);
            rect.anchoredPosition = Vector2.zero;
        }

        List<RectTransform> particles = new List<RectTransform>();
        for (int i = 0; i < 18; i++)
        {
            Image particle = CreateImage("BootParticle_" + i, canvasObject.transform, new Color(AccentColor.r, AccentColor.g, AccentColor.b, 0.08f));
            RectTransform rect = particle.rectTransform;
            float x = 0.14f + ((i * 37) % 72) / 100f;
            float y = 0.18f + ((i * 29) % 62) / 100f;
            rect.anchorMin = new Vector2(x, y);
            rect.anchorMax = new Vector2(x, y);
            rect.pivot = new Vector2(0.5f, 0.5f);
            float size = 4f + (i % 4) * 2f;
            rect.sizeDelta = new Vector2(size, size);
            rect.anchoredPosition = Vector2.zero;
            particles.Add(rect);
        }

        Image vignette = CreateImage("SoftVignette", canvasObject.transform, new Color(0f, 0f, 0f, 0.30f));
        Stretch(vignette.rectTransform, 0f, 0f, 0f, 0f);

        Image primaryScan = CreateImage("PrimaryScanLine", canvasObject.transform, new Color(AccentColor.r, AccentColor.g, AccentColor.b, 0.14f));
        RectTransform primaryScanRect = primaryScan.rectTransform;
        primaryScanRect.anchorMin = new Vector2(-0.004f, 0.14f);
        primaryScanRect.anchorMax = new Vector2(0.004f, 0.86f);
        primaryScanRect.offsetMin = Vector2.zero;
        primaryScanRect.offsetMax = Vector2.zero;

        Image scanLineA = CreateImage("SecondaryScanLineA", canvasObject.transform, new Color(AccentColor.r, AccentColor.g, AccentColor.b, 0.14f));
        RectTransform scanLineARect = scanLineA.rectTransform;
        scanLineARect.anchorMin = new Vector2(-0.004f, 0.20f);
        scanLineARect.anchorMax = new Vector2(0.004f, 0.80f);
        scanLineARect.offsetMin = Vector2.zero;
        scanLineARect.offsetMax = Vector2.zero;

        Image scanLineB = CreateImage("SecondaryScanLineB", canvasObject.transform, new Color(AccentColor.r, AccentColor.g, AccentColor.b, 0.09f));
        RectTransform scanLineBRect = scanLineB.rectTransform;
        scanLineBRect.anchorMin = new Vector2(-0.004f, 0.28f);
        scanLineBRect.anchorMax = new Vector2(0.004f, 0.72f);
        scanLineBRect.offsetMin = Vector2.zero;
        scanLineBRect.offsetMax = Vector2.zero;

        GameObject logoGroupObject = CreateUiObject("LogoGroup", canvasObject.transform, typeof(CanvasGroup));
        RectTransform logoGroupRect = logoGroupObject.GetComponent<RectTransform>();
        logoGroupRect.anchorMin = new Vector2(0.5f, 0.5f);
        logoGroupRect.anchorMax = new Vector2(0.5f, 0.5f);
        logoGroupRect.pivot = new Vector2(0.5f, 0.5f);
        logoGroupRect.anchoredPosition = new Vector2(0f, 92f);
        logoGroupRect.sizeDelta = new Vector2(820f, 430f);

        Image logoHalo = CreateImage("LogoHalo", logoGroupObject.transform, new Color(AccentColor.r, AccentColor.g, AccentColor.b, 0.04f));
        RectTransform haloRect = logoHalo.rectTransform;
        haloRect.anchorMin = new Vector2(0.5f, 0.5f);
        haloRect.anchorMax = new Vector2(0.5f, 0.5f);
        haloRect.pivot = new Vector2(0.5f, 0.5f);
        haloRect.anchoredPosition = new Vector2(0f, 50f);
        haloRect.sizeDelta = new Vector2(770f, 340f);

        Image logoClarityPlate = CreateImage("LogoClarityPlate", logoGroupObject.transform, new Color(0.72f, 1f, 0.94f, 0.075f));
        RectTransform clarityRect = logoClarityPlate.rectTransform;
        clarityRect.anchorMin = new Vector2(0.5f, 0.5f);
        clarityRect.anchorMax = new Vector2(0.5f, 0.5f);
        clarityRect.pivot = new Vector2(0.5f, 0.5f);
        clarityRect.anchoredPosition = new Vector2(0f, 50f);
        clarityRect.sizeDelta = new Vector2(730f, 310f);

        bool hasAnyLogo = hasLogo || hasSecondaryLogo;

Image logoImage = CreateImage("PrimaryLogoImage", logoGroupObject.transform, Color.white);

        logoImage.sprite = primaryLogo;
        logoImage.preserveAspect = true;
        logoImage.raycastTarget = false;
        logoImage.enabled = hasLogo;
        logoImage.color = hasLogo ? new Color(1.30f, 1.30f, 1.30f, 1f) : new Color(1f, 1f, 1f, 0f);

        RectTransform logoRect = logoImage.rectTransform;
        logoRect.anchorMin = new Vector2(0.5f, 0.5f);
        logoRect.anchorMax = new Vector2(0.5f, 0.5f);
        logoRect.pivot = new Vector2(0.5f, 0.5f);
        logoRect.anchoredPosition = new Vector2(0f, 140f);
        logoRect.sizeDelta = hasLogo ? new Vector2(118f, 118f) : Vector2.zero;

Image secondaryLogoImage = CreateImage("SecondaryLogoImage", logoGroupObject.transform, Color.white);

        secondaryLogoImage.sprite = secondaryLogo;
        secondaryLogoImage.preserveAspect = true;
        secondaryLogoImage.raycastTarget = false;
        secondaryLogoImage.enabled = hasSecondaryLogo;
        secondaryLogoImage.color = hasSecondaryLogo ? new Color(1.35f, 1.35f, 1.35f, 1f) : new Color(1f, 1f, 1f, 0f);

        RectTransform secondaryLogoRect = secondaryLogoImage.rectTransform;
        secondaryLogoRect.anchorMin = new Vector2(0.5f, 0.5f);
        secondaryLogoRect.anchorMax = new Vector2(0.5f, 0.5f);
        secondaryLogoRect.pivot = new Vector2(0.5f, 0.5f);
        secondaryLogoRect.anchoredPosition = new Vector2(0f, 0f);
        secondaryLogoRect.sizeDelta = hasSecondaryLogo ? new Vector2(550f, 112f) : Vector2.zero;

        TextMeshProUGUI studioName = CreateText("StudioName", logoGroupObject.transform, hasAnyLogo ? string.Empty : "USM", hasAnyLogo ? 1f : 82f, FontStyles.Bold, TextAlignmentOptions.Center, font);
        RectTransform studioNameRect = studioName.rectTransform;
        studioNameRect.anchorMin = new Vector2(0f, 0.5f);
        studioNameRect.anchorMax = new Vector2(1f, 0.5f);
        studioNameRect.anchoredPosition = hasAnyLogo ? new Vector2(0f, -70f) : new Vector2(0f, 74f);
        studioNameRect.sizeDelta = hasAnyLogo ? Vector2.zero : new Vector2(0f, 90f);

        TextMeshProUGUI presents = CreateText("PresentsText", logoGroupObject.transform, "presents", 21f, FontStyles.Normal, TextAlignmentOptions.Center, font);
        presents.color = new Color(0.72f, 0.92f, 0.96f, 0.82f);
        RectTransform presentsRect = presents.rectTransform;
        presentsRect.anchorMin = new Vector2(0f, 0.5f);
        presentsRect.anchorMax = new Vector2(1f, 0.5f);
        presentsRect.anchoredPosition = hasAnyLogo ? new Vector2(0f, -132f) : new Vector2(0f, -20f);
        presentsRect.sizeDelta = new Vector2(0f, 38f);

        TextMeshProUGUI productTitle = CreateText("ProductTitle", logoGroupObject.transform, "E-LAB PRIME", 34f, FontStyles.Bold, TextAlignmentOptions.Center, font);
        productTitle.color = new Color(0.86f, 0.98f, 1f, 0.94f);
        RectTransform productTitleRect = productTitle.rectTransform;
        productTitleRect.anchorMin = new Vector2(0f, 0.5f);
        productTitleRect.anchorMax = new Vector2(1f, 0.5f);
        productTitleRect.anchoredPosition = hasAnyLogo ? new Vector2(0f, -174f) : new Vector2(0f, -62f);
        productTitleRect.sizeDelta = new Vector2(0f, 54f);

        Image logoSheen = CreateImage("LogoSheen", logoGroupObject.transform, new Color(1f, 1f, 1f, 0f));
        RectTransform logoSheenRect = logoSheen.rectTransform;
        logoSheenRect.anchorMin = new Vector2(-0.12f, 0.24f);
        logoSheenRect.anchorMax = new Vector2(-0.06f, 0.86f);
        logoSheenRect.offsetMin = Vector2.zero;
        logoSheenRect.offsetMax = Vector2.zero;

        GameObject progressRoot = CreateUiObject("SegmentedProgressRoot", canvasObject.transform);
        RectTransform progressRootRect = progressRoot.GetComponent<RectTransform>();
        progressRootRect.anchorMin = new Vector2(0.5f, 0.205f);
        progressRootRect.anchorMax = new Vector2(0.5f, 0.205f);
        progressRootRect.pivot = new Vector2(0.5f, 0.5f);
        progressRootRect.anchoredPosition = Vector2.zero;
        progressRootRect.sizeDelta = new Vector2(560f, 24f);

        List<Image> progressSegments = new List<Image>();
        int segmentCount = 12;
        float segmentWidth = 38f;
        float gap = 8f;
        float startX = -((segmentCount * segmentWidth) + ((segmentCount - 1) * gap)) * 0.5f + (segmentWidth * 0.5f);
        for (int i = 0; i < segmentCount; i++)
        {
            Image segment = CreateImage("ProgressSegment_" + i, progressRoot.transform, new Color(AccentColor.r, AccentColor.g, AccentColor.b, 0.16f));
            RectTransform rect = segment.rectTransform;
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = new Vector2(startX + i * (segmentWidth + gap), 0f);
            rect.sizeDelta = new Vector2(segmentWidth, 5f);
            progressSegments.Add(segment);
        }

        Image progressTrack = CreateImage("ProgressTrack", canvasObject.transform, new Color(1f, 1f, 1f, 0.055f));
        RectTransform progressTrackRect = progressTrack.rectTransform;
        progressTrackRect.anchorMin = new Vector2(0.5f, 0.205f);
        progressTrackRect.anchorMax = new Vector2(0.5f, 0.205f);
        progressTrackRect.pivot = new Vector2(0.5f, 0.5f);
        progressTrackRect.anchoredPosition = new Vector2(0f, -1f);
        progressTrackRect.sizeDelta = new Vector2(560f, 2f);

        Image progressFill = CreateImage("ProgressFill", progressTrack.transform, AccentColor);
        RectTransform progressFillRect = progressFill.rectTransform;
        progressFillRect.anchorMin = Vector2.zero;
        progressFillRect.anchorMax = Vector2.one;
        progressFillRect.offsetMin = Vector2.zero;
        progressFillRect.offsetMax = Vector2.zero;
        progressFill.type = Image.Type.Filled;
        progressFill.fillMethod = Image.FillMethod.Horizontal;
        progressFill.fillAmount = 0f;
        progressFill.color = new Color(AccentColor.r, AccentColor.g, AccentColor.b, 0.25f);

        TextMeshProUGUI loadingText = CreateText("LoadingText", canvasObject.transform, "Initializing lab systems...", 20f, FontStyles.Normal, TextAlignmentOptions.Center, font);
        loadingText.color = new Color(0.76f, 0.9f, 0.95f, 0.86f);
        RectTransform loadingRect = loadingText.rectTransform;
        loadingRect.anchorMin = new Vector2(0.5f, 0.165f);
        loadingRect.anchorMax = new Vector2(0.5f, 0.165f);
        loadingRect.pivot = new Vector2(0.5f, 0.5f);
        loadingRect.anchoredPosition = Vector2.zero;
        loadingRect.sizeDelta = new Vector2(620f, 34f);

        TextMeshProUGUI skipHint = CreateText("SkipHint", canvasObject.transform, "Press any key to skip", 18f, FontStyles.Normal, TextAlignmentOptions.Right, font);
        skipHint.color = new Color(0.78f, 0.92f, 0.96f, 0f);
        RectTransform skipRect = skipHint.rectTransform;
        skipRect.anchorMin = new Vector2(1f, 0f);
        skipRect.anchorMax = new Vector2(1f, 0f);
        skipRect.pivot = new Vector2(1f, 0f);
        skipRect.anchoredPosition = new Vector2(-44f, 34f);
        skipRect.sizeDelta = new Vector2(360f, 36f);

        GameObject fadeCurtainObject = CreateUiObject("FadeCurtain", canvasObject.transform, typeof(Image), typeof(CanvasGroup));
        Image fadeImage = fadeCurtainObject.GetComponent<Image>();
        fadeImage.color = Color.black;
        fadeImage.raycastTarget = false;
        Stretch(fadeCurtainObject.GetComponent<RectTransform>(), 0f, 0f, 0f, 0f);
        CanvasGroup fadeCanvasGroup = fadeCurtainObject.GetComponent<CanvasGroup>();
        fadeCanvasGroup.alpha = 0f;
        fadeCanvasGroup.interactable = false;
        fadeCanvasGroup.blocksRaycasts = false;

        StudioSplashController controller = canvasObject.GetComponent<StudioSplashController>();
        controller.profile = profile;
        controller.fadeCurtain = fadeCurtainObject.GetComponent<CanvasGroup>();
        controller.logoGroup = logoGroupObject.GetComponent<CanvasGroup>();
        controller.backgroundImage = background;
        controller.logoImage = logoImage;
        controller.lightSweep = primaryScanRect;
        controller.logoSheen = logoSheenRect;
        controller.scanLineA = scanLineARect;
        controller.scanLineB = scanLineBRect;
        controller.backgroundParticles = particles.ToArray();
        controller.progressFill = progressFill;
        controller.progressSegments = progressSegments.ToArray();
        controller.studioNameText = studioName;
        controller.presentsText = presents;
        controller.skipHintText = skipHint;
        controller.loadingText = loadingText;
        controller.audioSource = audioSource;
        controller.bootChime = bootChime;
        controller.scanSfx = scanSfx;
        controller.progressTickSfx = tickSfx;
        controller.statusMessages = new[]
        {
            "Initializing lab systems...",
            "Loading experiment modules...",
            "Preparing student interface...",
            "Calibrating virtual instruments...",
            "Launching E-Lab Prime..."
        };

        EditorSceneManager.MarkSceneDirty(scene);
    }

    private static GameObject CreateUiObject(string name, Transform parent, params Type[] components)
    {
        Type[] types = new Type[components.Length + 1];
        types[0] = typeof(RectTransform);
        Array.Copy(components, 0, types, 1, components.Length);

        GameObject obj = new GameObject(name, types);
        obj.transform.SetParent(parent, false);
        SetUiLayer(obj);
        return obj;
    }

    private static Image CreateImage(string name, Transform parent, Color color)
    {
        GameObject obj = CreateUiObject(name, parent, typeof(Image));
        Image image = obj.GetComponent<Image>();
        image.color = color;
        image.raycastTarget = false;
        return image;
    }

    private static TextMeshProUGUI CreateText(
        string name,
        Transform parent,
        string text,
        float size,
        FontStyles style,
        TextAlignmentOptions alignment,
        TMP_FontAsset font)
    {
        GameObject obj = CreateUiObject(name, parent, typeof(TextMeshProUGUI));
        TextMeshProUGUI label = obj.GetComponent<TextMeshProUGUI>();
        label.text = text;
        label.font = font;
        label.fontSize = size;
        label.fontStyle = style;
        label.alignment = alignment;
        label.color = SoftWhite;
        label.raycastTarget = false;
        label.textWrappingMode = TextWrappingModes.NoWrap;
        return label;
    }

    private static void Stretch(RectTransform rect, float left, float right, float top, float bottom)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = new Vector2(left, bottom);
        rect.offsetMax = new Vector2(-right, -top);
    }

    private static void SetUiLayer(GameObject obj)
    {
        int uiLayer = LayerMask.NameToLayer("UI");
        if (uiLayer >= 0)
        {
            obj.layer = uiLayer;
        }
    }

    private static void UpdateBuildSettings()
    {
        string[] desiredScenes =
        {
            BootScenePath,
            "Assets/Scenes/MainMenu.unity",
            "Assets/Scenes/Login.unity",
            "Assets/Scenes/Lab1.unity",
            "Assets/Scenes/Lab1_3D.unity",
            "Assets/Scenes/Lab2.unity",
            "Assets/Scenes/Lab2_3D.unity",
            "Assets/Scenes/Lab3.unity",
            "Assets/Scenes/Lab3_3D.unity",
            "Assets/Scenes/Lab4_0.unity",
            "Assets/Scenes/Lab4_3D.unity",
            "Assets/Scenes/Lab5.unity",
            "Assets/Scenes/Lab5_0.unity",
            "Assets/Scenes/Lab5_3D.unity",
            "Assets/Scenes/Lab6.unity",
            "Assets/Scenes/Lab6_0.unity",
            "Assets/Scenes/Lab7.unity",
            "Assets/Scenes/Lab8.unity"
        };

        List<EditorBuildSettingsScene> scenes = new List<EditorBuildSettingsScene>();
        foreach (string scenePath in desiredScenes)
        {
            if (File.Exists(scenePath))
            {
                scenes.Add(new EditorBuildSettingsScene(scenePath, true));
            }
        }

        EditorBuildSettings.scenes = scenes.ToArray();
    }


private static bool IsReadableLogoImage(string path)
    {
        if (!File.Exists(path))
        {
            return false;
        }

        byte[] signature = File.ReadAllBytes(path);
        bool isPng = signature.Length >= 8
            && signature[0] == 0x89
            && signature[1] == 0x50
            && signature[2] == 0x4E
            && signature[3] == 0x47
            && signature[4] == 0x0D
            && signature[5] == 0x0A
            && signature[6] == 0x1A
            && signature[7] == 0x0A;

        bool isJpeg = signature.Length >= 3
            && signature[0] == 0xFF
            && signature[1] == 0xD8
            && signature[2] == 0xFF;

        return isPng || isJpeg;
    }
}
