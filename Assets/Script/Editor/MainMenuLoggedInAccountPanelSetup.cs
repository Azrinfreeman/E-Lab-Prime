using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

public static class MainMenuLoggedInAccountPanelSetup
{
    private const string MainMenuScenePath = "Assets/Scenes/MainMenu.unity";
    private const string PanelSpritePath = "Assets/UI Flat/pb_rect_rounded.png";
    private const string FontAssetPath = "Assets/Fonts/FredokaOne SDF.asset";

    private static readonly Color CardColor = new Color(0.035f, 0.055f, 0.08f, 0.88f);
    private static readonly Color InnerCardColor = new Color(0f, 0f, 0f, 0.22f);
    private static readonly Color FooterColor = new Color(0.015f, 0.03f, 0.05f, 0.72f);
    private static readonly Color DividerColor = new Color(0.42f, 0.82f, 0.86f, 0.18f);
    private static readonly Color AccentColor = new Color(0.18f, 0.77f, 0.71f, 1f);
    private static readonly Color GuestColor = new Color(1f, 0.78f, 0.25f, 1f);
    private static readonly Color SoftWhite = new Color(0.94f, 0.98f, 1f, 1f);
    private static readonly Color MutedText = new Color(0.68f, 0.86f, 0.9f, 1f);

    [MenuItem("E-Lab Prime/Setup Logged-In Account Panel")]
    public static void SetupLoggedInAccountPanel()
    {
        if (EditorSceneManager.GetActiveScene().path != MainMenuScenePath)
        {
            EditorSceneManager.SaveOpenScenes();
            EditorSceneManager.OpenScene(MainMenuScenePath, OpenSceneMode.Single);
        }

        Canvas canvas = Object.FindAnyObjectByType<Canvas>(FindObjectsInactive.Include);
        if (canvas == null)
        {
            Debug.LogError("Setup Logged-In Account Panel failed: no Canvas found.");
            return;
        }

        Transform panel = FindChildRecursive(canvas.transform, "PanelAkaun");
        if (panel == null)
        {
            Debug.LogError("Setup Logged-In Account Panel failed: PanelAkaun was not found.");
            return;
        }

        MainMenuAccountPanelController controller = panel.GetComponent<MainMenuAccountPanelController>();
        if (controller == null)
        {
            controller = panel.gameObject.AddComponent<MainMenuAccountPanelController>();
        }

        TMP_FontAsset font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FontAssetPath);
        Sprite panelSprite = LoadSpriteAsset(PanelSpritePath);

        Transform tabs = FindChildRecursive(panel, "Tabs");
        Transform loginForm = FindChildRecursive(panel, "LoginForm");
        Transform registerForm = FindChildRecursive(panel, "RegisterForm");
        Transform guestButton = FindChildRecursive(panel, "GuestButton");
        Transform statusText = FindChildRecursive(panel, "StatusText");

        if (tabs != null)
        {
            controller.tabsRoot = tabs.gameObject;
        }

        if (loginForm != null)
        {
            controller.loginForm = loginForm.gameObject;
        }

        if (registerForm != null)
        {
            controller.registerForm = registerForm.gameObject;
        }

        if (guestButton != null)
        {
            controller.guestButton = guestButton.GetComponent<Button>();
        }

        if (statusText != null)
        {
            controller.statusText = statusText.GetComponent<TextMeshProUGUI>();
        }

        GameObject signedInPanel = BuildSignedInPanel(panel, controller, panelSprite, font);
        controller.signedInPanel = signedInPanel;

        MainMenuSessionStatusUI sessionStatus = Object.FindAnyObjectByType<MainMenuSessionStatusUI>(FindObjectsInactive.Include);
        if (sessionStatus != null)
        {
            controller.sessionStatusUI = sessionStatus;
        }

        EditorUtility.SetDirty(controller);
        MainMenuKidFriendlyColourSetup.Apply();
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
        AssetDatabase.SaveAssets();

        Debug.Log("Logged-in account profile card created and wired successfully.");
    }

private static GameObject BuildSignedInPanel(Transform parent, MainMenuAccountPanelController controller, Sprite panelSprite, TMP_FontAsset font)
    {
        Transform existing = FindChildRecursive(parent, "SignedInPanel");
        GameObject root = existing != null ? existing.gameObject : CreateUIObject("SignedInPanel", parent, typeof(RectTransform));

        ClearChildren(root.transform);
        root.SetActive(false);

        RectTransform rootRect = root.GetComponent<RectTransform>();
        rootRect.anchorMin = new Vector2(0f, 1f);
        rootRect.anchorMax = new Vector2(0f, 1f);
        rootRect.pivot = new Vector2(0.5f, 1f);
        rootRect.sizeDelta = new Vector2(980f, 380f);

        LayoutElement rootLayout = GetOrAdd<LayoutElement>(root);
        rootLayout.preferredWidth = 980f;
        rootLayout.preferredHeight = 380f;
        rootLayout.minHeight = 360f;
        rootLayout.flexibleWidth = 0f;
        rootLayout.flexibleHeight = 0f;

        Image background = GetOrAdd<Image>(root);
        background.sprite = panelSprite;
        background.type = Image.Type.Sliced;
        background.color = CardColor;

        VerticalLayoutGroup layout = GetOrAdd<VerticalLayoutGroup>(root);
        layout.padding = new RectOffset(28, 28, 24, 22);
        layout.spacing = 14f;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;

        GameObject header = CreateUIObject("ProfileHeader", root.transform, typeof(HorizontalLayoutGroup), typeof(LayoutElement));
        LayoutElement headerLayout = header.GetComponent<LayoutElement>();
        headerLayout.preferredHeight = 92f;
        HorizontalLayoutGroup headerGroup = header.GetComponent<HorizontalLayoutGroup>();
        headerGroup.spacing = 20f;
        headerGroup.childAlignment = TextAnchor.MiddleLeft;
        headerGroup.childControlWidth = true;
        headerGroup.childControlHeight = true;
        headerGroup.childForceExpandWidth = false;
        headerGroup.childForceExpandHeight = false;

        Image avatar = CreateImage("AvatarCircle", header.transform, panelSprite, new Color(0.18f, 0.77f, 0.71f, 0.96f));
        LayoutElement avatarLayout = avatar.gameObject.AddComponent<LayoutElement>();
        avatarLayout.preferredWidth = 86f;
        avatarLayout.preferredHeight = 86f;
        controller.signedInInitialsText = CreateText(avatar.transform, "InitialsText", "EL", 34f, TextAlignmentOptions.Center, font, Color.white);
        Stretch(controller.signedInInitialsText.rectTransform, 0f, 0f, 0f, 0f);

        GameObject identity = CreateUIObject("IdentityColumn", header.transform, typeof(VerticalLayoutGroup), typeof(LayoutElement));
        LayoutElement identityLayout = identity.GetComponent<LayoutElement>();
        identityLayout.flexibleWidth = 1f;
        identityLayout.preferredHeight = 86f;
        VerticalLayoutGroup identityGroup = identity.GetComponent<VerticalLayoutGroup>();
        identityGroup.spacing = 5f;
        identityGroup.childControlWidth = true;
        identityGroup.childControlHeight = true;
        identityGroup.childForceExpandWidth = true;
        identityGroup.childForceExpandHeight = false;

        controller.signedInNameText = CreateText(identity.transform, "SignedInNameText", "Pelajar E-Lab", 30f, TextAlignmentOptions.Left, font, SoftWhite);
        controller.signedInDetailsText = CreateText(identity.transform, "SignedInDetailsText", "ID: -  |  Kelas: -", 17f, TextAlignmentOptions.Left, font, MutedText);
        TextMeshProUGUI helperText = CreateText(identity.transform, "SignedInHelperText", "Sesi pembelajaran sedia digunakan", 13f, TextAlignmentOptions.Left, font, new Color(0.75f, 0.92f, 0.94f, 0.82f));
        helperText.textWrappingMode = TextWrappingModes.NoWrap;

        Image pill = CreateImage("ModePill", header.transform, panelSprite, AccentColor);
        controller.signedInModePillImage = pill;
        LayoutElement pillLayout = pill.gameObject.AddComponent<LayoutElement>();
        pillLayout.preferredWidth = 184f;
        pillLayout.preferredHeight = 42f;
        controller.signedInModeText = CreateText(pill.transform, "ModePillText", "PROFIL AKTIF", 14f, TextAlignmentOptions.Center, font, Color.white);
        controller.signedInModeText.fontStyle = FontStyles.Bold;
        Stretch(controller.signedInModeText.rectTransform, 0f, 0f, 0f, 0f);

        GameObject summary = CreateUIObject("StatusSummary", root.transform, typeof(Image), typeof(HorizontalLayoutGroup), typeof(LayoutElement));
        Image summaryImage = summary.GetComponent<Image>();
        summaryImage.sprite = panelSprite;
        summaryImage.type = Image.Type.Sliced;
        summaryImage.color = InnerCardColor;
        summary.GetComponent<LayoutElement>().preferredHeight = 112f;

        HorizontalLayoutGroup summaryGroup = summary.GetComponent<HorizontalLayoutGroup>();
        summaryGroup.padding = new RectOffset(18, 18, 14, 14);
        summaryGroup.spacing = 18f;
        summaryGroup.childAlignment = TextAnchor.MiddleLeft;
        summaryGroup.childControlWidth = true;
        summaryGroup.childControlHeight = true;
        summaryGroup.childForceExpandWidth = false;
        summaryGroup.childForceExpandHeight = false;

        controller.signedInSaveStatusText = CreateMetric(summary.transform, "SaveMetric", "SIMPANAN", "Kemajuan amali disimpan.", font, AccentColor);
        CreateDivider(summary.transform);
        controller.signedInAccessText = CreateMetric(summary.transform, "AccessMetric", "AKSES", "Sesi makmal sedia digunakan.", font, new Color(0.52f, 0.72f, 1f, 1f));
        CreateDivider(summary.transform);
        controller.signedInDeviceText = CreateMetric(summary.transform, "DeviceMetric", "PERANTI", "Profil peranti aktif.", font, GuestColor);

        GameObject footer = CreateUIObject("ProfileFooter", root.transform, typeof(Image), typeof(HorizontalLayoutGroup), typeof(LayoutElement));
        Image footerImage = footer.GetComponent<Image>();
        footerImage.sprite = panelSprite;
        footerImage.type = Image.Type.Sliced;
        footerImage.color = FooterColor;
        footer.GetComponent<LayoutElement>().preferredHeight = 68f;

        HorizontalLayoutGroup footerGroup = footer.GetComponent<HorizontalLayoutGroup>();
        footerGroup.padding = new RectOffset(16, 10, 9, 9);
        footerGroup.spacing = 12f;
        footerGroup.childAlignment = TextAnchor.MiddleCenter;
        footerGroup.childControlWidth = true;
        footerGroup.childControlHeight = true;
        footerGroup.childForceExpandWidth = false;
        footerGroup.childForceExpandHeight = false;

        Image statusDot = CreateImage("SaveStatusDot", footer.transform, panelSprite, AccentColor);
        LayoutElement dotLayout = statusDot.gameObject.AddComponent<LayoutElement>();
        dotLayout.preferredWidth = 10f;
        dotLayout.preferredHeight = 10f;

        TextMeshProUGUI note = CreateText(footer.transform, "ProfileNoteText", "Profil aktif. Kemajuan disimpan secara automatik.", 15f, TextAlignmentOptions.MidlineLeft, font, MutedText);
        note.textWrappingMode = TextWrappingModes.Normal;
        LayoutElement noteLayout = note.gameObject.AddComponent<LayoutElement>();
        noteLayout.flexibleWidth = 1f;
        noteLayout.preferredHeight = 48f;

        Button logoutButton = CreateButton(footer.transform, "LogoutButton", "LOG KELUAR", panelSprite, font, new Color(0.95f, 0.28f, 0.28f, 0.94f));
        LayoutElement logoutLayout = logoutButton.gameObject.AddComponent<LayoutElement>();
        logoutLayout.preferredWidth = 210f;
        logoutLayout.preferredHeight = 50f;
        controller.logoutButton = logoutButton;
        controller.signedInPrimaryActionText = logoutButton.GetComponentInChildren<TextMeshProUGUI>(true);

        return root;
    }

    private static TextMeshProUGUI CreateMetric(Transform parent, string name, string title, string value, TMP_FontAsset font, Color accent)
    {
        GameObject column = CreateUIObject(name, parent, typeof(VerticalLayoutGroup), typeof(LayoutElement));

        LayoutElement layout = column.GetComponent<LayoutElement>();
        layout.flexibleWidth = 1f;
        layout.preferredHeight = 80f;

        VerticalLayoutGroup group = column.GetComponent<VerticalLayoutGroup>();
        group.spacing = 7f;
        group.childAlignment = TextAnchor.MiddleLeft;
        group.childControlWidth = true;
        group.childControlHeight = true;
        group.childForceExpandWidth = true;
        group.childForceExpandHeight = false;

        TextMeshProUGUI titleText = CreateText(column.transform, name + "Title", title, 13.5f, TextAlignmentOptions.Left, font, accent);
        titleText.fontStyle = FontStyles.Bold;
        LayoutElement titleLayout = titleText.gameObject.AddComponent<LayoutElement>();
        titleLayout.preferredHeight = 20f;

        TextMeshProUGUI valueText = CreateText(column.transform, name + "Value", value, 15f, TextAlignmentOptions.TopLeft, font, SoftWhite);
        valueText.textWrappingMode = TextWrappingModes.Normal;
        valueText.overflowMode = TextOverflowModes.Ellipsis;
        LayoutElement valueLayout = valueText.gameObject.AddComponent<LayoutElement>();
        valueLayout.preferredHeight = 46f;
        return valueText;
    }

    private static void CreateDivider(Transform parent)
    {
        GameObject divider = CreateUIObject("Divider", parent, typeof(Image), typeof(LayoutElement));
        divider.GetComponent<Image>().color = DividerColor;
        LayoutElement layout = divider.GetComponent<LayoutElement>();
        layout.preferredWidth = 1f;
        layout.preferredHeight = 64f;
        layout.flexibleWidth = 0f;
    }
    private static Button CreateButton(Transform parent, string name, string label, Sprite sprite, TMP_FontAsset font, Color color)
    {
        GameObject obj = CreateUIObject(name, parent, typeof(Image), typeof(Button));
        Image image = obj.GetComponent<Image>();
        image.sprite = sprite;
        image.type = Image.Type.Sliced;
        image.color = color;

        Button button = obj.GetComponent<Button>();
        button.targetGraphic = image;

        TextMeshProUGUI text = CreateText(obj.transform, "Text", label, 17f, TextAlignmentOptions.Center, font, Color.white);
        text.fontStyle = FontStyles.Bold;
        Stretch(text.rectTransform, 0f, 0f, 0f, 0f);
        return button;
    }

    private static Image CreateImage(string name, Transform parent, Sprite sprite, Color color)
    {
        GameObject obj = CreateUIObject(name, parent, typeof(Image));
        Image image = obj.GetComponent<Image>();
        image.sprite = sprite;
        image.type = Image.Type.Sliced;
        image.color = color;
        return image;
    }

    private static TextMeshProUGUI CreateText(Transform parent, string name, string text, float size, TextAlignmentOptions alignment, TMP_FontAsset font, Color color)
    {
        GameObject obj = CreateUIObject(name, parent, typeof(TextMeshProUGUI));
        TextMeshProUGUI label = obj.GetComponent<TextMeshProUGUI>();
        label.text = text;
        label.font = font;
        label.fontSize = size;
        label.alignment = alignment;
        label.color = color;
        label.raycastTarget = false;
        label.textWrappingMode = TextWrappingModes.NoWrap;
        return label;
    }

    private static GameObject CreateUIObject(string name, Transform parent, params System.Type[] components)
    {
        GameObject obj = new GameObject(name, typeof(RectTransform));
        obj.transform.SetParent(parent, false);
        foreach (System.Type component in components)
        {
            if (component != typeof(RectTransform) && obj.GetComponent(component) == null)
            {
                obj.AddComponent(component);
            }
        }
        return obj;
    }

    private static T GetOrAdd<T>(GameObject obj) where T : Component
    {
        T component = obj.GetComponent<T>();
        return component != null ? component : obj.AddComponent<T>();
    }

    private static void ClearChildren(Transform parent)
    {
        for (int i = parent.childCount - 1; i >= 0; i--)
        {
            Object.DestroyImmediate(parent.GetChild(i).gameObject);
        }
    }

    private static void Stretch(RectTransform rect, float left, float right, float top, float bottom)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = new Vector2(left, bottom);
        rect.offsetMax = new Vector2(-right, -top);
    }

    private static Transform FindChildRecursive(Transform root, string name)
    {
        if (root == null)
        {
            return null;
        }

        if (root.name == name)
        {
            return root;
        }

        for (int i = 0; i < root.childCount; i++)
        {
            Transform result = FindChildRecursive(root.GetChild(i), name);
            if (result != null)
            {
                return result;
            }
        }

        return null;
    }

    private static Sprite LoadSpriteAsset(string assetPath)
    {
        TextureImporter importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
        if (importer != null && importer.textureType != TextureImporterType.Sprite)
        {
            importer.textureType = TextureImporterType.Sprite;
            importer.alphaIsTransparency = true;
            importer.mipmapEnabled = false;
            importer.SaveAndReimport();
        }

        return AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);
    }
}

