using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro; // PENTING: Tambah baris ini untuk guna fungsi Teks!

public class GraphManager : MonoBehaviour
{
    [Header("References")]
    public PendulumInteraction pendulumScript; 
    public GameObject graphWindow;             

    [Header("Graph UI Components")]
    public List<RectTransform> graphDots;      
    public List<RectTransform> connectingLines; 

    [Header("Graph Tuning")]
    public float yScaleMultiplier = 100f; // Ingat untuk guna nilai yang sesuai (cth: 100)

    void Start()
    {
        if (graphWindow != null) graphWindow.SetActive(false);
    }

    public void GenerateGraph()
    {
        if (pendulumScript == null || graphDots.Count < 5 || connectingLines.Count < 4) return;

        // 1. POSITION THE DOTS & UPDATE TEXT
        for (int i = 0; i < graphDots.Count; i++)
        {
            float t2Value = 0f;

            if (pendulumScript.tableRows != null && i < pendulumScript.tableRows.Count)
            {
                float t = pendulumScript.tableRows[i].time10SwingsText.text != "-" ? float.Parse(pendulumScript.tableRows[i].time10SwingsText.text) : 0f;
                float T = t / 10f;
                t2Value = T * T; 
            }

            float localY = t2Value * yScaleMultiplier;
            graphDots[i].anchoredPosition = new Vector2(graphDots[i].anchoredPosition.x, localY);

            // --- KOD BAHARU: TULIS NOMBOR PADA TITIK ---
            // Kod ini akan mencari Teks yang berada di dalam Dot tersebut dan menulis nilai T²
            TextMeshProUGUI dotText = graphDots[i].GetComponentInChildren<TextMeshProUGUI>();
            if (dotText != null)
            {
                if (t2Value == 0f) 
                    dotText.text = ""; // Kosongkan teks jika amali belum dibuat
                else 
                    dotText.text = t2Value.ToString("F2"); // Tulis nombor dengan 2 titik perpuluhan
            }
        }

        // 2. STRETCH AND ROTATE LINES TO CONNECT THE DOTS
        for (int i = 0; i < connectingLines.Count; i++)
        {
            Vector2 posA = graphDots[i].anchoredPosition;
            Vector2 posB = graphDots[i + 1].anchoredPosition;

            connectingLines[i].anchoredPosition = posA;

            Vector2 direction = posB - posA;
            float distance = direction.magnitude;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

            connectingLines[i].sizeDelta = new Vector2(distance, connectingLines[i].sizeDelta.y);
            connectingLines[i].localEulerAngles = new Vector3(0f, 0f, angle);
        }

        if (graphWindow != null) graphWindow.SetActive(true);
    }

    public void CloseGraphWindow()
    {
        if (graphWindow != null) graphWindow.SetActive(false);
    }
}