public static class ExperimentTextbookContent
{
    public const string Lab3Left = "PEMANASAN AIR\n\nTUJUAN\nBandingkan takat didih\nair tulen dan air garam.\n\nPEMERHATIAN\nSuhu naik semasa\npemanasan. Catat suhu\napabila air mendidih.\n\nKESIMPULAN DIJANGKA\nGaram menaikkan\ntakat didih air.";
    public const string Lab3Right = "REKOD TAKAT DIDIH\n\nBikar A (air tulen):\nBelum direkod.\nBikar B (air garam):\nBelum direkod.\n\nPANDUAN SIMULASI\nA: 100 C; B: 104 C.\nIni nilai rujukan,\nbukan bacaan anda.\n\nSemak termometer dan\nrekod melalui Log Data.";
    public const string Lab4Left = "PENYEJATAN AIR\n\nTUJUAN\nMengkaji faktor yang\nmengubah kadar\npenyejatan air.\n\nFAKTOR DIUJI\nSuhu, kelajuan angin,\nluas permukaan dan\nkelembapan udara.\n\nUbah satu faktor;\nkekalkan yang lain.";
    public const string Lab4Right = "JANGKAAN PEMERHATIAN\n\nAir berkurang apabila\npenyejatan berlaku.\n\nLEBIH CEPAT APABILA\nSuhu lebih tinggi,\nangin lebih laju,\npermukaan lebih luas,\nkelembapan lebih rendah.\n\nBANDINGKAN HASIL\nSemak kadar pada tablet\ndan masa yang diambil.\nCatat tetapan setiap ujian.";
    public const string Lab5Left = "PELARUTAN GULA\n\nTUJUAN\nMengkaji faktor yang\nmengubah kadar gula\nmelarut dalam air.\n\nFAKTOR DIUJI\nSuhu air, kacauan\ndan saiz zarah gula.\n\nUbah satu faktor;\nkekalkan yang lain.";
    public const string Lab5Right = "JANGKAAN PEMERHATIAN\n\nGula semakin berkurang\napabila melarut.\n\nLEBIH CEPAT APABILA\nAir lebih panas,\nkacauan lebih laju,\nzarah gula lebih kecil.\n\nKESIMPULAN\nFaktor ini mengubah\nkadar pelarutan, bukan\nsekadar jumlah gula.\nBandingkan masa ujian.";
    public static string Left(int lab) => lab==3 ? Lab3Left : lab==4 ? Lab4Left : Lab5Left;
    public static string Right(int lab) => lab==3 ? Lab3Right : lab==4 ? Lab4Right : Lab5Right;
    public static string Recorded(int lab) => "CATATAN DIREKOD\n" + Right(lab);
}
