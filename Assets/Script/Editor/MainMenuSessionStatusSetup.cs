using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public static class MainMenuSessionStatusSetup
{
    private static readonly Color StripColor = new Color(0.08f, 0.12f, 0.16f, 0.72f);
    private static readonly Color SlotColor = new Color(0f, 0f, 0f, 0.32f);
    private static readonly Color TealColor = new Color(0.18f, 0.77f, 0.71f, 1f);
    private static readonly Color SoftWhite = new Color(1f, 1f, 1f, 0.9f);

    [MenuItem("E-Lab/Setup/Add Main Menu Session Status")]
    public static void AddSessionStatus()
    {
        var scene = UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene();
        if (scene.name != "MainMenu")
        {
            Debug.LogError("Please open the MainMenu scene first.");
            return;
        }

        Canvas canvas = FindCanvasInScene(scene);
        if (canvas == null)
        {
            Debug.LogError("No Canvas found in MainMenu.");
            return;
        }

        Transform panelLogin = FindChildRecursive(canvas.transform, "PanelLogin");
        Transform accountButton = FindChildRecursive(canvas.transform, "AccountNavButton");

        if (panelLogin == null)
        {
            Debug.LogError("Missing PanelLogin in MainMenu.");
            return;
        }

        Undo.RegisterCompleteObjectUndo(canvas.gameObject, "Add Main Menu Session Status");

        Transform oldStrip = panelLogin.Find("SessionStatusStrip");
        if (oldStrip != null)
        {
            Object.DestroyImmediate(oldStrip.gameObject);
        }

        if (accountButton != null)
        {
            Transform oldBadge = accountButton.Find("AccountStatusBadge");
            if (oldBadge != null)
            {
                Object.DestroyImmediate(oldBadge.gameObject);
            }
        }

        Sprite glassSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/UI Flat/pb_rect_rounded.png");
        Sprite accountIcon = LoadSpriteAsset("Assets/UI Flat/vlab_account_icon.png");
        TMP_FontAsset font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Fonts/FredokaOne SDF.asset");

        GameObject strip = CreateUIObject("SessionStatusStrip", panelLogin, typeof(Image), typeof(Button), typeof(Shadow), typeof(MainMenuSessionStatusUI));
        RectTransform stripRect = strip.GetComponent<RectTransform>();
        stripRect.anchorMin = new Vector2(0.5f, 0.5f);
        stripRect.anchorMax = new Vector2(0.5f, 0.5f);
        stripRect.pivot = new Vector2(0.5f, 0.5f);
        stripRect.sizeDelta = new Vector2(900f, 86f);
        stripRect.anchoredPosition = new Vector2(0f, 122f);
        
        Button stripButton = strip.GetComponent<Button>();
        stripButton.targetGraphic = strip.GetComponent<Image>();
        stripButton.transition = Selectable.Transition.ColorTint;
ApplyGlassImage(strip.GetComponent<Image>(), glassSprite, StripColor);

        Shadow stripShadow = strip.GetComponent<Shadow>();
        stripShadow.effectColor = new Color(0f, 0f, 0f, 0.28f);
        stripShadow.effectDistance = new Vector2(3f, -5f);

        GameObject accent = CreateUIObject("StatusAccent", strip.transform, typeof(Image));
        RectTransform accentRect = accent.GetComponent<RectTransform>();
        accentRect.anchorMin = new Vector2(0f, 0f);
        accentRect.anchorMax = new Vector2(0f, 1f);
        accentRect.pivot = new Vector2(0f, 0.5f);
        accentRect.sizeDelta = new Vector2(8f, 0f);
        accentRect.anchoredPosition = Vector2.zero;
        Image accentImage = accent.GetComponent<Image>();
        accentImage.color = TealColor;
        accentImage.raycastTarget = false;

        GameObject iconObject = CreateUIObject("StatusIcon", strip.transform, typeof(Image));
        RectTransform iconRect = iconObject.GetComponent<RectTransform>();
        iconRect.anchorMin = new Vector2(0f, 0.5f);
        iconRect.anchorMax = new Vector2(0f, 0.5f);
        iconRect.pivot = new Vector2(0.5f, 0.5f);
        iconRect.sizeDelta = new Vector2(58f, 58f);
        iconRect.anchoredPosition = new Vector2(58f, 0f);
        Image iconImage = iconObject.GetComponent<Image>();
        iconImage.sprite = accountIcon;
        iconImage.preserveAspect = true;
        iconImage.raycastTarget = false;

        TextMeshProUGUI title = CreateText(strip.transform, "StatusTitleText", "AKSES PELAJAR BELUM AKTIF", 19f, TextAlignmentOptions.Left, font);
        title.fontStyle = FontStyles.Bold;
        title.color = Color.white;
        SetRect(title.GetComponent<RectTransform>(), new Vector2(0f, 0.5f), new Vector2(1f, 0.5f), new Vector2(0f, 0.5f), new Vector2(106f, 12f), new Vector2(-270f, 30f));

        TextMeshProUGUI subtitle = CreateText(strip.transform, "StatusSubtitleText", "Log masuk untuk simpan progress pelajar", 15f, TextAlignmentOptions.Left, font);
        subtitle.color = SoftWhite;
        SetRect(subtitle.GetComponent<RectTransform>(), new Vector2(0f, 0.5f), new Vector2(1f, 0.5f), new Vector2(0f, 0.5f), new Vector2(106f, -18f), new Vector2(-270f, 26f));

        GameObject pill = CreateUIObject("StatusPill", strip.transform, typeof(Image), typeof(Button));
        RectTransform pillRect = pill.GetComponent<RectTransform>();
        pillRect.anchorMin = new Vector2(1f, 0.5f);
        pillRect.anchorMax = new Vector2(1f, 0.5f);
        pillRect.pivot = new Vector2(1f, 0.5f);
        pillRect.sizeDelta = new Vector2(150f, 40f);
        pillRect.anchoredPosition = new Vector2(-32f, 0f);
        
        Button pillButton = pill.GetComponent<Button>();
        pillButton.targetGraphic = pill.GetComponent<Image>();
        pillButton.transition = Selectable.Transition.ColorTint;
ApplyGlassImage(pill.GetComponent<Image>(), glassSprite, TealColor);

        TextMeshProUGUI pillText = CreateText(pill.transform, "StatusPillText", "SIGN IN", 15f, TextAlignmentOptions.Center, font);
        pillText.fontStyle = FontStyles.Bold;
        Stretch(pillText.GetComponent<RectTransform>(), 8f, 3f, 8f, 3f);

        GameObject sideBadge = null;
        TextMeshProUGUI sideBadgeText = null;
        Image sideBadgeImage = null;
        if (accountButton != null)
        {
            sideBadge = CreateUIObject("AccountStatusBadge", accountButton, typeof(Image));
            RectTransform badgeRect = sideBadge.GetComponent<RectTransform>();
            badgeRect.anchorMin = new Vector2(0.5f, 0f);
            badgeRect.anchorMax = new Vector2(0.5f, 0f);
            badgeRect.pivot = new Vector2(0.5f, 0.5f);
            badgeRect.sizeDelta = new Vector2(96f, 32f);
            badgeRect.anchoredPosition = new Vector2(0f, -12f);
            ApplyGlassImage(sideBadge.GetComponent<Image>(), glassSprite, TealColor);
            sideBadgeImage = sideBadge.GetComponent<Image>();

            sideBadgeText = CreateText(sideBadge.transform, "Text", "SIGN IN", 12f, TextAlignmentOptions.Center, font);
            sideBadgeText.fontStyle = FontStyles.Bold;
            Stretch(sideBadgeText.GetComponent<RectTransform>(), 6f, 2f, 6f, 2f);
        }

        MainMenuSessionStatusUI statusUI = strip.GetComponent<MainMenuSessionStatusUI>();
        statusUI.statusTitleText = title;
        statusUI.statusSubtitleText = subtitle;
        statusUI.statusPillText = pillText;
        statusUI.statusPillImage = pill.GetComponent<Image>();
        statusUI.statusAccentImage = accentImage;
        statusUI.sideBadgeRoot = sideBadge;
        statusUI.sideBadgeText = sideBadgeText;
        
        statusUI.mainMenuManager = Object.FindFirstObjectByType<MainMenuManager>(FindObjectsInactive.Include);
        statusUI.statusStripButton = stripButton;
        statusUI.statusPillButton = pillButton;
statusUI.sideBadgeImage = sideBadgeImage;
        statusUI.Refresh(false);

        MainMenuAccountPanelController accountController = FindSceneAccountPanelController();
        if (accountController != null)
        {
            accountController.sessionStatusUI = statusUI;
            EditorUtility.SetDirty(accountController);
        }

        EditorUtility.SetDirty(canvas.gameObject);
        MainMenuKidFriendlyColourSetup.Apply();
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(scene);
        UnityEditor.SceneManagement.EditorSceneManager.SaveScene(scene);

        Debug.Log("Main Menu session status strip and account badge created successfully.");
    }

    private static GameObject CreateUIObject(string name, Transform parent, params System.Type[] components)
    {
        GameObject obj = new GameObject(name, typeof(RectTransform));
        obj.layer = parent.gameObject.layer;
        foreach (System.Type component in components)
        {
            obj.AddComponent(component);
        }

        obj.transform.SetParent(parent, false);
        return obj;
    }

    private static TextMeshProUGUI CreateText(Transform parent, string name, string text, float size, TextAlignmentOptions alignment, TMP_FontAsset font)
    {
        GameObject textObject = CreateUIObject(name, parent, typeof(TextMeshProUGUI));
        TextMeshProUGUI tmp = textObject.GetComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = size;
        tmp.alignment = alignment;
        tmp.color = Color.white;
        tmp.textWrappingMode = TextWrappingModes.NoWrap;
        tmp.raycastTarget = false;
        if (font != null) tmp.font = font;
        return tmp;
    }

    private static void ApplyGlassImage(Image image, Sprite sprite, Color color)
    {
        if (image == null) return;

        image.color = color;
        if (sprite != null)
        {
            image.sprite = sprite;
            image.type = Image.Type.Sliced;
            image.pixelsPerUnitMultiplier = 2f;
        }
    }

    private static Sprite LoadSpriteAsset(string assetPath)
    {
        TextureImporter importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
        bool needsReimport = false;
        if (importer != null && importer.textureType != TextureImporterType.Sprite)
        {
            importer.textureType = TextureImporterType.Sprite;
            needsReimport = true;
        }

        if (importer != null && importer.spriteImportMode != SpriteImportMode.Single)
        {
            importer.spriteImportMode = SpriteImportMode.Single;
            needsReimport = true;
        }

        if (importer != null)
        {
            importer.alphaIsTransparency = true;
            importer.mipmapEnabled = false;
        }

        if (importer != null && needsReimport)
        {
            importer.SaveAndReimport();
        }

        Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);
        if (sprite != null) return sprite;

        foreach (Object asset in AssetDatabase.LoadAllAssetsAtPath(assetPath))
        {
            if (asset is Sprite importedSprite) return importedSprite;
        }

        return null;
    }

    private static void SetRect(RectTransform rect, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 anchoredPosition, Vector2 sizeDelta)
    {
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.pivot = pivot;
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = sizeDelta;
    }

    private static void Stretch(RectTransform rect, float left, float top, float right, float bottom)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = new Vector2(left, bottom);
        rect.offsetMax = new Vector2(-right, -top);
    }

    private static Transform FindChildRecursive(Transform parent, string name)
    {
        Transform[] children = parent.GetComponentsInChildren<Transform>(true);
        foreach (Transform child in children)
        {
            if (child.name == name) return child;
        }

        return null;
    }

    private static Canvas FindCanvasInScene(UnityEngine.SceneManagement.Scene scene)
    {
        foreach (GameObject root in scene.GetRootGameObjects())
        {
            Canvas canvas = root.GetComponentInChildren<Canvas>(true);
            if (canvas != null) return canvas;
        }

        return null;
    }

    private static MainMenuAccountPanelController FindSceneAccountPanelController()
    {
        MainMenuAccountPanelController[] controllers = Resources.FindObjectsOfTypeAll<MainMenuAccountPanelController>();
        foreach (MainMenuAccountPanelController controller in controllers)
        {
            if (controller != null && controller.gameObject.scene.IsValid())
            {
                return controller;
            }
        }

        return null;
    }

}