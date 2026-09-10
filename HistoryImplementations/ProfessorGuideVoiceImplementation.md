# Professor Guide Voice Audio Implementation

## Goal

Generate real professor voice audio for the existing 2D professor guide in the main menu, replacing the current silent placeholder `voice_*.wav` files.

The professor guide already supports voice clips. The task is mostly asset generation and validation, not a full UI rebuild.

## Current State

Existing voice placeholder folder:

```text
Assets/Branding/ProfessorGuide/Audio/
```

Current silent placeholder clips:

```text
voice_welcome_ms.wav
voice_signedout_ms.wav
voice_signedin_ms.wav
voice_start_ms.wav
voice_settings_ms.wav
voice_achievements_ms.wav
```

Existing note:

```text
VOICE_PLACEHOLDERS.txt
```

Current behavior:

- `MainMenuProfessorGuide.cs` already plays `line.voiceClip`.
- `MainMenuProfessorGuideSetup.cs` already assigns each `voice_*.wav` file to the correct dialogue line.
- The hover dialog repeat rule is already protected by `playedHoverPrompts`, so generated voice should follow the same one-time hover behavior.

## Recommended Voice Direction

Use a warm male Malay professor voice:

- Calm.
- Clear.
- Slightly formal.
- Friendly teacher tone.
- Moderate pace.
- No exaggerated cartoon delivery.
- Suitable for Form 1 science lab context.

Style reference:

```text
Malay science teacher, supportive, confident, classroom-friendly, clear pronunciation.
```

## Dialogue Clip Map

### 1. Welcome

File:

```text
Assets/Branding/ProfessorGuide/Audio/voice_welcome_ms.wav
```

Malay:

```text
Selamat datang ke Makmal Sains Digital.
```

English subtitle:

```text
[Welcome to the Digital Science Lab.]
```

Suggested duration:

```text
2.0s - 2.8s
```

### 2. Signed Out Prompt

File:

```text
Assets/Branding/ProfessorGuide/Audio/voice_signedout_ms.wav
```

Malay:

```text
Log masuk dahulu supaya kemajuan eksperimen anda dapat disimpan.
```

English subtitle:

```text
[Sign in first so your experiment progress can be saved.]
```

Suggested duration:

```text
4.0s - 5.2s
```

### 3. Signed In Welcome

File:

```text
Assets/Branding/ProfessorGuide/Audio/voice_signedin_ms.wav
```

Malay:

```text
Selamat kembali. Mari sambung penerokaan sains anda.
```

English subtitle:

```text
[Welcome back. Let us continue your science exploration.]
```

Suggested duration:

```text
3.2s - 4.4s
```

### 4. Start Experiments

File:

```text
Assets/Branding/ProfessorGuide/Audio/voice_start_ms.wav
```

Malay:

```text
Tekan Mulakan Amali untuk memilih eksperimen sains anda.
```

English subtitle:

```text
[Press Start Experiments to choose your science activity.]
```

Suggested duration:

```text
3.5s - 4.8s
```

### 5. Settings

File:

```text
Assets/Branding/ProfessorGuide/Audio/voice_settings_ms.wav
```

Malay:

```text
Laraskan bunyi, paparan, dan pengalaman pembelajaran di sini.
```

English subtitle:

```text
[Adjust sound, display, and learning experience here.]
```

Suggested duration:

```text
4.0s - 5.0s
```

### 6. Achievements

File:

```text
Assets/Branding/ProfessorGuide/Audio/voice_achievements_ms.wav
```

Malay:

```text
Di sini anda boleh melihat pencapaian dan kemajuan setiap amali.
```

English subtitle:

```text
[Here you can view achievements and progress for each experiment.]
```

Suggested duration:

```text
4.2s - 5.4s
```

## Generation Options

## Option A: Local Windows Text-To-Speech

Recommended first attempt because it can run offline.

Process:

1. Use Windows speech synthesis if available.
2. Generate WAV files directly into `Assets/Branding/ProfessorGuide/Audio/`.
3. Keep the existing filenames.
4. Let Unity reimport the WAV files.
5. Rebuild or refresh the main menu professor guide setup.

Pros:

- No internet required.
- Fast.
- Fully local.
- Good enough for immediate testing.

Cons:

- Voice quality depends on installed Windows voices.
- Malay pronunciation may vary if no Malay voice is installed.
- May sound robotic compared to final production narration.

## Option B: Local Synthetic Placeholder Voice

Fallback if Windows TTS is unavailable.

Process:

1. Generate tonal syllable-like audio that follows the text rhythm.
2. Use a warm low-pitch voice-like oscillator, not silence.
3. Keep it clearly marked as placeholder.

Pros:

- Always possible with local Python.
- Better than silence for testing timing and audio routing.

Cons:

- Not real speech.
- Should not be final.

## Option C: High-Quality AI Voice

Best final production option if an audio-generation service/tool is available.

Process:

1. Generate each Malay line as a separate WAV.
2. Use the same filenames.
3. Normalize loudness.
4. Import into Unity.
5. Test in the main menu.

Pros:

- Most professional result.
- Natural teacher voice.
- Better pronunciation and emotion.

Cons:

- May require internet or API access.
- Needs licensing/usage clarity.
- Should use a generic generated voice, not a cloned real person unless permission is explicit.
### Option C Generator Script

This project now includes a ready-to-run OpenAI TTS generator:

```text
Tools/GenerateProfessorGuideVoiceOpenAI.ps1
```

It reads the API key from:

```text
OPENAI_API_KEY
```

It writes directly to:

```text
Assets/Branding/ProfessorGuide/Audio/
```

Dry run:

```powershell
.\Tools\GenerateProfessorGuideVoiceOpenAI.ps1 -DryRun
```

Generate WAV files:

```powershell
$env:OPENAI_API_KEY = "your_api_key_here"
.\Tools\GenerateProfessorGuideVoiceOpenAI.ps1
```

Default voice setup:

```text
Model: gpt-4o-mini-tts
Voice: cedar
Format: wav
```

OpenAI's current text-to-speech docs recommend `gpt-4o-mini-tts` for newer speech generation, support WAV output, and list `cedar` as one of the higher-quality built-in voices.

## File Format

Recommended Unity-friendly WAV format:

```text
Format: WAV PCM
Sample rate: 44100 Hz
Channels: Mono
Bit depth: 16-bit
Peak level: around -1 dB
Target loudness: roughly -16 LUFS to -18 LUFS
```

Mono is preferred because these are UI voice clips.

## Implementation Plan

### Phase 1: Generate Working Voice Clips

1. Generate the six Malay professor lines.
2. Save using the existing filenames:
   - `voice_welcome_ms.wav`
   - `voice_signedout_ms.wav`
   - `voice_signedin_ms.wav`
   - `voice_start_ms.wav`
   - `voice_settings_ms.wav`
   - `voice_achievements_ms.wav`
3. Replace the silent placeholders.
4. Keep the existing SFX:
   - `professor_appear.wav`
   - `professor_text_blip.wav`
   - `professor_highlight.wav`
   - `professor_confirm.wav`

### Phase 2: Import And Verify In Unity

1. Refresh Unity assets.
2. Confirm each WAV imports as an `AudioClip`.
3. Confirm `MainMenuProfessorGuideSetup.cs` still assigns the right clip to each dialogue line.
4. Run the setup menu if needed.
5. Open the main menu.
6. Verify:
   - Welcome voice plays.
   - Sign-in voice plays.
   - Hover voices play once per button per play session.
   - Text blips do not overpower voice.
   - Highlight SFX does not mask narration.

### Phase 3: Polish Playback

If needed, adjust:

- Voice volume from `0.8f` to `0.9f`.
- Text blip volume from `0.14f` down to `0.08f` while voice is playing.
- Typewriter speed to better match spoken timing.
- Dialogue auto-advance delay if voice is longer than text.

## Suggested Code Polish

Current voice playback:

```csharp
audioSource.PlayOneShot(line.voiceClip, 0.8f);
```

Potential improvement:

```csharp
private bool IsVoicePlaying()
{
    return audioSource != null && audioSource.isPlaying;
}
```

Then lower or skip text blips during narration:

```csharp
if (!IsVoicePlaying())
{
    audioSource.PlayOneShot(textBlipSfx, 0.14f);
}
```

However, this is optional. Phase 1 can keep existing code if the mix sounds okay.

## Asset Naming Rules

Do not rename the current voice files unless the setup script is updated.

Keep:

```text
voice_welcome_ms.wav
voice_signedout_ms.wav
voice_signedin_ms.wav
voice_start_ms.wav
voice_settings_ms.wav
voice_achievements_ms.wav
```

Optional later naming if more languages are added:

```text
voice_welcome_en.wav
voice_welcome_ms.wav
voice_welcome_ar.wav
```

## Quality Checklist

Before considering the task complete:

- No clip is silent.
- All clips are clear.
- No clipping or distortion.
- Volumes feel consistent.
- Malay pronunciation is understandable.
- Each clip starts quickly with no long empty gap.
- Each clip has a tiny fade-out to avoid clicks.
- Unity console has no import errors.
- Main menu professor guide plays the right line for each context.

## Recommendation

Proceed with Phase 1 using local generation first.

Best immediate path:

1. Try local Windows TTS to generate real WAV speech.
2. If unavailable, generate temporary voice-like placeholder WAVs so the menu audio pipeline can be verified.
3. Later replace with polished AI/professional narration using the same filenames.

