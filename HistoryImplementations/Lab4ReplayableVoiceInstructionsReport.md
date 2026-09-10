# Lab4_3D replayable Malay voice guidance

Added three bundled synthetic Malay instructions with the same approved key-free Edge TTS workflow (ms-MY-OsmanNeural, -8% rate). Guidance covers switching on the heater, adjusting evaporation factors and observing the experiment, then recording observations and answering the quiz. Game playback works offline without an API key.

The screen-space card includes the matching transcript, Ulang Arahan, Hentikan suara and session mute. One audio source prevents overlap. Narration follows the displayed tutorial stages, waits for the loading overlay, and retains the final instruction for replay. Reset clears playback. Lab4 setup recreates the feature. Experiment logic and progress storage remain unchanged.

## Verification

- Three imported clips: 6.960, 10.272 and 8.304 seconds.
- Runtime replay, rapid replay, mute, unmute/replay and stop passed.
- All transcripts fit without overflow.
- Presentation transitions tested by setting the tutorial-only heater flag and invoking tutorial callbacks; no experiment completion or progress-writing methods were called.
- Other lab files were not edited by this task.

## Limits

Human pronunciation review, full experiment completion, keyboard-only navigation and alternative aspect-ratio checks remain unverified. Service-output licensing was not independently assessed.

## Evidence

Backups and test results: output/lab4-voice/. Runtime capture: output/agent-captures/83b1a6cdcb384bf8a9e129518563f0bb/capture.png.

Implementation: Assets/Script/3d/Lab4VoiceInstructions.cs; Lab4TutorialManager.cs; Assets/Script/Editor/Lab4VoiceSetup.cs; narrow integration in Lab4_3D_Setup.cs; Assets/Scenes/Lab4_3D.unity; Assets/Audio/Lab4/Instructions/; Tools/GenerateLab4Voice.py.
