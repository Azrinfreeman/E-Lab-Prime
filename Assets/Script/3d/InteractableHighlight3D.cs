using UnityEngine;
using System.Collections.Generic;

public class InteractableHighlight3D : MonoBehaviour
{
    [Header("Highlight Outline Settings")]
    public Color highlightColor = new Color(0.0f, 0.8f, 1.0f, 0.4f); // Cyan glow
    public float scaleFactor = 1.12f; // Scale up factor for the outline shell

    private List<GameObject> glowShells = new List<GameObject>();
    private bool isHovering = false;

    private void Start()
    {
        CreateGlowShells();
    }

    private void OnMouseEnter()
    {
        isHovering = true;
        SetGlowActive(true);
    }

    private void OnMouseExit()
    {
        isHovering = false;
        SetGlowActive(false);
    }

    private void OnDisable()
    {
        if (isHovering)
        {
            OnMouseExit();
        }
    }

    private void OnDestroy()
    {
        foreach (var shell in glowShells)
        {
            if (shell != null)
            {
                Destroy(shell);
            }
        }
        glowShells.Clear();
    }

    private void SetGlowActive(bool active)
    {
        foreach (var shell in glowShells)
        {
            if (shell != null)
            {
                shell.SetActive(active);
            }
        }
    }

    private void CreateGlowShells()
    {
        MeshFilter[] filters = GetComponentsInChildren<MeshFilter>(true);
        if (filters == null || filters.Length == 0) return;

        // Create a dedicated outline material compatible with URP and Standard
        Material outlineMat = null;
        Shader urpUnlit = Shader.Find("Universal Render Pipeline/Unlit");
        if (urpUnlit != null)
        {
            outlineMat = new Material(urpUnlit);
            outlineMat.SetColor("_BaseColor", highlightColor);
            outlineMat.SetFloat("_Surface", 1f); // Transparent
            outlineMat.SetInt("_Blend", 0); // Alpha
            outlineMat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            outlineMat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            outlineMat.SetInt("_ZWrite", 0);
            outlineMat.SetFloat("_Cull", 1f); // Cull Front (draws back faces only)
        }
        else
        {
            Shader standardShader = Shader.Find("Standard");
            outlineMat = new Material(standardShader != null ? standardShader : Shader.Find("Sprites/Default"));
            outlineMat.color = highlightColor;
            if (outlineMat.HasProperty("_Mode")) outlineMat.SetFloat("_Mode", 3f); // Transparent
            outlineMat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            outlineMat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            outlineMat.SetInt("_ZWrite", 0);
            outlineMat.DisableKeyword("_ALPHATEST_ON");
            outlineMat.EnableKeyword("_ALPHABLEND_ON");
            outlineMat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            outlineMat.SetFloat("_Cull", 1f); // Cull Front
        }
        outlineMat.renderQueue = 3100;

        foreach (var filter in filters)
        {
            if (filter == null || filter.sharedMesh == null) continue;
            
            // Skip existing outline shells if script is re-run or starts in a weird state
            if (filter.gameObject.name == "GlowOutlineShell") continue;

            GameObject shell = new GameObject("GlowOutlineShell");
            shell.transform.SetParent(filter.transform, false);
            // Scale up slightly to sit outside the parent mesh
            shell.transform.localScale = new Vector3(scaleFactor, scaleFactor, scaleFactor);

            MeshFilter mf = shell.AddComponent<MeshFilter>();
            mf.sharedMesh = filter.sharedMesh;

            MeshRenderer mr = shell.AddComponent<MeshRenderer>();
            mr.sharedMaterial = outlineMat;
            mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            mr.receiveShadows = false;

            shell.SetActive(false); // Disabled by default
            glowShells.Add(shell);
        }
    }
}
