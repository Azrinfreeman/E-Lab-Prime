using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class Lab5TutorialManager : MonoBehaviour
{
    public Lab5VoiceInstructions voiceInstructions;
    public SolubilityManager3D manager;
    public Transform switchTarget;
    public Transform tabletTarget;

    private GameObject hint1Obj;
    private GameObject hint2Obj;
    private bool hint1Resolved = false;
    private bool hint2Resolved = false;

    void Start()
    {
        manager = Object.FindAnyObjectByType<SolubilityManager3D>();
        StartCoroutine(TutorialSequence());
    }

    public void ResetTutorial(bool skipCameraDelay = true)
    {
        StopAllCoroutines();
        if (voiceInstructions != null) voiceInstructions.ResetPresentation();
        
        if (hint1Obj != null) Destroy(hint1Obj);
        if (hint2Obj != null) Destroy(hint2Obj);
        
        foreach (var obj in Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None))
        {
            if (obj != null && obj.name.StartsWith("TutorialHint_"))
            {
                Destroy(obj);
            }
        }

        hint1Resolved = false;
        hint2Resolved = false;

        StartCoroutine(TutorialSequence(skipCameraDelay));
    }

    private IEnumerator TutorialSequence(bool skipCameraDelay = false)
    {
        if (!skipCameraDelay)
        {
            // Wait for intro camera to settle
            yield return new WaitForSeconds(2.6f);
        }
        else
        {
            yield return null;
        }

        if (manager == null) manager = Object.FindAnyObjectByType<SolubilityManager3D>();
        if (manager == null) yield break;

        if (voiceInstructions != null) voiceInstructions.Present(0);
        // Create Hint 1: Pointing at Heater Power Switch
        if (manager.powerSwitch != null)
        {
            switchTarget = manager.powerSwitch.transform;
            Vector3 off1 = new Vector3(0f, 0.04f, -0.16f);
            hint1Obj = CreateFloatingHint("1. Klik Untuk Hidupkan Pemanas", switchTarget.position + off1, Color.yellow, switchTarget, off1, false);
        }

        // Create Hint 2: Pointing at Table / Beaker
        GameObject tableTargetObj = GameObject.Find("Beaker_3D") ?? GameObject.Find("Pemanas_3D") ?? GameObject.Find("Lab5_GridMat");
        if (tableTargetObj != null)
        {
            tabletTarget = tableTargetObj.transform;
            Vector3 off2 = new Vector3(0f, 0.45f, 0f);
            hint2Obj = CreateFloatingHint("2. Laraskan Suhu, Kacauan & Saiz", tabletTarget.position + off2, new Color(0.2f, 1f, 0.8f), tabletTarget, off2, false);
        }

        // Monitor interactions (hints re-show every scene load as requested)
        while (!hint1Resolved || !hint2Resolved)
        {
            if (manager != null && manager.experimentStarted)
            {
                hint1Resolved = true;
                hint2Resolved = true;
                if (hint1Obj != null) StartCoroutine(FadeAndDestroy(hint1Obj));
                if (hint2Obj != null) StartCoroutine(FadeAndDestroy(hint2Obj));
                break;
            }

            if (!hint1Resolved && manager != null && manager.powerSwitch != null && manager.powerSwitch.isOn)
            {
                hint1Resolved = true;
                if (hint1Obj != null) StartCoroutine(FadeAndDestroy(hint1Obj));
            }

            if (!hint2Resolved && manager != null)
            {
                bool sliderMoved = (manager.tempSlider != null && manager.tempSlider.value > 0.05f) ||
                                   (manager.stirSlider != null && manager.stirSlider.value > 0.05f) ||
                                   (manager.sizeSlider != null && manager.sizeSlider.value > 0.05f);
                if (sliderMoved)
                {
                    hint2Resolved = true;
                    if (hint2Obj != null) StartCoroutine(FadeAndDestroy(hint2Obj));
                }
            }

            yield return null;
        }

        if (voiceInstructions != null) voiceInstructions.Present(1);
        // Create Hint 3: Pointing at Table / Beaker to tell player to click Start Experiment
        GameObject hint3Obj = null;
        if (manager != null && !manager.experimentStarted)
        {
            Vector3 off3Start = new Vector3(0f, 0.45f, 0f);
            hint3Obj = CreateFloatingHint("3. Klik Butang 'START EKSPERIMEN' Pada Tablet", tabletTarget != null ? tabletTarget.position + off3Start : new Vector3(-3f, 2.09f, 2f), new Color(1f, 0.5f, 0.2f), tabletTarget, off3Start, false);
        }

        // Wait until experiment starts
        while (manager != null && !manager.experimentStarted)
        {
            yield return null;
        }

        if (hint3Obj != null) StartCoroutine(FadeAndDestroy(hint3Obj));

        // Wait until experiment finishes to tell the player what to do next!
        while (manager != null && !manager.experimentFinished)
        {
            yield return null;
        }

        if (manager != null && manager.experimentFinished)
        {
            if (voiceInstructions != null) voiceInstructions.Present(2);
            GameObject notebook = GameObject.Find("Lab5_Notebook") ?? GameObject.Find("InteractiveNotebook_Lab5") ?? GameObject.Find("InteractiveNotebook");
            if (notebook == null)
            {
                var nbScript = Object.FindAnyObjectByType<InteractiveNotebookLab5>();
                if (nbScript != null) notebook = nbScript.gameObject;
            }
            Vector3 off4 = new Vector3(-0.15f, 0.28f, -0.05f);
            Vector3 hintPos = notebook != null ? notebook.transform.position + off4 : (switchTarget != null ? switchTarget.position + new Vector3(0.6f, 0.28f, -0.05f) : new Vector3(0f, 1.6f, 0f));
            GameObject hint4Obj = CreateFloatingHint("4. Selesai! Klik [Log Data] Pada Buku Untuk Kuiz", hintPos, new Color(1f, 0.85f, 0.2f), notebook != null ? notebook.transform : null, off4, false);
        }
    }

    private GameObject CreateFloatingHint(string text, Vector3 worldPos, Color textColor, Transform targetToFollow = null, Vector3 localOffset = default, bool followInCameraSpace = false)
    {
        GameObject hint = new GameObject("TutorialHint_" + text.Substring(0, Mathf.Min(5, text.Length)));
        hint.transform.position = worldPos;

        var bob = hint.AddComponent<FloatingBobLab5>();
        bob.targetToFollow = targetToFollow;
        bob.localOffset = localOffset;
        bob.followInCameraSpace = followInCameraSpace;

        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(hint.transform, false);
        textObj.transform.localPosition = new Vector3(0f, 0f, -0.005f); // Positioned slightly in front of badge box
        textObj.transform.localScale = new Vector3(0.008f, 0.008f, 0.008f);

        TextMesh tm = textObj.AddComponent<TextMesh>();
        tm.text = text;
        tm.fontSize = 64;
        tm.anchor = TextAnchor.MiddleCenter;
        tm.alignment = TextAlignment.Center;
        tm.color = KidFriendlySceneTheme.Current ? KidFriendlySceneTheme.MaterialColor(KidFriendlyPalette.Ink) : textColor;
        tm.fontStyle = FontStyle.Bold;

        // Create a sleek dark midnight-blue HUD badge background for 100% readability against any background
        GameObject bgObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
        bgObj.name = "BadgeBackground";
        bgObj.transform.SetParent(hint.transform, false);
        bgObj.transform.localPosition = Vector3.zero;
        float exactTextWidth = 0f;
        var textRenderer = tm.GetComponent<Renderer>();
        if (textRenderer != null) exactTextWidth = textRenderer.bounds.size.x;
        float panelWidth = Mathf.Max(0.6f, Mathf.Max(exactTextWidth + 0.3f, text.Length * 0.028f + 0.2f));
        bgObj.transform.localScale = new Vector3(panelWidth, 0.13f, 0.006f);
        DestroyImmediate(bgObj.GetComponent<Collider>());

        Material bgMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        if (bgMat.shader == null || !bgMat.shader.isSupported) bgMat = new Material(Shader.Find("Standard"));
        bgMat.color = KidFriendlySceneTheme.Current ? KidFriendlySceneTheme.MaterialColor(KidFriendlyPalette.Slot) : new Color(0.04f, 0.07f, 0.15f, 0.90f); // Sleek high-contrast dark badge
        bgMat.SetFloat("_Surface", 1);
        bgMat.SetFloat("_Blend", 0);
        bgMat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        bgMat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        bgMat.SetInt("_ZWrite", 0);
        bgMat.renderQueue = 3000;
        bgObj.GetComponent<MeshRenderer>().sharedMaterial = bgMat;

        var billboard = hint.AddComponent<BillboardLab5>();

        return hint;
    }

    private IEnumerator FadeAndDestroy(GameObject target)
    {
        TextMesh[] texts = target.GetComponentsInChildren<TextMesh>();
        MeshRenderer[] renderers = target.GetComponentsInChildren<MeshRenderer>();
        float elapsed = 0f;
        float duration = 0.8f;
        while (elapsed < duration && target != null)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsed / duration);
            foreach (var tm in texts)
            {
                if (tm != null)
                {
                    Color c = tm.color;
                    c.a = alpha;
                    tm.color = c;
                }
            }
            foreach (var r in renderers)
            {
                if (r != null && r.material != null)
                {
                    Color c = r.material.color;
                    c.a = alpha * 0.90f;
                    r.material.color = c;
                }
            }
            yield return null;
        }
        if (target != null) Destroy(target);
    }
}

public class FloatingBobLab5 : MonoBehaviour
{
    public Transform targetToFollow;
    public Vector3 localOffset;
    public bool followInCameraSpace = false;

    private Vector3 startPos;
    void Start() { startPos = transform.position; }
    void Update()
    {
        Vector3 basePos = startPos;
        if (targetToFollow != null)
        {
            if (followInCameraSpace && Camera.main != null)
            {
                basePos = targetToFollow.position + Camera.main.transform.up * localOffset.y + Camera.main.transform.right * localOffset.x + Camera.main.transform.forward * localOffset.z;
            }
            else
            {
                basePos = targetToFollow.position + localOffset;
            }
        }
        transform.position = basePos + new Vector3(0f, Mathf.Sin(Time.time * 3f) * 0.015f, 0f);

        if (Camera.main != null)
        {
            float dist = Vector3.Distance(transform.position, Camera.main.transform.position);
            float distScale = Mathf.Clamp(dist * 0.35f, 0.5f, 3.5f);
            transform.localScale = new Vector3(distScale, distScale, distScale);
        }
    }
}

public class BillboardLab5 : MonoBehaviour
{
    void LateUpdate()
    {
        if (Camera.main != null)
        {
            transform.LookAt(transform.position + Camera.main.transform.rotation * Vector3.forward,
                             Camera.main.transform.rotation * Vector3.up);
        }
    }
}

