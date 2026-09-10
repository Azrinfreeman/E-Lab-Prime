using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Lab5IntroCamera : MonoBehaviour
{
    public Vector3 targetPosition = new Vector3(0, 0.95f, -1.8f);
    public Quaternion targetRotation = Quaternion.Euler(22f, 0f, 0f);
    public float duration = 2.5f;

    private CanvasGroup fadeGroup;

    void Start()
    {
        targetPosition = transform.position;
        targetRotation = transform.rotation;

        Vector3 startPos = targetPosition + new Vector3(0f, 1.2f, -1.5f);
        Quaternion startRot = Quaternion.Euler(38f, 0f, 0f);

        transform.position = startPos;
        transform.rotation = startRot;

        GameObject fadeObj = new GameObject("Lab5_IntroFadeCanvas");
        Canvas c = fadeObj.AddComponent<Canvas>();
        c.renderMode = RenderMode.ScreenSpaceOverlay;
        c.sortingOrder = 9999;
        fadeObj.AddComponent<GraphicRaycaster>();
        
        GameObject quad = new GameObject("FadeQuad");
        quad.transform.SetParent(fadeObj.transform, false);
        Image img = quad.AddComponent<Image>();
        img.color = Color.black;
        RectTransform rect = quad.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.sizeDelta = Vector2.zero;

        fadeGroup = fadeObj.AddComponent<CanvasGroup>();
        fadeGroup.alpha = 1.0f;
        fadeGroup.blocksRaycasts = true;

        StartCoroutine(PlayIntroCoroutine(startPos, startRot, fadeObj));
    }

    private IEnumerator PlayIntroCoroutine(Vector3 startPos, Quaternion startRot, GameObject fadeObj)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float curve = t * t * (3f - 2f * t);

            transform.position = Vector3.Lerp(startPos, targetPosition, curve);
            transform.rotation = Quaternion.Slerp(startRot, targetRotation, curve);

            if (fadeGroup != null)
            {
                fadeGroup.alpha = Mathf.Clamp01(1f - (t / 0.6f));
                if (fadeGroup.alpha <= 0f) fadeGroup.blocksRaycasts = false;
            }

            yield return null;
        }

        transform.position = targetPosition;
        transform.rotation = targetRotation;

        if (fadeObj != null)
        {
            Destroy(fadeObj);
        }
        Destroy(this);
    }
}
