using System.Collections.Generic;
using UnityEngine;

public class RealTimeTempGraph3D : MonoBehaviour
{
    [Header("Line Renderers (Assigned or Auto-Found)")]
    public LineRenderer lineBeakerA;
    public LineRenderer lineBeakerB;

    [Header("Graphing Limits")]
    public float maxTime = 30.0f;       // Matches the water heating duration
    public float maxTemp = 110.0f;      // Maximum temperature scale

    [Header("Screen Mapping Boundaries (Local Space Meters)")]
    public float xMin = -0.13f;
    public float xMax = 0.13f;
    public float yMin = 0.26f;          // Adjusted offset to match the screen mesh
    public float yMax = 0.44f;

    private List<Vector3> pointsA = new List<Vector3>();
    private List<Vector3> pointsB = new List<Vector3>();

    void Start()
    {
        // Try to find line renderers in children if not assigned
        if (lineBeakerA == null)
        {
            Transform t = transform.Find("Line_BeakerA");
            if (t != null) lineBeakerA = t.GetComponent<LineRenderer>();
        }

        if (lineBeakerB == null)
        {
            Transform t = transform.Find("Line_BeakerB");
            if (t != null) lineBeakerB = t.GetComponent<LineRenderer>();
        }

        // Configure Line Renderers to use local space drawing
        ConfigureLineRenderer(lineBeakerA);
        ConfigureLineRenderer(lineBeakerB);

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
        pointsA.Clear();
        pointsB.Clear();

        if (lineBeakerA != null) lineBeakerA.positionCount = 0;
        if (lineBeakerB != null) lineBeakerB.positionCount = 0;
    }

    public void AddDataPoint(float time, float tempA, float tempB)
    {
        if (time > maxTime) return;

        // 1. Beaker A (Pure Water)
        Vector3 localA = MapDataToLocal(time, tempA);
        pointsA.Add(localA);
        if (lineBeakerA != null)
        {
            lineBeakerA.positionCount = pointsA.Count;
            lineBeakerA.SetPosition(pointsA.Count - 1, localA);
        }

        // 2. Beaker B (Impure Water)
        Vector3 localB = MapDataToLocal(time, tempB);
        pointsB.Add(localB);
        if (lineBeakerB != null)
        {
            lineBeakerB.positionCount = pointsB.Count;
            lineBeakerB.SetPosition(pointsB.Count - 1, localB);
        }
    }

    private Vector3 MapDataToLocal(float time, float temp)
    {
        // Map time (0 to maxTime) to [xMin, xMax]
        float tNormal = Mathf.Clamp01(time / maxTime);
        float localX = Mathf.Lerp(xMin, xMax, tNormal);

        // Map temperature (0 to maxTemp) to [yMin, yMax]
        float tempNormal = Mathf.Clamp01(temp / maxTemp);
        float localY = Mathf.Lerp(yMin, yMax, tempNormal);

        // Offset slightly in local Z to sit in front of the screen mesh
        return new Vector3(localX, localY, -0.01f);
    }
}
