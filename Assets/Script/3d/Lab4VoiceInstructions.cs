using UnityEngine;
using UnityEngine.UI;
using TMPro;

// Scene-local playback only; never changes experiment or saved progress.
public sealed class Lab4VoiceInstructions : MonoBehaviour
{
    public AudioSource voice;
    public AudioClip[] clips = new AudioClip[3];
    public TMP_Text transcript, status, muteLabel;
    public Button replayButton, stopButton, muteButton;
    public GameObject loadingOverlay;
    public static readonly string[] Lines = {
        "Mari mulakan eksperimen penyejatan. Klik suis pemanas untuk menghidupkannya.",
        "Laraskan faktor penyejatan menggunakan kawalan yang disediakan. Mulakan eksperimen dan perhatikan perubahan sehingga eksperimen selesai.",
        "Buka buku amali. Rekodkan pemerhatian anda dan jawab kuiz berdasarkan hasil eksperimen."
    };
    public int DisplayedCue { get; private set; } = -1;
    public bool Muted { get; private set; }
    bool pending;
    bool Ready => loadingOverlay == null || !loadingOverlay.activeInHierarchy;
    bool HasClip => DisplayedCue >= 0 && clips != null && DisplayedCue < clips.Length && clips[DisplayedCue] != null;
    void Awake() { voice.playOnAwake=false; voice.loop=false; voice.spatialBlend=0; }
    void OnEnable() { replayButton.onClick.AddListener(Replay); stopButton.onClick.AddListener(StopVoice); muteButton.onClick.AddListener(ToggleMute); }
    void OnDisable() { StopVoice(); replayButton.onClick.RemoveListener(Replay); stopButton.onClick.RemoveListener(StopVoice); muteButton.onClick.RemoveListener(ToggleMute); }
    void Update()
    {
        if(pending && Ready) { pending=false; Replay(); }
        replayButton.interactable=Ready && HasClip && !Muted;
        stopButton.interactable=voice != null && voice.isPlaying;
        muteLabel.text=Muted ? "Suara: Senyap" : "Suara: Hidup";
        status.text=DisplayedCue<0 ? "Bersedia..." : !HasClip ? "Rakaman suara belum tersedia" : Muted ? "Suara disenyapkan" : voice.isPlaying ? "Sedang membaca..." : "Tekan Ulang Arahan untuk mendengar semula";
    }
    public void Present(int cue)
    {
        if(cue<0 || cue>=Lines.Length) return;
        StopVoice(); DisplayedCue=cue;
        transcript.text=(cue<3 ? "LANGKAH "+(cue+1)+" / 3\n" : "SELESAI\n")+Lines[cue];
        pending=true;
    }
    public void Replay() { if(!Ready || Muted || !HasClip || voice==null)return; voice.Stop(); voice.clip=clips[DisplayedCue]; voice.Play(); }
    public void StopVoice() { pending=false; if(voice!=null)voice.Stop(); }
    public void ToggleMute() { Muted=!Muted; if(Muted)StopVoice(); }
    public void ResetPresentation() { StopVoice(); DisplayedCue=-1; transcript.text="Bersedia untuk eksperimen penyejatan..."; }
}


