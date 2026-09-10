using UnityEngine;
using UnityEditor;
using TMPro;
using UnityEngine.UI;

public class LoginSceneSetup : EditorWindow
{
    [MenuItem("E-Lab Prime/Setup Login Scene")]
    public static void SetupLoginScene()
    {
        // 1. Ensure the active scene is Login
        var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
        if (scene.name != "Login")
        {
            Debug.LogError("Sila buka scene 'Login' terlebih dahulu sebelum menjalankan setup!");
            return;
        }

        // 2. Locate Canvas
        GameObject canvasObj = GameObject.Find("Canvas");
        if (canvasObj == null)
        {
            Debug.LogError("Canvas tidak dijumpai!");
            return;
        }

        // 3. Locate background and PanelLogin
        Transform backgroundTrans = canvasObj.transform.Find("background");
        if (backgroundTrans == null)
        {
            Debug.LogError("GameObject 'background' tidak dijumpai!");
            return;
        }

        Transform panelLoginTrans = backgroundTrans.Find("PanelLogin");
        if (panelLoginTrans == null)
        {
            Debug.LogError("GameObject 'PanelLogin' tidak dijumpai!");
            return;
        }

        Undo.RegisterCompleteObjectUndo(canvasObj, "Setup Login Scene UI");

        // Apply new classroom background
        Image bgImg = backgroundTrans.GetComponent<Image>();
        if (bgImg != null)
        {
            Sprite bgSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/UI Flat/classroom_bg.png");
            if (bgSprite != null)
            {
                bgImg.sprite = bgSprite;
                bgImg.color = Color.white;
            }
        }

        // 4. Create Overlay/Vignette under background (sibling to PanelLogin, but placed before it to render behind)
        Transform overlayTrans = backgroundTrans.Find("Overlay");
        GameObject overlayObj;
        if (overlayTrans == null)
        {
            overlayObj = new GameObject("Overlay");
            overlayObj.transform.SetParent(backgroundTrans, false);
            overlayObj.transform.SetSiblingIndex(0); // Make it render behind PanelLogin
        }
        else
        {
            overlayObj = overlayTrans.gameObject;
        }

        RectTransform overlayRect = overlayObj.GetComponent<RectTransform>();
        if (overlayRect == null) overlayRect = overlayObj.AddComponent<RectTransform>();
        overlayRect.anchorMin = Vector2.zero;
        overlayRect.anchorMax = Vector2.one;
        overlayRect.sizeDelta = Vector2.zero;
        overlayRect.anchoredPosition = Vector2.zero;

        Image overlayImg = overlayObj.GetComponent<Image>();
        if (overlayImg == null) overlayImg = overlayObj.AddComponent<Image>();
        overlayImg.color = new Color(0f, 0f, 0f, 0.15f); // Semi-transparent black vignette

        // 5. Stylize PanelLogin background
        Image panelImg = panelLoginTrans.GetComponent<Image>();
        if (panelImg != null)
        {
            // Dark frosted glass panel for better contrast against bright background
            panelImg.color = new Color(0.1f, 0.15f, 0.2f, 0.7f);

            Outline outline = panelLoginTrans.GetComponent<Outline>();
            if (outline == null) outline = panelLoginTrans.gameObject.AddComponent<Outline>();
            outline.effectColor = new Color(1f, 1f, 1f, 0.15f);
            outline.effectDistance = new Vector2(1f, -1f);
        }

        // 6. Stylize Header
        Transform headerTrans = panelLoginTrans.Find("Header");
        if (headerTrans != null)
        {
            Image headerImg = headerTrans.GetComponent<Image>();
            if (headerImg != null)
            {
                // Transparent header
                headerImg.color = new Color(0f, 0f, 0f, 0f);
            }

            // Title Typography Outline Adjustments
            Transform titleTrans = headerTrans.Find("Text (TMP)");
            if (titleTrans != null)
            {
                TextMeshProUGUI titleText = titleTrans.GetComponent<TextMeshProUGUI>();
                if (titleText != null)
                {
                    titleText.color = Color.white;
                    titleText.fontStyle = FontStyles.Bold;
                    titleText.outlineWidth = 0.05f;
                    titleText.outlineColor = new Color(0f, 0f, 0f, 0.8f);
                }
            }

            Transform subtitleTrans = headerTrans.Find("Text (TMP) (1)");
            if (subtitleTrans != null)
            {
                TextMeshProUGUI subtitleText = subtitleTrans.GetComponent<TextMeshProUGUI>();
                if (subtitleText != null)
                {
                    subtitleText.color = new Color(1f, 1f, 1f, 0.9f); // softer white
                    subtitleText.fontStyle = FontStyles.Normal;
                    subtitleText.outlineWidth = 0f;
                }
            }
        }

        // 7. Stylize Body contents (Inputs, Buttons)
        Transform bodyTrans = panelLoginTrans.Find("Body");
        if (bodyTrans != null)
        {
            // Spacing adjustment
            VerticalLayoutGroup vlg = bodyTrans.GetComponent<VerticalLayoutGroup>();
            if (vlg != null)
            {
                vlg.spacing = 18f;
                vlg.childForceExpandHeight = false;
                vlg.childControlHeight = false;
            }

            // Input fields
            Transform usernameInputTrans = bodyTrans.Find("InputField (TMP)");
            Transform passwordInputTrans = bodyTrans.Find("InputField (TMP) (1)");
            Sprite roundedSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/UI Flat/pb_rect_rounded.png");

            if (usernameInputTrans != null)
            {
                ConfigureInputField(usernameInputTrans, roundedSprite, "Masukkan nama pengguna...");
            }

            if (passwordInputTrans != null)
            {
                ConfigureInputField(passwordInputTrans, roundedSprite, "Masukkan kata laluan...");
                // Set password content type
                TMP_InputField tmpInput = passwordInputTrans.GetComponent<TMP_InputField>();
                if (tmpInput != null)
                {
                    tmpInput.contentType = TMP_InputField.ContentType.Password;
                }
            }

            // Stylize Labels
            Transform usernameLabelTrans = bodyTrans.Find("Text (TMP)");
            if (usernameLabelTrans != null)
            {
                TextMeshProUGUI label = usernameLabelTrans.GetComponent<TextMeshProUGUI>();
                if (label != null)
                {
                    label.color = Color.white;
                    label.fontSize = 18f;
                    label.fontStyle = FontStyles.Bold;
                    label.outlineWidth = 0f;
                }
            }

            Transform passwordLabelTrans = bodyTrans.Find("Text (TMP) (1)");
            if (passwordLabelTrans != null)
            {
                TextMeshProUGUI label = passwordLabelTrans.GetComponent<TextMeshProUGUI>();
                if (label != null)
                {
                    label.color = Color.white;
                    label.fontSize = 18f;
                    label.fontStyle = FontStyles.Bold;
                    label.outlineWidth = 0f;
                }
            }

            // Stylize Buttons
            Transform loginBtnTrans = bodyTrans.Find("Button");
            if (loginBtnTrans != null)
            {
                Image btnImg = loginBtnTrans.GetComponent<Image>();
                if (btnImg != null)
                {
                    // Vibrant green button
                    btnImg.color = new Color(0.2f, 0.85f, 0.45f, 1f);
                }

                // Make sure bouncy button is attached
                if (loginBtnTrans.GetComponent<BouncyButton>() == null)
                {
                    loginBtnTrans.gameObject.AddComponent<BouncyButton>();
                }
                
                // Align text style
                TextMeshProUGUI btnText = loginBtnTrans.GetComponentInChildren<TextMeshProUGUI>();
                if (btnText != null)
                {
                    btnText.color = Color.white;
                    btnText.fontStyle = FontStyles.Bold;
                    btnText.outlineWidth = 0f;
                }
            }

            Transform registerBtnTrans = bodyTrans.Find("Button (1)");
            if (registerBtnTrans != null)
            {
                // Make it look like a borderless text link
                Image btnImg = registerBtnTrans.GetComponent<Image>();
                if (btnImg != null)
                {
                    btnImg.color = new Color(0f, 0f, 0f, 0f); // completely transparent background
                }

                // Add bouncy button for feedback
                if (registerBtnTrans.GetComponent<BouncyButton>() == null)
                {
                    registerBtnTrans.gameObject.AddComponent<BouncyButton>();
                }

                TextMeshProUGUI btnText = registerBtnTrans.GetComponentInChildren<TextMeshProUGUI>();
                if (btnText != null)
                {
                    btnText.text = "Belum daftar? Sila daftar di sini";
                    btnText.color = new Color(1f, 1f, 1f, 0.85f); // Clean soft white
                    btnText.fontStyle = FontStyles.Underline;
                    btnText.outlineWidth = 0f;
                    btnText.fontSize = 16f;
                }
            }

            // 8. Create or configure StatusText GameObject (added under Body so layout takes care of positioning)
            Transform statusTextTrans = bodyTrans.Find("StatusText");
            GameObject statusTextObj;
            if (statusTextTrans == null)
            {
                statusTextObj = new GameObject("StatusText");
                statusTextObj.transform.SetParent(bodyTrans, false);
            }
            else
            {
                statusTextObj = statusTextTrans.gameObject;
            }

            TextMeshProUGUI statusTextComp = statusTextObj.GetComponent<TextMeshProUGUI>();
            if (statusTextComp == null) statusTextComp = statusTextObj.AddComponent<TextMeshProUGUI>();
            
            // Set text component properties
            statusTextComp.text = ""; // Empty initially
            statusTextComp.alignment = TextAlignmentOptions.Center;
            statusTextComp.fontSize = 16f;
            statusTextComp.fontStyle = FontStyles.Normal;
            statusTextComp.outlineWidth = 0.04f;
            statusTextComp.color = Color.white;
            
            // Assign font asset
            TMP_FontAsset fontAsset = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Fonts/FredokaOne SDF.asset");
            if (fontAsset != null)
            {
                statusTextComp.font = fontAsset;
            }

            // Layout Element to give it some padding/size in layout
            LayoutElement le = statusTextObj.GetComponent<LayoutElement>();
            if (le == null) le = statusTextObj.AddComponent<LayoutElement>();
            le.minHeight = 30f;
            le.preferredHeight = 30f;

            // Lock in manual width and position adjustments
            RectTransform statusRect = statusTextObj.GetComponent<RectTransform>();
            if (statusRect == null) statusRect = statusTextObj.AddComponent<RectTransform>();
            statusRect.anchorMin = new Vector2(0f, 1f);
            statusRect.anchorMax = new Vector2(0f, 1f);
            statusRect.sizeDelta = new Vector2(627.67f, 50f);
            statusRect.anchoredPosition = new Vector2(306.07f, -489.47f);

            // 9. Attach and configure LoginManager on Canvas
            LoginManager loginMgr = canvasObj.GetComponent<LoginManager>();
            if (loginMgr == null) loginMgr = canvasObj.AddComponent<LoginManager>();

            // Assign references
            if (usernameInputTrans != null) loginMgr.usernameInput = usernameInputTrans.GetComponent<TMP_InputField>();
            if (passwordInputTrans != null) loginMgr.passwordInput = passwordInputTrans.GetComponent<TMP_InputField>();
            if (loginBtnTrans != null) loginMgr.loginButton = loginBtnTrans.GetComponent<Button>();
            if (registerBtnTrans != null) loginMgr.registerButton = registerBtnTrans.GetComponent<Button>();
            loginMgr.statusText = statusTextComp;
        }

        // 10. Save and mark as dirty
        EditorUtility.SetDirty(canvasObj);
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(scene);
        UnityEditor.SceneManagement.EditorSceneManager.SaveScene(scene);
        
        Debug.Log("Login scene UI successfully updated and saved!");
    }

    private static void ConfigureInputField(Transform inputTrans, Sprite roundedSprite, string placeholderTextStr)
    {
        // Set sprite to rounded
        Image inputImg = inputTrans.GetComponent<Image>();
        if (inputImg != null && roundedSprite != null)
        {
            inputImg.sprite = roundedSprite;
            inputImg.overrideSprite = roundedSprite;
            inputImg.type = Image.Type.Sliced;
            inputImg.pixelsPerUnitMultiplier = 2f;
            inputImg.color = new Color(0f, 0f, 0f, 0.35f); // Dark translucent recessed slot
        }

        TMP_InputField tmpInput = inputTrans.GetComponent<TMP_InputField>();
        if (tmpInput != null && tmpInput.textComponent != null)
        {
            tmpInput.textComponent.color = Color.white;
        }

        // Configure placeholder text
        Transform placeholderTrans = inputTrans.Find("Placeholder");
        if (placeholderTrans != null)
        {
            TextMeshProUGUI placeholderText = placeholderTrans.GetComponent<TextMeshProUGUI>();
            if (placeholderText != null)
            {
                placeholderText.text = placeholderTextStr;
                placeholderText.fontStyle = FontStyles.Italic;
                placeholderText.color = new Color(1f, 1f, 1f, 0.5f); // Soft white placeholder
            }
        }
    }
}
