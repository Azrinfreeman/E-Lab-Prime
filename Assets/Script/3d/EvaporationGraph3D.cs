using System.Collections.Generic;
using UnityEngine;

public class EvaporationGraph3D : MonoBehaviour
{
    [Header("Graphing Limits")]
    public float maxTime = 20.0f;       // Initial time limit (X-axis), auto-scales if exceeded
    public float maxVolume = 100.0f;    // Initial volume limit (Y-axis, representing 100 ml)

    [Header("Screen Mapping Boundaries (Local Space relative to Container)")]
    public float xMin = -0.05f;
    public float xMax = 0.05f;
    public float zMin = -0.12f;
    public float zMax = 0.08f;
    public float yOffset = 0.003f;      // Sit slightly above the page surface to prevent z-fighting

    [Header("References")]
    public LineRenderer graphLine;
    public LineRenderer axesLine;
    public TextMesh titleText;
    public TextMesh yLabel;
    public TextMesh xLabel;

    private List<KeyValuePair<float, float>> dataPoints = new List<KeyValuePair<float, float>>();

    void Awake()
    {
        CreateGraphElements();
    }

    public void ResetGraph()
    {
        dataPoints.Clear();
        maxTime = 20.0f; // Reset to default limit
        if (graphLine != null) graphLine.positionCount = 0;
    }

    public void AddDataPoint(float time, float volume)
    {
        dataPoints.Add(new KeyValuePair<float, float>(time, volume));

        bool needRedrawAll = false;
        if (time > maxTime)
        {
            maxTime = time;
            needRedrawAll = true;
        }

        if (needRedrawAll)
        {
            RedrawLine();
        }
        else
        {
            if (graphLine != null)
            {
                graphLine.positionCount = dataPoints.Count;
                graphLine.SetPosition(dataPoints.Count - 1, MapDataToLocal(time, volume));
            }
        }
    }

    private void RedrawLine()
    {
        if (graphLine == null) return;

        graphLine.positionCount = dataPoints.Count;
        for (int i = 0; i < dataPoints.Count; i++)
        {
            graphLine.SetPosition(i, MapDataToLocal(dataPoints[i].Key, dataPoints[i].Value));
        }
    }

    private Vector3 MapDataToLocal(float time, float volume)
    {
        float tNormal = Mathf.Clamp01(time / maxTime);
        float localX = Mathf.Lerp(xMin, xMax, tNormal);

        float volNormal = Mathf.Clamp01(volume / maxVolume);
        float localZ = Mathf.Lerp(zMin, zMax, volNormal);

        return new Vector3(localX, yOffset, localZ);
    }

    private void CreateGraphElements()
    {
        // 1. Setup Graph Line Renderer
        if (graphLine == null)
        {
            GameObject lineObj = new GameObject("GraphLine");
            lineObj.transform.SetParent(transform, false);
            graphLine = lineObj.AddComponent<LineRenderer>();
            ConfigureLineRenderer(graphLine, new Color(0.1f, 0.6f, 0.2f), 0.003f); // Bold green curve
        }

        // 2. Setup Axes Line Renderer
        if (axesLine == null)
        {
            GameObject axesObj = new GameObject("AxesLine");
            axesObj.transform.SetParent(transform, false);
            axesLine = axesObj.AddComponent<LineRenderer>();
            ConfigureLineRenderer(axesLine, Color.black, 0.002f); // Thin black axes

            // Draw Y-axis (from bottom-left to top-left) and X-axis (from bottom-left to bottom-right)
            axesLine.positionCount = 3;
            axesLine.SetPosition(0, new Vector3(xMin, yOffset, zMax)); // top of Y-axis
            axesLine.SetPosition(1, new Vector3(xMin, yOffset, zMin)); // origin
            axesLine.SetPosition(2, new Vector3(xMax, yOffset, zMin)); // end of X-axis
        }

        // 3. Setup Title & Axis Text Labels
        Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        Material fontMat = font != null ? font.material : null;

        if (titleText == null)
        {
            GameObject tObj = new GameObject("TitleText");
            tObj.transform.SetParent(transform, false);
            tObj.transform.localPosition = new Vector3(0f, yOffset, zMax + 0.02f);
            tObj.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            tObj.transform.localScale = new Vector3(0.001f, 0.001f, 0.001f);

            titleText = tObj.AddComponent<TextMesh>();
            titleText.text = "Graf Isipadu Air vs Masa";
            titleText.fontSize = 28;
            titleText.fontStyle = FontStyle.Bold;
            titleText.anchor = TextAnchor.MiddleCenter;
            titleText.alignment = TextAlignment.Center;
            titleText.color = Color.black;
            if (font != null) { titleText.font = font; tObj.GetComponent<MeshRenderer>().sharedMaterial = fontMat; }
        }

        if (yLabel == null)
        {
            GameObject yObj = new GameObject("YLabel");
            yObj.transform.SetParent(transform, false);
            yObj.transform.localPosition = new Vector3(xMin - 0.015f, yOffset, (zMin + zMax) * 0.5f);
            yObj.transform.localRotation = Quaternion.Euler(90f, 90f, 0f); // align vertically along Z axis
            yObj.transform.localScale = new Vector3(0.0008f, 0.0008f, 0.0008f);

            yLabel = yObj.AddComponent<TextMesh>();
            yLabel.text = "Isipadu (ml)";
            yLabel.fontSize = 24;
            yLabel.fontStyle = FontStyle.Bold;
            yLabel.anchor = TextAnchor.MiddleCenter;
            yLabel.alignment = TextAlignment.Center;
            yLabel.color = Color.black;
            if (font != null) { yLabel.font = font; yObj.GetComponent<MeshRenderer>().sharedMaterial = fontMat; }
        }

        if (xLabel == null)
        {
            GameObject xObj = new GameObject("XLabel");
            xObj.transform.SetParent(transform, false);
            xObj.transform.localPosition = new Vector3(0f, yOffset, zMin - 0.015f);
            xObj.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            xObj.transform.localScale = new Vector3(0.0008f, 0.0008f, 0.0008f);

            xLabel = xObj.AddComponent<TextMesh>();
            xLabel.text = "Masa (s)";
            xLabel.fontSize = 24;
            xLabel.fontStyle = FontStyle.Bold;
            xLabel.anchor = TextAnchor.MiddleCenter;
            xLabel.alignment = TextAlignment.Center;
            xLabel.color = Color.black;
            if (font != null) { xLabel.font = font; xObj.GetComponent<MeshRenderer>().sharedMaterial = fontMat; }
        }
    }

    private void ConfigureLineRenderer(LineRenderer lr, Color color, float width)
    {
        lr.useWorldSpace = false;
        lr.startWidth = width;
        lr.endWidth = width;
        lr.numCornerVertices = 4;
        lr.numCapVertices = 4;
        
        Shader spriteShader = Shader.Find("Sprites/Default");
        if (spriteShader != null)
        {
            lr.material = new Material(spriteShader);
            lr.material.color = color;
        }
    }
}
