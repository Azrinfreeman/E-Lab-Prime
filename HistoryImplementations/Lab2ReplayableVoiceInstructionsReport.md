# Lab2_3D replayable Malay voice guidance

Implemented the same key-free, offline-playback approach as Lab1, scoped to Lab2.

Three synthetic Malay clips (ms-MY-OsmanNeural, -8% rate) guide the learner to switch on the circuit, observe gas collection, and review results using the notebook and quiz. Generated through the existing edge-tts workflow. No runtime network connection or API key is required.

A dedicated screen-space card includes the current transcript, Ulang Arahan, Hentikan suara and a session-only mute control. Speech follows the actual Lab2 tutorial presentation points. The final instruction remains replayable when the existing success callback occurs; it is not interrupted after the short success delay. Reset clears playback and restarts guidance. The feature does not change electrolysis logic or progress storage.

## Verification

- All three clips imported: 8.400, 8.016 and 10.344 seconds.
- Runtime replay, rapid replay, mute, unmute/replay and stop passed.
- Existing tutorial callbacks advanced through all three cues. Final guidance remained selected on completion callback.
- Longest transcript initially overflowed; bounded text autosizing fixed it (19.6-point rendered font, no overflow).
- Runtime screenshot reviewed: controls sit below the apparatus. Dedicated canvas avoids accidentally placing the card on the 3D tablet.
- No experiment-completion method or progress-writing action was invoked; only presentation callbacks were probed.
- Lab1 source and scene files were not edited by this task.

## Limits

Human pronunciation review, full experiment completion, keyboard-only navigation and alternative aspect-ratio checks remain unverified. The existing Lab2 success popup/quiz flow was preserved. Service-output licensing was not independently assessed.

## Files

Assets/Script/3d/Lab2VoiceInstructions.cs; Assets/Script/3d/Lab2TutorialManager.cs; Assets/Script/Editor/Lab2VoiceSetup.cs; narrow integration in Assets/Script/Editor/Lab2_3D_Setup.cs; Assets/Scenes/Lab2_3D.unity; Assets/Audio/Lab2/Instructions/; Tools/GenerateLab2Voice.py.

Backups and test evidence: output/lab2-voice/. Runtime screenshot before text-fit correction: output/agent-captures/df7b2ca3df7d4044a8e0cfd5617e39f3/capture.png. Final saved preview: output/linked-scene-colours/Lab2_3D-lab-view-voice-final.png.
