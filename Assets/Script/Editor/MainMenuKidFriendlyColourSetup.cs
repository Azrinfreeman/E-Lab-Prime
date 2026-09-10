using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using P = KidFriendlyPalette;

/// <summary>Idempotent, scene-local styling. Does not rebuild controls or save the scene.</summary>
public static class MainMenuKidFriendlyColourSetup
{
    [MenuItem("E-Lab/Setup/Apply Kid-Friendly Main Menu Colours")]
    public static void Apply()
    {
        var scene = EditorSceneManager.GetActiveScene();
        if (scene.path != "Assets/Scenes/MainMenu.unity" || Application.isPlaying)
        {
            Debug.LogError("Open MainMenu in Edit mode before applying its colours.");
            return;
        }
        var all = scene.GetRootGameObjects().SelectMany(r => r.GetComponentsInChildren<Transform>(true)).ToArray();
        foreach (string required in new[] { "PanelLogin", "AmaliPanel", "PanelPengaturan", "PanelPencapaian", "PanelAkaun", "SIdemenu" })
            if (!all.Any(t => t.name == required))
            {
                Debug.LogError("MainMenu colour target missing: " + required);
                return;
            }
        var rounded = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/UI Flat/pb_rect_rounded.png");
        if (rounded == null) { Debug.LogError("MainMenu rounded surface sprite missing."); return; }
        foreach (var root in scene.GetRootGameObjects()) Undo.RegisterFullObjectHierarchyUndo(root, "MainMenu bright colours");
        var background = all.FirstOrDefault(t => Path(t) == "Canvas/background");
        if (background != null && !all.Any(t => t.name == "KidFriendlySkyWash"))
        {
            var wash = new GameObject("KidFriendlySkyWash", typeof(RectTransform), typeof(Image));
            Undo.RegisterCreatedObjectUndo(wash, "MainMenu sky wash");
            wash.transform.SetParent(background, false);
            wash.transform.SetAsFirstSibling();
            var rect = (RectTransform)wash.transform;
            rect.anchorMin = Vector2.zero; rect.anchorMax = Vector2.one;
            rect.offsetMin = rect.offsetMax = Vector2.zero;
            wash.GetComponent<Image>().color = new Color(P.Sky.r, P.Sky.g, P.Sky.b, 0.32f);
            wash.GetComponent<Image>().raycastTarget = false;
        }
        int count = 0;
        foreach (var t in all)
        {
            string path = Path(t);
            // Experiment thumbnails contain meaningful water, heat and apparatus colours.
            bool illustration = path.Contains("/IconContainer/");
            var img = t.GetComponent<Image>();
            if (img != null && !illustration)
            {
                string asset = AssetDatabase.GetAssetPath(img.sprite);
                bool surface = asset.EndsWith("pb_rect_rounded.png") || asset.Contains("/btn_white_flat") || asset.Contains("/btn_gray_flat") || asset.Contains("/btn_light_blue");
                bool nav = t.parent != null && t.parent.name == "SIdemenu";
                Color? colour = null;
                if (surface || nav)
                {
                    colour = P.Surface;
                    if (t.name.Contains("Input") || t.name == "Dropdown" || t.name == "SignedInPanel" || t.name == "StatusSummary" || t.name == "ProfileFooter") colour = P.Slot;
                    if (t.name.StartsWith("Row_Lab") || t.name == "LockedBadge" || t.name == "BarBG" || t.name == "ProgressTrack" || t.name == "Background") colour = P.Neutral;
                    if (t.name == "BarFill" || t.name == "Fill" || t.name == "UnlockedBadge" || t.name == "SaveStatusDot") colour = P.Mint;
                    if (t.name == "Header" || t.name == "AvatarCircle") colour = P.Blue;
                    if (t.name == "IconContainer") colour = Accent(t.parent.name);
                    if (t.GetComponent<Button>() != null) colour = ButtonColour(t, path);
                    if (t.name.StartsWith("Amali") && t.name != "AmaliPanel") colour = Accent(t.name);
                    if (t.name == "ProfessorDialoguePanel") colour = P.Slot;
                    if (nav) colour = t.name == "HomeNavButton" ? P.Yellow : P.Slot;
                    if (t.name == "ProfessorHighlight") colour = new Color(P.Yellow.r, P.Yellow.g, P.Yellow.b, 0.3f);
                    // Neutral white sprites prevent baked coloured button textures multiplying the palette.
                    if (surface && img.sprite != rounded) img.sprite = rounded;
                }
                if (t.name == "ConfirmationPanel" || t.name == "DimBackground") colour = new Color(P.Ink.r, P.Ink.g, P.Ink.b, 0.5f);
                if (t.name == "Separator" || t.name == "Divider") colour = P.Neutral;
                if (t.name == "ProgressFill" || t.name == "StatusAccent") colour = P.Mint;
                if (t.name == "KidFriendlySkyWash") colour = new Color(P.Sky.r, P.Sky.g, P.Sky.b, 0.32f);
                if (t.name == "ScanlineSweep") colour = new Color(P.Blue.r, P.Blue.g, P.Blue.b, 0.12f);
                if (path.Contains("/HomeIcon/")) colour = P.Ink;
                if (t.name == "Handle") { colour = P.Lavender; img.sprite = rounded; img.type = Image.Type.Simple; img.preserveAspect = true; }
                if (t.name == "PanelPengaturan") colour = new Color(P.Sky.r, P.Sky.g, P.Sky.b, 0.55f);
                if (t.name == "Item Background") colour = P.Slot;
                if (path == "Canvas/background/PanelPencapaian/Image") colour = P.Sky;
                if (asset.EndsWith("icon sprites.png")) colour = Color.white;
                if (colour.HasValue) { img.color = colour.Value; EditorUtility.SetDirty(img); count++; }
            }
            var text = t.GetComponent<TextMeshProUGUI>();
            if (text != null && !illustration)
            {
                text.color = t.name.Contains("Subtitle") || t.name.Contains("Desc") || t.name == "Placeholder" ? P.Muted : P.Ink;
                if (path.Contains("/Grid/Amali")) text.color = P.Ink;
                text.enableVertexGradient = false;
                // Instance material only; never change the shared font asset.
                text.outlineWidth = 0;
                EditorUtility.SetDirty(text);
            }
            var selectable = t.GetComponent<Selectable>();
            if (selectable != null && !illustration)
            {
                var colors = selectable.colors;
                colors.normalColor = Color.white;
                colors.highlightedColor = new Color(0.94f, 0.97f, 1f);
                colors.pressedColor = new Color(0.76f, 0.84f, 0.92f);
                colors.selectedColor = new Color(0.84f, 0.91f, 1f);
                colors.disabledColor = new Color(0.72f, 0.76f, 0.8f, 1f);
                colors.colorMultiplier = 1;
                colors.fadeDuration = 0.12f;
                selectable.colors = colors;
                EditorUtility.SetDirty(selectable);
            }
            foreach (var shadow in t.GetComponents<Shadow>())
                if (!illustration) { shadow.effectColor = new Color(P.Ink.r, P.Ink.g, P.Ink.b, 0.12f); EditorUtility.SetDirty(shadow); }
        }
        foreach (var manager in all.Select(t => t.GetComponent<MainMenuManager>()).Where(c => c != null))
        {
            manager.warnaBiasa = P.Slot; manager.warnaMenyala = P.Yellow;
            foreach (var nav in new[] { manager.butangNavUtama, manager.butangNavAmali, manager.butangNavPengaturan, manager.butangNavPencapaian, manager.butangNavAkaun })
            {
                if (nav == null) { Debug.LogWarning("Missing MainMenu navigation colour reference."); continue; }
                var outline = nav.GetComponent<Outline>() ?? Undo.AddComponent<Outline>(nav.gameObject);
                outline.effectColor = P.Ink; outline.effectDistance = new Vector2(3, -3);
                outline.enabled = nav == manager.butangNavUtama;
                EditorUtility.SetDirty(outline);
            }
            EditorUtility.SetDirty(manager);
        }
        foreach (var account in all.Select(t => t.GetComponent<MainMenuAccountPanelController>()).Where(c => c != null))
        { account.activeTabColor = P.Yellow; account.inactiveTabColor = P.Slot; account.successColor = P.Success; account.warningColor = P.Warning; account.errorColor = P.Error; EditorUtility.SetDirty(account); }
        foreach (var session in all.Select(t => t.GetComponent<MainMenuSessionStatusUI>()).Where(c => c != null))
        { session.studentColor = P.Mint; session.guestColor = P.Yellow; session.signedOutColor = P.Neutral; session.darkPillColor = P.Neutral; EditorUtility.SetDirty(session); }
        LevelCardPolishSetup.Apply();
        EditorSceneManager.MarkSceneDirty(scene);
        Debug.Log("MainMenu kid-friendly colours applied to " + count + " images. Scene ready to review and save.");
    }

    private static Color ButtonColour(Transform t, string path)
    {
        string label = string.Join(" ", t.GetComponentsInChildren<TMP_Text>(true).Select(x => x.text)).ToUpperInvariant();
        if (label.Contains("KELUAR") || label.Contains("RESET") || t.name == "Btn_CONFIRM" || t.name == "ExitButton") return P.Coral;
        if (label.Contains("PENGATURAN") || label.Contains("SETTINGS")) return P.Lavender;
        if (label.Contains("PENCAPAIAN") || label.Contains("ACHIEVEMENTS")) return P.Mint;
        if (t.name == "GuestButton" || t.name == "SkipButton" || t.name == "Btn_CANCEL") return P.Slot;
        if (t.name == "SessionStatusStrip") return P.Slot;
        return P.Yellow;
    }
    private static Color Accent(string name)
    {
        int n;
        return int.TryParse(name.Replace("Amali", ""), out n) ? new[] { P.Blue, P.Mint, P.Lavender, P.Yellow }[(n - 1) % 4] : P.Blue;
    }
    private static string Path(Transform t) => t.parent == null ? t.name : Path(t.parent) + "/" + t.name;
}

