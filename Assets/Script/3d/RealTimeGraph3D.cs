using System.Collections.Generic;
using UnityEngine;

public class RealTimeGraph3D : MonoBehaviour
{
    [Header("Line Renderers (Assigned or Auto-Found)")]
    public LineRenderer lineOxygen;
    public LineRenderer lineHydrogen;

    [Header("Graphing Limits")]
    public float maxTime = 8.0f;       // Matches the electrolysis duration
    public float maxVolume = 10.0f;    // Maximum volume (Hydrogen peak)

    [Header("Screen Mapping Boundaries (Local Space Meters)")]
    public float xMin = -0.13f;
    public float xMax = 0.13f;
    public float yMin = -0.09f;
    public float yMax = 0.09f;

    private List<Vector3> oxygenPoints = new List<Vector3>();
    private List<Vector3> hydrogenPoints = new List<Vector3>();

    void Start()
    {
        // Try to find line renderers in children if not assigned
        if (lineOxygen == null)
        {
            Transform t = transform.Find("Line_Oxygen");
            if (t != null) lineOxygen = t.GetComponent<LineRenderer>();
        }

        if (lineHydrogen == null)
        {
            Transform t = transform.Find("Line_Hydrogen");
            if (t != null) lineHydrogen = t.GetComponent<LineRenderer>();
        }

        // Configure Line Renderers to use local space drawing
        ConfigureLineRenderer(lineOxygen);
        ConfigureLineRenderer(lineHydrogen);

        ResetGraph();
    }

    private void ConfigureLineRenderer(LineRenderer lr)
    {
        if (lr != null)
        {
            lr.useWorldSpace = false;
            lr.positionCount = 0;
            lr.startWidth = 0.006f;
            lr.endWidth = 0.006f;
            lr.numCornerVertices = 4;
            lr.numCapVertices = 4;

            // Ensure material uses a LineRenderer-compatible shader
            // URP Unlit/Lit shaders do NOT render correctly on LineRenderers
            if (lr.sharedMaterial != null)
            {
                string shaderName = lr.sharedMaterial.shader.name;
                if (shaderName.Contains("Universal Render Pipeline") || shaderName.Contains("Standard"))
                {
                    Color originalColor = lr.sharedMaterial.color;
                    Shader spriteShader = Shader.Find("Sprites/Default");
                    if (spriteShader != null)
                    {
                        lr.material = new Material(spriteShader);
                        lr.material.color = originalColor;
                    }
                }
            }
        }
    }

    public void ResetGraph()
    {
        oxygenPoints.Clear();
        hydrogenPoints.Clear();

        if (lineOxygen != null) lineOxygen.positionCount = 0;
        if (lineHydrogen != null) lineHydrogen.positionCount = 0;
    }

    public void AddDataPoint(float time, float o2Vol, float h2Vol)
    {
        if (oxygenPoints.Count == 0)
            Debug.Log($"[RealTimeGraph3D] First data point received: time={time:F2}, O2={o2Vol:F2}, H2={h2Vol:F2}");

        // 1. Oxygen (Anode)
        Vector3 o2Local = MapDataToLocal(time, o2Vol);
        oxygenPoints.Add(o2Local);
        if (lineOxygen != null)
        {
            lineOxygen.positionCount = oxygenPoints.Count;
            lineOxygen.SetPosition(oxygenPoints.Count - 1, o2Local);
        }

        // 2. Hydrogen (Cathode)
        Vector3 h2Local = MapDataToLocal(time, h2Vol);
        hydrogenPoints.Add(h2Local);
        if (lineHydrogen != null)
        {
            lineHydrogen.positionCount = hydrogenPoints.Count;
            lineHydrogen.SetPosition(hydrogenPoints.Count - 1, h2Local);
        }
    }

    private Vector3 MapDataToLocal(float time, float volume)
    {
        // Map time (0 to maxTime) to [xMin, xMax]
        float tNormal = Mathf.Clamp01(time / maxTime);
        float localX = Mathf.Lerp(xMin, xMax, tNormal);

        // Map volume (0 to maxVolume) to [yMin, yMax]
        float vNormal = Mathf.Clamp01(volume / maxVolume);
        float localY = Mathf.Lerp(yMin, yMax, vNormal);

        // Offset slightly in local Z to sit in front of the screen mesh
        return new Vector3(localX, localY, -0.01f);
    }
}
