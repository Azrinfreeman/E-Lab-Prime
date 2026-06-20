using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class RekodEksperimen
{
    public float panjangBenang;
    public float masaAyunan;
}

[System.Serializable]
public class TableRowUI
{
    public float targetLength; 
    public TMPro.TextMeshProUGUI time10SwingsText;
    public TMPro.TextMeshProUGUI periodText;       
    public TMPro.TextMeshProUGUI periodSquaredText; 
}

public class PengurusDataAmali : MonoBehaviour
{
    [Header("Rujukan Skrip Lain")]
    public PengawalPanjangTali skripPanjangTali;
    public JamRandik skripJamRandik;

    [Header("Pangkalan Data Makmal")]
    public List<RekodEksperimen> senaraiDataEksperimen = new List<RekodEksperimen>();

    [Header("Jadual Keputusan")]
    public List<TableRowUI> tableRows = new List<TableRowUI>();

    public void SimpanDataSemasa()
    {
        if (skripPanjangTali == null || skripJamRandik == null)
            return;

        if (skripJamRandik.DapatkanJumlahAyunan() < 10)
        {
            Debug.LogWarning("Eksperimen belum selesai! Sila tunggu sehingga 10 ayunan.");
            return;
        }

        float panjangSemasa = skripPanjangTali.sliderPanjang.value;
        float masaSemasa = skripJamRandik.DapatkanMasaSemasa();

        RekodEksperimen rekodBaru = new RekodEksperimen();
        rekodBaru.panjangBenang = panjangSemasa;
        rekodBaru.masaAyunan = masaSemasa;

        senaraiDataEksperimen.Add(rekodBaru);

        Debug.Log(
            "DATA BERJAYA DISIMPAN! Panjang: "
                + rekodBaru.panjangBenang
                + "cm, Masa: "
                + rekodBaru.masaAyunan
                + "s"
        );

        foreach (var row in tableRows)
        {
            if (Mathf.Abs(row.targetLength - panjangSemasa) < 2f)
            {
                float t = masaSemasa;
                float T = t / 10f;
                float T2 = T * T;

                if (row.time10SwingsText != null) row.time10SwingsText.text = t.ToString("F2");
                if (row.periodText != null) row.periodText.text = T.ToString("F2");
                if (row.periodSquaredText != null) row.periodSquaredText.text = T2.ToString("F3");
                break;
            }
        }
    }

    // --- KOD BAHARU DI SINI ---
    // Fungsi ini akan mengosongkan senarai rekod supaya pelajar boleh mula semula
    public void PadamSemuaData()
    {
        // 1. Kosongkan memori senarai eksperimen
        senaraiDataEksperimen.Clear();

        // 2. Padamkan juga paparan jam dan ayunan di skrin supaya bersih
        if (skripJamRandik != null)
        {
            skripJamRandik.ResetMasa();
        }

        Debug.Log("SEMUA DATA TELAH DIPADAM! Sistem bersedia untuk eksperimen baharu.");

        foreach (var row in tableRows)
        {
            if (row.time10SwingsText != null) row.time10SwingsText.text = "-";
            if (row.periodText != null) row.periodText.text = "-";
            if (row.periodSquaredText != null) row.periodSquaredText.text = "-";
        }
    }
}
