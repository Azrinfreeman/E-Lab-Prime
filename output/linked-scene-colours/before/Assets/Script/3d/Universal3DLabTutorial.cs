using System.Collections;
using UnityEngine;

namespace ELab.Tutorial
{
    public class UniversalBadgeTracker : MonoBehaviour
    {
        public Transform targetToFollow;
        public Vector3 localOffset;
        public bool followInCameraSpace = false;

        private Vector3 startPos;

        void Start()
        {
            startPos = transform.position;
        }

        void Update()
        {
            Vector3 basePos = startPos;
            if (targetToFollow != null)
            {
                if (followInCameraSpace && Camera.main != null)
                {
                    basePos = targetToFollow.position + 
                              Camera.main.transform.up * localOffset.y + 
                              Camera.main.transform.right * localOffset.x + 
                              Camera.main.transform.forward * localOffset.z;
                }
                else
                {
                    basePos = targetToFollow.position + localOffset;
                }
            }
            // Smooth professional vertical floating bob
            transform.position = basePos + new Vector3(0f, Mathf.Sin(Time.time * 3f) * 0.015f, 0f);

            if (Camera.main != null)
            {
                float dist = Vector3.Distance(transform.position, Camera.main.transform.position);
                float distScale = Mathf.Clamp(dist * 0.35f, 0.5f, 3.5f);
                transform.localScale = new Vector3(distScale, distScale, distScale);
            }
        }
    }

    public class UniversalBadgeBillboard : MonoBehaviour
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

    public static class Universal3DLabTutorial
    {
        public static GameObject CreateInstructionBadge(string text, Transform target, Vector3 offset, Color textColor, bool followInCameraSpace = false)
        {
            GameObject hint = new GameObject("TutorialBadge_" + text.Replace(" ", "_"));
            
            Vector3 initialPos = target != null ? target.position + offset : offset;
            if (target != null && followInCameraSpace && Camera.main != null)
            {
                initialPos = target.position + 
                             Camera.main.transform.up * offset.y + 
                             Camera.main.transform.right * offset.x + 
                             Camera.main.transform.forward * offset.z;
            }
            hint.transform.position = initialPos;

            var tracker = hint.AddComponent<UniversalBadgeTracker>();
            tracker.targetToFollow = target;
            tracker.localOffset = offset;
            tracker.followInCameraSpace = followInCameraSpace;

            // Text visual
            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(hint.transform, false);
            textObj.transform.localPosition = new Vector3(0f, 0f, -0.005f); // Positioned slightly in front of badge box
            textObj.transform.localScale = new Vector3(0.008f, 0.008f, 0.008f);

            TextMesh tm = textObj.AddComponent<TextMesh>();
            tm.text = text;
            tm.fontSize = 64;
            tm.anchor = TextAnchor.MiddleCenter;
            tm.alignment = TextAlignment.Center;
            tm.color = textColor;
            tm.fontStyle = FontStyle.Bold;

            // Sleek dark midnight-blue HUD badge background for 100% readability against any lab environment
            GameObject bgObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
            bgObj.name = "BadgeBackground";
            bgObj.transform.SetParent(hint.transform, false);
            bgObj.transform.localPosition = Vector3.zero;
            float exactTextWidth = 0f;
            var textRenderer = tm.GetComponent<Renderer>();
            if (textRenderer != null) exactTextWidth = textRenderer.bounds.size.x;
            float panelWidth = Mathf.Max(0.6f, Mathf.Max(exactTextWidth + 0.3f, text.Length * 0.028f + 0.2f));
            bgObj.transform.localScale = new Vector3(panelWidth, 0.13f, 0.006f);
            UnityEngine.Object.DestroyImmediate(bgObj.GetComponent<Collider>());

            Material bgMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            if (bgMat.shader == null || !bgMat.shader.isSupported) bgMat = new Material(Shader.Find("Standard"));
            bgMat.color = new Color(0.04f, 0.07f, 0.15f, 0.90f); // Sleek high-contrast dark badge
            bgMat.SetFloat("_Surface", 1);
            bgMat.SetFloat("_Blend", 0);
            bgMat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            bgMat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            bgMat.SetInt("_ZWrite", 0);
            bgMat.renderQueue = 3000;
            bgObj.GetComponent<MeshRenderer>().sharedMaterial = bgMat;

            hint.AddComponent<UniversalBadgeBillboard>();

            return hint;
        }

        public static IEnumerator FadeAndDestroyBadge(GameObject target, float duration = 0.8f)
        {
            if (target == null) yield break;
            
            TextMesh[] texts = target.GetComponentsInChildren<TextMesh>();
            MeshRenderer[] renderers = target.GetComponentsInChildren<MeshRenderer>();
            float elapsed = 0f;
            
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
            if (target != null) UnityEngine.Object.Destroy(target);
        }

        public static void CleanAllBadges()
        {
            foreach (var obj in UnityEngine.Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None))
            {
                if (obj != null && (obj.name.StartsWith("TutorialBadge_") || obj.name.StartsWith("TutorialHint_")))
                {
                    UnityEngine.Object.Destroy(obj);
                }
            }
        }
    }
}
