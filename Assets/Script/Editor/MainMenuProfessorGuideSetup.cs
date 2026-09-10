using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

public static class MainMenuProfessorGuideSetup
{
    private const string MainMenuScenePath = "Assets/Scenes/MainMenu.unity";
    private const string ProfessorSpritePath = "Assets/Branding/ProfessorGuide/Sprites/professor_idle_v2.png";
    private const string ProfessorTalkSpritePath = "Assets/Branding/ProfessorGuide/Sprites/professor_talk.png";
    private const string ProfessorPointSpritePath = "Assets/Branding/ProfessorGuide/Sprites/professor_point.png";
    private const string ProfessorSmileSpritePath = "Assets/Branding/ProfessorGuide/Sprites/professor_smile.png";
    private const string PanelSpritePath = "Assets/UI Flat/pb_rect_rounded.png";
    private const string FontAssetPath = "Assets/Fonts/FredokaOne SDF.asset";
    private const string AppearSfxPath = "Assets/Branding/ProfessorGuide/Audio/professor_appear.wav";
    private const string TextBlipSfxPath = "Assets/Branding/ProfessorGuide/Audio/professor_text_blip.wav";
    private const string HighlightSfxPath = "Assets/Branding/ProfessorGuide/Audio/professor_highlight.wav";

    private static readonly Color GlassColor = new Color(0.06f, 0.09f, 0.13f, 0.88f);
    private static readonly Color AccentColor = new Color(0.18f, 0.77f, 0.71f, 1f);
    private static readonly Color SoftWhite = new Color(0.92f, 0.97f, 1f, 1f);

    [MenuItem("E-Lab/Setup/Setup Main Menu Professor Guide Phase 1")]
    public static void SetupProfessorGuide()
    {
        if (EditorSceneManager.GetActiveScene().path != MainMenuScenePath)
        {
            EditorSceneManager.SaveOpenScenes();
            EditorSceneManager.OpenScene(MainMenuScenePath);
        }

        Canvas canvas = Object.FindAnyObjectByType<Canvas>(FindObjectsInactive.Include);
        if (canvas == null)
        {
            Debug.LogError("Main Menu Professor Guide setup failed: no Canvas found.");
            return;
        }

        Sprite professorSprite = LoadSpriteAsset(ProfessorSpritePath);
        Sprite professorTalkSprite = LoadSpriteAsset(ProfessorTalkSpritePath);
        Sprite professorPointSprite = LoadSpriteAsset(ProfessorPointSpritePath);
        Sprite professorSmileSprite = LoadSpriteAsset(ProfessorSmileSpritePath);

        Sprite panelSprite = LoadSpriteAsset(PanelSpritePath);
        TMP_FontAsset font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FontAssetPath);
        AudioClip appearSfx = AssetDatabase.LoadAssetAtPath<AudioClip>(AppearSfxPath);
        AudioClip textBlipSfx = AssetDatabase.LoadAssetAtPath<AudioClip>(TextBlipSfxPath);
        AudioClip highlightSfx = AssetDatabase.LoadAssetAtPath<AudioClip>(HighlightSfxPath);


        Transform existing = FindChildRecursive(canvas.transform, "ProfessorGuideRoot");
        if (existing != null)
        {
            Undo.DestroyObjectImmediate(existing.gameObject);
        }

        GameObject root = CreateUiObject("ProfessorGuideRoot", canvas.transform, typeof(CanvasGroup), typeof(MainMenuProfessorGuide), typeof(AudioSource));
        RectTransform rootRect = root.GetComponent<RectTransform>();
        Stretch(rootRect, 0f, 0f, 0f, 0f);
        root.transform.SetAsLastSibling();

        CanvasGroup rootGroup = root.GetComponent<CanvasGroup>();
        rootGroup.alpha = 0f;

        AudioSource audioSource = root.GetComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0f;

        Image professorImage = CreateProfessorImage(root.transform, professorSprite);
        Image dialoguePanel = CreateDialoguePanel(root.transform, panelSprite);
        TextMeshProUGUI nameText = CreateText(dialoguePanel.transform, "ProfessorName", "PROF. HAKIM", 19f, TextAlignmentOptions.Left, font, AccentColor);
        TextMeshProUGUI malayText = CreateText(dialoguePanel.transform, "MalayText", string.Empty, 24f, TextAlignmentOptions.TopLeft, font, SoftWhite);
        TextMeshProUGUI subtitleText = CreateText(dialoguePanel.transform, "EnglishSubtitleText", string.Empty, 16f, TextAlignmentOptions.TopLeft, font, new Color(0.66f, 0.94f, 0.92f, 1f));
        Button continueButton = CreateSmallButton(dialoguePanel.transform, "ContinueButton", "NEXT", panelSprite, font, new Vector2(-80f, 34f));
        Button skipButton = CreateSmallButton(dialoguePanel.transform, "SkipButton", "SKIP", panelSprite, font, new Vector2(-205f, 34f));

        Anchor(nameText.rectTransform, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(28f, -42f), new Vector2(-28f, -12f));
        Anchor(malayText.rectTransform, new Vector2(0f, 0.45f), new Vector2(1f, 1f), new Vector2(28f, -114f), new Vector2(-28f, -50f));
        Anchor(subtitleText.rectTransform, new Vector2(0f, 0f), new Vector2(1f, 0.45f), new Vector2(28f, 68f), new Vector2(-28f, -8f));

        MainMenuProfessorGuide guide = root.GetComponent<MainMenuProfessorGuide>();
        guide.rootGroup = rootGroup;
        guide.professorImage = professorImage;
        guide.nameText = nameText;
        guide.malayText = malayText;
        guide.englishSubtitleText = subtitleText;
        guide.continueButton = continueButton;
        guide.skipButton = skipButton;
        guide.audioSource = audioSource;
        guide.appearSfx = appearSfx;
        guide.textBlipSfx = textBlipSfx;
        guide.highlightSfx = highlightSfx;
        guide.idleSprite = professorSprite;
        guide.talkSprite = professorTalkSprite;
        guide.pointSprite = professorPointSprite;
        guide.smileSprite = professorSmileSprite;
        guide.startupDelaySeconds = 0.35f;


        guide.dialogueLines = CreateDefaultDialogue();

        WireMenuTargets(canvas, guide, panelSprite);

        EditorUtility.SetDirty(canvas.gameObject);
        MainMenuKidFriendlyColourSetup.Apply();
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
        AssetDatabase.SaveAssets();

        Debug.Log("Main Menu Professor Guide Phase 1 setup complete.");
    }

    private static Image CreateProfessorImage(Transform parent, Sprite professorSprite)
    {
        GameObject obj = CreateUiObject("ProfessorAvatar", parent, typeof(Image));
        RectTransform rect = obj.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0f, 0f);
        rect.anchorMax = new Vector2(0f, 0f);
        rect.pivot = new Vector2(0.5f, 0f);
        rect.sizeDelta = new Vector2(330f, 430f);
        rect.anchoredPosition = new Vector2(190f, 36f);

        Image image = obj.GetComponent<Image>();
        image.sprite = professorSprite;
        image.preserveAspect = true;
        image.raycastTarget = false;
        return image;
    }

    private static Image CreateDialoguePanel(Transform parent, Sprite panelSprite)
    {
        GameObject obj = CreateUiObject("ProfessorDialoguePanel", parent, typeof(Image));
        RectTransform rect = obj.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0f, 0f);
        rect.anchorMax = new Vector2(0f, 0f);
        rect.pivot = new Vector2(0f, 0f);
        rect.sizeDelta = new Vector2(590f, 210f);
        rect.anchoredPosition = new Vector2(360f, 60f);

        Image image = obj.GetComponent<Image>();
        image.sprite = panelSprite;
        image.type = Image.Type.Sliced;
        image.color = GlassColor;
        image.pixelsPerUnitMultiplier = 2f;
        return image;
    }

    private static Button CreateSmallButton(Transform parent, string name, string label, Sprite sprite, TMP_FontAsset font, Vector2 anchoredPosition)
    {
        GameObject obj = CreateUiObject(name, parent, typeof(Image), typeof(Button));
        RectTransform rect = obj.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(1f, 0f);
        rect.anchorMax = new Vector2(1f, 0f);
        rect.pivot = new Vector2(1f, 0.5f);
        rect.sizeDelta = new Vector2(104f, 42f);
        rect.anchoredPosition = anchoredPosition;

        Image image = obj.GetComponent<Image>();
        image.sprite = sprite;
        image.type = Image.Type.Sliced;
        image.color = new Color(0f, 0f, 0f, 0.34f);

        Button button = obj.GetComponent<Button>();
        button.targetGraphic = image;
        button.transition = Selectable.Transition.ColorTint;

        TextMeshProUGUI text = CreateText(obj.transform, "Label", label, 15f, TextAlignmentOptions.Center, font, SoftWhite);
        Stretch(text.rectTransform, 0f, 0f, 0f, 0f);
        return button;
    }

    private static void WireMenuTargets(Canvas canvas, MainMenuProfessorGuide guide, Sprite panelSprite)
    {
        Button startButton = FindButtonCalling(canvas, "BukaAmali");
        Button settingsButton = FindButtonCalling(canvas, "BukaPengaturan");
        Button achievementsButton = FindButtonCalling(canvas, "BukaPencapaian");
        Button signInButton = FindSignInButton();

        guide.targets = new List<ProfessorGuideTarget>();
        AddTarget(guide, startButton, "StartExperiments", ProfessorGuideContext.StartExperiments, panelSprite);
        AddTarget(guide, settingsButton, "Settings", ProfessorGuideContext.Settings, panelSprite);
        AddTarget(guide, achievementsButton, "Achievements", ProfessorGuideContext.Achievements, panelSprite);
        AddTarget(guide, signInButton, "SignIn", ProfessorGuideContext.SignedOutPrompt, panelSprite);
    }

    private static void AddTarget(MainMenuProfessorGuide guide, Button button, string key, ProfessorGuideContext context, Sprite panelSprite)
    {
        if (button == null)
        {
            Debug.LogWarning("Professor Guide target not found: " + key);
            return;
        }

        Image highlight = EnsureHighlight(button.transform, panelSprite);
        guide.targets.Add(new ProfessorGuideTarget
        {
            key = key,
            button = button,
            highlight = highlight
        });

        ProfessorGuideHoverPrompt prompt = button.GetComponent<ProfessorGuideHoverPrompt>();
        if (prompt == null)
        {
            prompt = button.gameObject.AddComponent<ProfessorGuideHoverPrompt>();
        }

        prompt.guide = guide;
        prompt.context = context;
        EditorUtility.SetDirty(button.gameObject);
    }

    private static Image EnsureHighlight(Transform buttonTransform, Sprite panelSprite)
    {
        Transform existing = buttonTransform.Find("ProfessorHighlight");
        GameObject obj = existing != null ? existing.gameObject : CreateUiObject("ProfessorHighlight", buttonTransform, typeof(Image));
        obj.transform.SetAsLastSibling();

        RectTransform rect = obj.GetComponent<RectTransform>();
        Stretch(rect, -9f, -9f, -9f, -9f);

        Image image = obj.GetComponent<Image>();
        image.sprite = panelSprite;
        image.type = Image.Type.Sliced;
        image.color = new Color(0.18f, 0.77f, 0.71f, 0.36f);
        image.raycastTarget = false;
        image.gameObject.SetActive(false);
        return image;
    }

private static List<ProfessorDialogueLine> CreateDefaultDialogue()
    {
        return new List<ProfessorDialogueLine>
        {
            new ProfessorDialogueLine
            {
                context = ProfessorGuideContext.FirstLaunchWelcome,
                avatarPose = ProfessorAvatarPose.Smile,
                malayText = "Selamat datang ke Makmal Sains Digital.",
                englishSubtitle = "[Welcome to the Digital Science Lab.]",
                voiceClip = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Branding/ProfessorGuide/Audio/voice_welcome_ms.wav")
            },
            new ProfessorDialogueLine
            {
                context = ProfessorGuideContext.SignedOutPrompt,
                avatarPose = ProfessorAvatarPose.Point,
                malayText = "Log masuk dahulu supaya kemajuan eksperimen anda dapat disimpan.",
                englishSubtitle = "[Sign in first so your experiment progress can be saved.]",
                targetKey = "SignIn",
                voiceClip = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Branding/ProfessorGuide/Audio/voice_signedout_ms.wav")
            },
            new ProfessorDialogueLine
            {
                context = ProfessorGuideContext.SignedInWelcome,
                avatarPose = ProfessorAvatarPose.Smile,
                malayText = "Selamat kembali. Mari sambung penerokaan sains anda.",
                englishSubtitle = "[Welcome back. Let us continue your science exploration.]",
                voiceClip = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Branding/ProfessorGuide/Audio/voice_signedin_ms.wav")
            },
            new ProfessorDialogueLine
            {
                context = ProfessorGuideContext.StartExperiments,
                avatarPose = ProfessorAvatarPose.Point,
                malayText = "Tekan Mulakan Amali untuk memilih eksperimen sains anda.",
                englishSubtitle = "[Press Start Experiments to choose your science activity.]",
                targetKey = "StartExperiments",
                voiceClip = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Branding/ProfessorGuide/Audio/voice_start_ms.wav")
            },
            new ProfessorDialogueLine
            {
                context = ProfessorGuideContext.Settings,
                avatarPose = ProfessorAvatarPose.Talk,
                malayText = "Laraskan bunyi, paparan, dan pengalaman pembelajaran di sini.",
                englishSubtitle = "[Adjust sound, display, and learning experience here.]",
                targetKey = "Settings",
                voiceClip = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Branding/ProfessorGuide/Audio/voice_settings_ms.wav")
            },
            new ProfessorDialogueLine
            {
                context = ProfessorGuideContext.Achievements,
                avatarPose = ProfessorAvatarPose.Smile,
                malayText = "Di sini anda boleh melihat pencapaian dan kemajuan setiap amali.",
                englishSubtitle = "[Here you can view achievements and progress for each experiment.]",
                targetKey = "Achievements",
                voiceClip = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Branding/ProfessorGuide/Audio/voice_achievements_ms.wav")
            }
        };
    }

    private static Button FindSignInButton()
    {
        MainMenuSessionStatusUI statusUi = Object.FindAnyObjectByType<MainMenuSessionStatusUI>(FindObjectsInactive.Include);
        if (statusUi == null)
        {
            return null;
        }

        if (statusUi.statusStripButton != null)
        {
            return statusUi.statusStripButton;
        }

        Button button = statusUi.GetComponent<Button>();
        if (button != null)
        {
            return button;
        }

        return statusUi.statusPillButton;
    }

    private static Button FindButtonCalling(Canvas canvas, string methodName)
    {
        Button[] buttons = canvas.GetComponentsInChildren<Button>(true);
        for (int i = 0; i < buttons.Length; i++)
        {
            Button button = buttons[i];
            int eventCount = button.onClick.GetPersistentEventCount();
            for (int eventIndex = 0; eventIndex < eventCount; eventIndex++)
            {
                if (button.onClick.GetPersistentMethodName(eventIndex) == methodName)
                {
                    return button;
                }
            }
        }

        return null;
    }

    private static TextMeshProUGUI CreateText(Transform parent, string name, string text, float size, TextAlignmentOptions alignment, TMP_FontAsset font, Color color)
    {
        GameObject obj = CreateUiObject(name, parent, typeof(TextMeshProUGUI));
        TextMeshProUGUI tmp = obj.GetComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = size;
        tmp.alignment = alignment;
        tmp.color = color;
        tmp.enableWordWrapping = true;
        tmp.raycastTarget = false;
        if (font != null)
        {
            tmp.font = font;
        }

        return tmp;
    }

private static GameObject CreateUiObject(string name, Transform parent, params System.Type[] components)
    {
        GameObject obj = new GameObject(name, components);
        if (obj.GetComponent<RectTransform>() == null)
        {
            obj.AddComponent<RectTransform>();
        }

        obj.transform.SetParent(parent, false);
        return obj;
    }

    private static Sprite LoadSpriteAsset(string assetPath)
    {
        TextureImporter importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
        if (importer != null)
        {
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

        return AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);
    }

    private static Transform FindChildRecursive(Transform parent, string name)
    {
        Transform[] children = parent.GetComponentsInChildren<Transform>(true);
        for (int i = 0; i < children.Length; i++)
        {
            if (children[i].name == name)
            {
                return children[i];
            }
        }

        return null;
    }

    private static void Stretch(RectTransform rect, float left, float right, float top, float bottom)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = new Vector2(left, bottom);
        rect.offsetMax = new Vector2(-right, -top);
    }

    private static void Anchor(RectTransform rect, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax)
    {
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.offsetMin = offsetMin;
        rect.offsetMax = offsetMax;
    }
}
