# Lab1_3D — replayable experiment voice instructions

Status: approved and implemented. See Lab1ReplayableVoiceInstructionsReport.md for verification and limitations.

## Goal and scope

Add friendly spoken guidance to Assets/Scenes/Lab1_3D.unity. Explain the existing pendulum experiment one step at a time, with a clearly labelled replay button. Preserve the approved bright palette, scientific behaviour, data, layout of experiment controls and all other scenes.

Default voice direction: clear Bahasa Melayu (Malaysia), matching the existing tutorial language and MainMenu narration precedent. Use a warm teacher-like voice, a moderate pace and short sentences. Match the existing narrator where its source is available; do not assume the existing menu clips contain experiment instructions.

## Existing behaviour verified in source

Lab1TutorialManager already presents four instructions and advances through callbacks from the experiment. JamRandik automatically stops after ten complete swings. PengurusDataAmali rejects recording before ten swings. Existing MainMenu Malay voice clips are present, but no Lab1 narration clips were found.

One integration detail matters: currentStep becomes 4 when timing starts, before the fourth instruction is displayed. Narration must follow the instruction actually presented, not simply read that numeric field.

## Player experience

Add a compact instruction card anchored near the top centre of the existing Canvas, below the experiment heading. Confirm its final position against the actual Game view so it does not cover the pendulum, stopwatch, right controls or notebook. The card shows the current step and the complete spoken instruction, using dark text on a warm-white surface.

Inside the card:

- A speaker icon with the text **Ulang Arahan**, using the established bright button style. It replays the currently displayed instruction from the beginning.
- A small **Hentikan suara** control while speech is playing.
- A **Suara: Hidup / Senyap** toggle. Honour the existing project voice preference if it is shared; otherwise keep a Lab1 session preference without adding persistent settings in this change.

Play each instruction once when it becomes visible and the scene loading overlay has finished. Never loop instructions while waiting for the learner. A new instruction stops the previous clip before starting. Rapid replay clicks restart a single voice source; they must not stack voices. Replay does not restart the tutorial, stopwatch, pendulum or data recording.

Muted users retain every written instruction. Replay is disabled while muted, with an explicit visible muted state. After completion, leave a short completion message replayable. Reset returns to instruction one; leaving the scene stops speech.

## Proposed narration script

| Cue | When to present it | Spoken words / matching transcript |
|---|---|---|
| 1 — Panjang tali | First tutorial instruction becomes visible | “Mari mulakan eksperimen bandul. Laraskan panjang tali menggunakan peluncur di panel kanan.” |
| 2 — Lepaskan bandul | The existing length-adjustment step completes | “Tarik bandul sedikit ke sisi, pada sudut kurang daripada sepuluh darjah. Kemudian, lepaskan bandul.” |
| 3 — Ukur masa | The existing release step completes | “Mulakan jam randik di panel kiri. Tunggu sehingga sepuluh ayunan lengkap. Jam randik akan berhenti secara automatik.” |
| 4 — Rekod data | Ten swings have completed and the recording instruction appears | “Sepuluh ayunan sudah lengkap. Rekodkan bacaan menggunakan kawalan rekod data. Semak hasilnya dalam jadual pemerhatian.” |
| Selesai | A record has been successfully saved through the existing callback | “Bagus! Bacaan telah direkodkan. Ulang eksperimen dengan panjang tali yang berbeza untuk membandingkan hasil.” |

Before producing the final clips, align the words for stopwatch and recording controls with their exact visible labels. Do not narrate an action unsupported by the actual controls. The ten-degree instruction follows the existing tutorial; this feature does not change the pendulum's angle limits or success rules.

## Implementation approach after approval

1. Inspect the live Lab1 scene, audio settings, loading overlay, tutorial references and existing voice-generation provenance. Preserve any dirty editor state. Back up affected files and record event bindings.
2. Create five real spoken clips, preferably using the existing narration production method, and import them under Assets/Audio/Lab1/Instructions/. Bundle them with the game for offline playback; no runtime speech service, API key or new package is needed. Record voice source and usage rights. If a suitable Malay voice cannot be produced with available tooling, report that dependency; sound effects or text blips do not satisfy the requested voice feature.
3. Add Assets/Script/3d/Lab1VoiceInstructions.cs with explicit clip/transcript references, replay, stop, mute and cleanup. Use one scene-local non-spatial AudioSource with play-on-awake disabled. Respect existing master audio settings and avoid changing global audio volume.
4. Integrate presentation cues into Lab1TutorialManager at the actual badge-display points and successful completion. Keep the pending ten-swing wait on instruction three. Handle reset and disable without duplicate listeners, stale clips or interrupted-step callbacks.
5. Add the instruction card and controls only to Lab1_3D. Prefer a small dedicated, repeat-safe editor setup helper; have Lab1_3D_Setup preserve or recreate the feature so a later scene setup does not remove it. Do not rerun the entire scene builder merely to add narration.
6. Validate in one focused pass, then save the scene and report exact results and remaining limits.

Expected changes: Lab1_3D.unity; Lab1TutorialManager.cs; new Lab1VoiceInstructions.cs; a dedicated Lab1 voice setup helper and narrow Lab1_3D_Setup integration; five clips and their Unity metadata. Shared experiment and tutorial utilities should remain unchanged unless a demonstrated integration need requires a narrowly scoped change.

## Acceptance checks

- Listen to every clip: clear Malay pronunciation, correct step, matching transcript, no clipping and comfortable volume against existing sound effects.
- Verify automatic playback only when each instruction appears, including the full ten-swing wait before recording guidance.
- Replay before, during and after playback; repeated clicks never overlap voices or alter experiment state.
- Verify mute, stop, reset, scene exit and missing-clip fallback. Missing audio keeps text usable and produces a clear diagnostic; it is not accepted as a completed voice delivery.
- Check completion only after successful recording. Use isolated test data for any progress-writing interactions; do not reset or overwrite the user's real progress.
- Verify mouse and keyboard button operation, readable transcripts and unobstructed controls at 1920 × 1080, 1280 × 720 and a supported alternative aspect ratio.
- Compile successfully; check for new console errors, missing references and duplicate AudioListeners. Preserve original event targets and experiment behaviour.
- Confirm other scene files are unchanged. Restore the original editor scene and play state. Deliver final screenshots, audio paths and a concise validation report.

Implementation and key-free voice generation completed after approval.

