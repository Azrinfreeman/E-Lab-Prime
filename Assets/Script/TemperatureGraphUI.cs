using UnityEngine;
using UnityEngine.UI;

public class TemperatureGraphUI : MonoBehaviour
{
    [Header("Graph UI References")]
    public RectTransform graphContainer;
    public GameObject pointPrefab; // Put your DotPrefab here

    [Header("Graph Settings")]
    public float yMaxTemp = 110f; // Max temperature on Y-axis
    public float xMaxTime = 30f;  // Max time on X-axis (in seconds)
    
    [Header("Line Colors")]
    public Color colorBeakerA = Color.cyan;
    public Color colorBeakerB = Color.red;

    private float graphWidth;
    private float graphHeight;

    private GameObject lastPointA;
    private GameObject lastPointB;

    void Start()
    {
// Use .rect.width and .rect.height instead of sizeDelta for accurate Canvas sizing
        graphWidth = graphContainer.rect.width;
        graphHeight = graphContainer.rect.height;
    }

    public void PlotPoint(float currentTime, float tempA, float tempB)
    {
        // Stop plotting if time exceeds the graph's max X-axis
        if (currentTime > xMaxTime) return;

        // Calculate X position
        float normalizedX = currentTime / xMaxTime;
        float xPos = normalizedX * graphWidth;

        // --- PLOT BEAKER A (PURE WATER) ---
        float normalizedYA = tempA / yMaxTemp;
        float yPosA = normalizedYA * graphHeight;
        Vector2 posA = new Vector2(xPos, yPosA);
        
        GameObject pointA = CreateDot(posA, colorBeakerA);
        if (lastPointA != null)
        {
            CreateConnection(lastPointA.GetComponent<RectTransform>().anchoredPosition, posA, colorBeakerA);
        }
        lastPointA = pointA;

        // --- PLOT BEAKER B (IMPURE WATER) ---
        float normalizedYB = tempB / yMaxTemp;
        float yPosB = normalizedYB * graphHeight;
        Vector2 posB = new Vector2(xPos, yPosB);
        
        GameObject pointB = CreateDot(posB, colorBeakerB);
        if (lastPointB != null)
        {
            CreateConnection(lastPointB.GetComponent<RectTransform>().anchoredPosition, posB, colorBeakerB);
        }
        lastPointB = pointB;
    }

    private GameObject CreateDot(Vector2 anchoredPosition, Color dotColor)
    {
        GameObject dot = Instantiate(pointPrefab, graphContainer);
        RectTransform rect = dot.GetComponent<RectTransform>();
        
        rect.anchorMin = new Vector2(0, 0);
        rect.anchorMax = new Vector2(0, 0);
        rect.anchoredPosition = anchoredPosition;
        
        dot.GetComponent<Image>().color = dotColor;
        return dot;
    }

    private void CreateConnection(Vector2 dotPositionA, Vector2 dotPositionB, Color lineColor)
    {
        GameObject link = new GameObject("GraphLine", typeof(Image));
        link.transform.SetParent(graphContainer, false);
        link.GetComponent<Image>().color = lineColor;

        RectTransform rect = link.GetComponent<RectTransform>();
        Vector2 dir = (dotPositionB - dotPositionA).normalized;
        float distance = Vector2.Distance(dotPositionA, dotPositionB);

        rect.anchorMin = new Vector2(0, 0);
        rect.anchorMax = new Vector2(0, 0);
        rect.sizeDelta = new Vector2(distance, 2f); 
        rect.anchoredPosition = dotPositionA + dir * distance * 0.5f;
        
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        rect.localEulerAngles = new Vector3(0, 0, angle);
        
        link.transform.SetSiblingIndex(0); 
    }
    
    public void ResetGraph()
    {
        foreach (Transform child in graphContainer)
        {
            Destroy(child.gameObject);
        }
        
        lastPointA = null;
        lastPointB = null;
    }
}