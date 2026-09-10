using System.Collections.Generic;
using UnityEngine;

public class SolubilityGraph3D : MonoBehaviour
{
    [Header("Graphing Limits")]
    public float maxTime = 20.0f;       
    public float maxSugar = 10.0f;    

    [Header("Screen Mapping Boundaries (Local Space relative to Container)")]
    public float xMin = -0.05f;
    public float xMax = 0.05f;
    public float zMin = -0.12f;
    public float zMax = 0.08f;
    public float yOffset = 0.003f;      

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
        ResetGraph();
    }

    public void ResetGraph()
    {
        dataPoints.Clear();
        maxTime = 10.0f; 
        if (graphLine != null) graphLine.positionCount = 0;
    }

    public void FinishGraph(float finalTime)
    {
        if (finalTime > 0.1f)
        {
            maxTime = Mathf.Max(5.0f, finalTime);
            RedrawLine();
        }
    }

    public void AddDataPoint(float time, float sugarLeft)
    {
        dataPoints.Add(new KeyValuePair<float, float>(time, sugarLeft));

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
                graphLine.SetPosition(dataPoints.Count - 1, MapDataToLocal(time, sugarLeft));
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

    private Vector3 MapDataToLocal(float time, float sugarLeft)
    {
        float tNormal = Mathf.Clamp01(time / maxTime);
        float localX = Mathf.Lerp(xMin, xMax, tNormal);

        float sugarNormal = Mathf.Clamp01(sugarLeft / maxSugar);
        float localZ = Mathf.Lerp(zMin, zMax, sugarNormal);

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
            ConfigureLineRenderer(graphLine, new Color(0.1f, 0.6f, 0.2f), 0.003f); 
        }

        // 2. Setup Axes Line Renderer
        if (axesLine == null)
        {
            GameObject axesObj = new GameObject("AxesLine");
            axesObj.transform.SetParent(transform, false);
            axesLine = axesObj.AddComponent<LineRenderer>();
            ConfigureLineRenderer(axesLine, Color.black, 0.002f); 

            axesLine.positionCount = 3;
            axesLine.SetPosition(0, new Vector3(xMin, yOffset, zMax)); 
            axesLine.SetPosition(1, new Vector3(xMin, yOffset, zMin)); 
            axesLine.SetPosition(2, new Vector3(xMax, yOffset, zMin)); 
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
            titleText.text = "Graf Gula (g) vs Masa (s)";
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
            yObj.transform.localRotation = Quaternion.Euler(90f, 90f, 0f); 
            yObj.transform.localScale = new Vector3(0.0008f, 0.0008f, 0.0008f);

            yLabel = yObj.AddComponent<TextMesh>();
            yLabel.text = "Gula Tertinggal (g)";
            yLabel.fontSize = 20;
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
        lr.positionCount = 0;
        
        Shader spriteShader = Shader.Find("Sprites/Default");
        if (spriteShader != null)
        {
            lr.material = new Material(spriteShader);
            lr.material.color = color;
        }
    }
}
