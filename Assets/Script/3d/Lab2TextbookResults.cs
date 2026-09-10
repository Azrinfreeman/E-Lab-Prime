using UnityEngine;
public sealed class Lab2TextbookResults : MonoBehaviour
{
    public TextMesh leftPage, rightPage;
    public const string Theory = "ELEKTROLISIS AIR\n\nTUJUAN\nMengkaji gas terhasil\napabila arus elektrik\nmengalir melalui air.\n\nELEKTROD\nKatod (-): hidrogen\nAnod (+): oksigen\n\nPERSAMAAN KESELURUHAN\n2H2O -> 2H2 + O2";
    public void ShowResults(bool completed)
    {
        if(leftPage!=null)leftPage.text=Theory;
        if(rightPage!=null)rightPage.text=(completed ? "HASIL EKSPERIMEN\nProses selesai.\n" : "JANGKAAN HASIL\nBelum selesai.\n")+"\nPEMERHATIAN\nGelembung gas terbentuk\npada kedua-dua elektrod.\n\nNISBAH ISI PADU\nHidrogen : Oksigen\n2 : 1 (nisbah teori)\n\nKESIMPULAN\nAir terurai kepada\nhidrogen dan oksigen.";
    }
    void Start(){ShowResults(false);}
}
