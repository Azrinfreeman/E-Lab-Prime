using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PengurusGraf : MonoBehaviour
{
    [Header("Rujukan")]
    // GANTIKAN rujukan lama dengan PengurusDataAmali
    public PengurusDataAmali pengurusData;
    public GameObject graphWindow;

    [Header("Komponen UI Graf")]
    public List<RectTransform> graphDots;
    public List<RectTransform> connectingLines;

    [Header("Talaan Graf")]
    public float yScaleMultiplier = 100f;

    void Start()
    {
        if (graphWindow != null)
            graphWindow.SetActive(false);
    }

    public void GenerateGraph()
    {
        if (pengurusData == null)
        {
            Debug.LogError("RALAT: Slot Pengurus Data kosong di Inspector!");
            return;
        }
        if (graphDots.Count < 5 || connectingLines.Count < 4)
        {
            Debug.LogError("RALAT: Jumlah Dots atau Garisan tidak cukup 5/4 di Inspector!");
            return;
        }

        List<RekodEksperimen> dataEksperimen = pengurusData.senaraiDataEksperimen;

        // MESEJ PENTING: Semak berapa banyak data yang dihantar ke graf
        Debug.Log("Graf sedang dilukis! Jumlah data yang dikesan: " + dataEksperimen.Count);

        dataEksperimen.Sort((a, b) => a.panjangBenang.CompareTo(b.panjangBenang));

        // Matikan semua dot dan garisan dahulu
        foreach (var dot in graphDots) dot.gameObject.SetActive(false);
        foreach (var line in connectingLines) line.gameObject.SetActive(false);

        List<RectTransform> activeDots = new List<RectTransform>();

        for (int i = 0; i < dataEksperimen.Count; i++)
        {
            var data = dataEksperimen[i];
            int dotIndex = -1;
            
            if (Mathf.Abs(data.panjangBenang - 20) < 2f) dotIndex = 0;
            else if (Mathf.Abs(data.panjangBenang - 40) < 2f) dotIndex = 1;
            else if (Mathf.Abs(data.panjangBenang - 60) < 2f) dotIndex = 2;
            else if (Mathf.Abs(data.panjangBenang - 80) < 2f) dotIndex = 3;
            else if (Mathf.Abs(data.panjangBenang - 100) < 2f) dotIndex = 4;

            if (dotIndex != -1 && dotIndex < graphDots.Count)
            {
                var dot = graphDots[dotIndex];
                dot.gameObject.SetActive(true);

                float t = data.masaAyunan;
                float T = t / 10f;
                float t2Value = T * T;

                float localY = t2Value * yScaleMultiplier;
                dot.anchoredPosition = new Vector2(dot.anchoredPosition.x, localY);

                TextMeshProUGUI dotText = dot.GetComponentInChildren<TextMeshProUGUI>();
                if (dotText != null) dotText.text = t2Value.ToString("F2");

                activeDots.Add(dot);
            }
        }

        // Sambung titik yang aktif
        for (int i = 0; i < activeDots.Count - 1; i++)
        {
            if (i < connectingLines.Count)
            {
                var line = connectingLines[i];
                line.gameObject.SetActive(true);

                Vector2 posA = activeDots[i].anchoredPosition;
                Vector2 posB = activeDots[i + 1].anchoredPosition;
                line.anchoredPosition = posA;

                Vector2 direction = posB - posA;
                float distance = direction.magnitude;
                float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

                line.sizeDelta = new Vector2(distance, line.sizeDelta.y);
                line.localEulerAngles = new Vector3(0f, 0f, angle);
            }
        }

        if (graphWindow != null)
            graphWindow.SetActive(true);
    }

    public void CloseGraphWindow()
    {
        if (graphWindow != null)
            graphWindow.SetActive(false);
    }
}
