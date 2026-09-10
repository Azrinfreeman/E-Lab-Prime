# Lab3_3D replayable Malay voice guidance

Added four bundled synthetic Malay instructions using the same approved key-free Edge TTS workflow as Lab1 and Lab2 (ms-MY-OsmanNeural, -8% rate). Clips cover adding salt, wearing goggles, opening gas, and lighting the burner/observing temperature. Playback works offline without an API key.

A lower-centre screen-space card provides the matching transcript, Ulang Arahan, Hentikan suara and session mute. One audio source prevents voice overlap. Guidance follows the existing tutorial presentation points and waits for the loading screen. The final instruction stays replayable during heating; it does not claim that the entire experiment has completed. Reset clears playback and restarts the tutorial. Lab3 setup recreates the feature without altering heating behaviour.

## Verification

- Four clips imported: 8.688, 5.568, 9.336 and 8.784 seconds.
- Runtime replay, rapid replay, mute, unmute/replay and stop passed.
- All four transcripts fit without overflow.
- Runtime screenshot reviewed: apparatus and tablet controls remain visible.
- Presentation callbacks tested independently of experiment execution. No heating completion or progress-writing methods invoked.
- Lab1 and Lab2 files were not edited in this task.

## Limits

Human pronunciation review, full experiment completion, keyboard-only navigation and alternative aspect-ratio testing remain unverified. Service-output licensing was not independently assessed.

## Evidence and files

Backups and playback results: output/lab3-voice/. Runtime capture: output/agent-captures/abf509f1fa5947279fd614f39b523855/capture.png.

Implementation: Assets/Script/3d/Lab3VoiceInstructions.cs; Lab3TutorialManager.cs; Assets/Script/Editor/Lab3VoiceSetup.cs; narrow integration in Lab3_3D_Setup.cs; Assets/Scenes/Lab3_3D.unity; Assets/Audio/Lab3/Instructions/; Tools/GenerateLab3Voice.py.
