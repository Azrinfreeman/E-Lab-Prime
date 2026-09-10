using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public static class MainMenuHomeNavButtonSetup
{
    private const string MainMenuScenePath = "Assets/Scenes/MainMenu.unity";
    private const string GlassSpritePath = "Assets/UI Flat/pb_rect_rounded.png";

    private static readonly Color NormalColor = new Color(0.10f, 0.16f, 0.28f, 0.88f);
    private static readonly Color AccentColor = new Color(0.18f, 0.77f, 0.71f, 1f);
    private static readonly Color IconDark = new Color(0.012f, 0.03f, 0.04f, 1f);

    [MenuItem("E-Lab Prime/Setup Main Menu Home Nav Button")]
    public static void SetupHomeNavButton()
    {
        if (EditorSceneManager.GetActiveScene().path != MainMenuScenePath)
        {
            EditorSceneManager.SaveOpenScenes();
            EditorSceneManager.OpenScene(MainMenuScenePath, OpenSceneMode.Single);
        }

        Canvas canvas = Object.FindAnyObjectByType<Canvas>(FindObjectsInactive.Include);
        if (canvas == null)
        {
            Debug.LogError("Setup Main Menu Home Nav Button failed: no Canvas found.");
            return;
        }

        Transform sideMenu = FindChildRecursive(canvas.transform, "SIdemenu");
        if (sideMenu == null)
        {
            Debug.LogError("Setup Main Menu Home Nav Button failed: SIdemenu was not found.");
            return;
        }

        MainMenuManager menuManager = Object.FindAnyObjectByType<MainMenuManager>(FindObjectsInactive.Include);
        if (menuManager == null)
        {
            Debug.LogError("Setup Main Menu Home Nav Button failed: MainMenuManager was not found.");
            return;
        }

        Undo.RegisterCompleteObjectUndo(canvas.gameObject, "Setup Main Menu Home Nav Button");

        Transform existing = sideMenu.Find("HomeNavButton");
        if (existing != null)
        {
            Object.DestroyImmediate(existing.gameObject);
        }

        RectTransform sideRect = sideMenu.GetComponent<RectTransform>();
        if (sideRect != null)
        {
            sideRect.sizeDelta = new Vector2(Mathf.Max(sideRect.sizeDelta.x, 140f), Mathf.Max(sideRect.sizeDelta.y, 610f));
        }

        VerticalLayoutGroup layout = sideMenu.GetComponent<VerticalLayoutGroup>();
        if (layout != null)
        {
            layout.spacing = 8f;
            layout.childAlignment = TextAnchor.MiddleCenter;
        }

        Sprite glassSprite = LoadSpriteAsset(GlassSpritePath);

        GameObject buttonObject = CreateUIObject("HomeNavButton", sideMenu, typeof(Image), typeof(Button), typeof(LayoutElement));
        buttonObject.transform.SetSiblingIndex(0);

        RectTransform buttonRect = buttonObject.GetComponent<RectTransform>();
        buttonRect.sizeDelta = new Vector2(100f, 100f);

        LayoutElement buttonLayout = buttonObject.GetComponent<LayoutElement>();
        buttonLayout.preferredWidth = 100f;
        buttonLayout.preferredHeight = 100f;
        buttonLayout.minWidth = 92f;
        buttonLayout.minHeight = 92f;

        Image buttonImage = buttonObject.GetComponent<Image>();
        buttonImage.sprite = glassSprite;
        buttonImage.type = Image.Type.Sliced;
        buttonImage.color = NormalColor;
        buttonImage.pixelsPerUnitMultiplier = 2f;

        Button button = buttonObject.GetComponent<Button>();
        button.targetGraphic = buttonImage;
        button.transition = Selectable.Transition.ColorTint;
        button.onClick.RemoveAllListeners();
        UnityAction action = menuManager.BukaUtama;
        UnityEventTools.AddPersistentListener(button.onClick, action);

        if (buttonObject.GetComponent<BouncyButton>() == null)
        {
            buttonObject.AddComponent<BouncyButton>();
        }

        BuildHomeIcon(buttonObject.transform);
        BuildLabel(buttonObject.transform);

        menuManager.butangNavUtama = buttonImage;
        EditorUtility.SetDirty(menuManager);
        EditorUtility.SetDirty(canvas.gameObject);
        MainMenuKidFriendlyColourSetup.Apply();
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
        AssetDatabase.SaveAssets();

        Debug.Log("Main Menu Home navigation button created and wired successfully.");
    }

    private static void BuildHomeIcon(Transform parent)
    {
        GameObject iconRoot = CreateUIObject("HomeIcon", parent);
        RectTransform iconRootRect = iconRoot.GetComponent<RectTransform>();
        iconRootRect.anchorMin = new Vector2(0.5f, 0.5f);
        iconRootRect.anchorMax = new Vector2(0.5f, 0.5f);
        iconRootRect.pivot = new Vector2(0.5f, 0.5f);
        iconRootRect.sizeDelta = new Vector2(62f, 58f);
        iconRootRect.anchoredPosition = new Vector2(0f, 3f);

        Image roofLeft = CreateShape("RoofLeft", iconRoot.transform, AccentColor);
        SetShape(roofLeft.rectTransform, new Vector2(-16f, 16f), new Vector2(42f, 12f), 34f);

        Image roofRight = CreateShape("RoofRight", iconRoot.transform, AccentColor);
        SetShape(roofRight.rectTransform, new Vector2(16f, 16f), new Vector2(42f, 12f), -34f);

        Image body = CreateShape("Body", iconRoot.transform, AccentColor);
        SetShape(body.rectTransform, new Vector2(0f, -8f), new Vector2(46f, 34f), 0f);

        Image door = CreateShape("Door", iconRoot.transform, IconDark);
        SetShape(door.rectTransform, new Vector2(0f, -15f), new Vector2(13f, 20f), 0f);

        Image baseLine = CreateShape("BaseLine", iconRoot.transform, IconDark);
        SetShape(baseLine.rectTransform, new Vector2(0f, -28f), new Vector2(54f, 6f), 0f);
    }

    private static void BuildLabel(Transform parent)
    {
        TMP_FontAsset font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Fonts/FredokaOne SDF.asset");
        GameObject labelObject = CreateUIObject("HomeTooltipLabel", parent, typeof(TextMeshProUGUI));
        RectTransform labelRect = labelObject.GetComponent<RectTransform>();
        labelRect.anchorMin = new Vector2(0.5f, 0f);
        labelRect.anchorMax = new Vector2(0.5f, 0f);
        labelRect.pivot = new Vector2(0.5f, 0f);
        labelRect.sizeDelta = new Vector2(96f, 20f);
        labelRect.anchoredPosition = new Vector2(0f, 7f);

        TextMeshProUGUI label = labelObject.GetComponent<TextMeshProUGUI>();
        label.text = "MENU";
        label.font = font;
        label.fontSize = 11f;
        label.fontStyle = FontStyles.Bold;
        label.alignment = TextAlignmentOptions.Center;
        label.color = Color.white;
        label.raycastTarget = false;
    }

    private static Image CreateShape(string name, Transform parent, Color color)
    {
        GameObject shapeObject = CreateUIObject(name, parent, typeof(Image));
        Image image = shapeObject.GetComponent<Image>();
        image.color = color;
        image.raycastTarget = false;
        return image;
    }

    private static void SetShape(RectTransform rect, Vector2 anchoredPosition, Vector2 size, float rotationZ)
    {
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = size;
        rect.localRotation = Quaternion.Euler(0f, 0f, rotationZ);
    }

    private static GameObject CreateUIObject(string name, Transform parent, params System.Type[] components)
    {
        GameObject obj = new GameObject(name, typeof(RectTransform));
        obj.layer = parent.gameObject.layer;
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

    private static Transform FindChildRecursive(Transform parent, string name)
    {
        if (parent == null) return null;
        if (parent.name == name) return parent;

        for (int i = 0; i < parent.childCount; i++)
        {
            Transform result = FindChildRecursive(parent.GetChild(i), name);
            if (result != null) return result;
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
