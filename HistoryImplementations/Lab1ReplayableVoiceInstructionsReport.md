# Lab1 replayable voice instructions — implemented

## Delivered

Lab1_3D now has five bundled Malay narration clips: four experiment steps and a completion cue. The lower-centre instruction card includes the matching transcript, Ulang Arahan, Hentikan suara and a session-only mute toggle. A single non-spatial AudioSource prevents overlapping playback. Narration follows the visible tutorial cues, waits for the loading overlay, and retains timing instructions until ten swings complete. Replay does not call any experiment or progress methods.

Generated with the installed edge-tts tool using ms-MY-OsmanNeural at -8% rate. The user explicitly approved sending only the five instruction sentences to Microsoft Edge TTS. No API key is used; the game plays imported MP3 assets offline. The narration is synthetic. Generator reference: https://github.com/rany2/edge-tts . Service output licensing was not independently assessed.

## Validation

- Five clips imported successfully, durations 7.824, 8.544, 10.296, 10.608 and 9.744 seconds.
- Unity compiled the scripts; console error checks returned zero entries.
- Runtime checks passed for rapid replay, a single audio source, mute stopping playback, replay after unmute, stop, and fitting the long timing transcript without overflow.
- Existing step callbacks advanced narration to cues 2 and 3. Starting the timing step retained cue 3 while awaiting ten swings.
- Runtime screenshot inspected. Final card moved below the experiment area; its background does not intercept pointer input.
- Lab1 setup integration recreates the controls and audio bindings when that scene is regenerated.

## Limits

No full experiment completion or progress-writing action was invoked. Pronunciation has not received a human listening review. Keyboard interaction and alternative aspect ratios have not been fully tested. Original editor was running Lab2; it is restored to Lab2 in edit mode to avoid restarting an unrelated experiment.

## Files and evidence

Assets/Script/3d/Lab1VoiceInstructions.cs; Lab1TutorialManager.cs; Assets/Script/Editor/Lab1VoiceSetup.cs; Lab1_3D_Setup.cs; Assets/Scenes/Lab1_3D.unity; Assets/Audio/Lab1/Instructions/; Tools/GenerateLab1Voice.py.

Backups and playback checks: output/lab1-voice/. Runtime capture (before final lower placement): output/agent-captures/40e585307b854921a582709244023aab/. Final saved-scene preview: output/linked-scene-colours/Lab1_3D-lab-view-voice-final.png.
