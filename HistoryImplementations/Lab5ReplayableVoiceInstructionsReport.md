# Lab5_3D replayable Malay voice guidance

Added three bundled synthetic Malay clips using the same approved key-free Edge TTS workflow (ms-MY-OsmanNeural, -8% rate). Game playback works offline without an API key.

Lab5 presents its first two hints together, so one voice cue covers switching on the heater and adjusting temperature, stirring and particle size. The second cue covers starting and observing the experiment. The third appears when the existing finished state is reached and directs the learner to Log Data and the quiz.

The lower-centre card contains the matching transcript, Ulang Arahan, Hentikan suara and session mute. A single audio source prevents overlapping narration. Reset clears playback; startup respects the existing camera delay and loading overlay. Lab5 setup recreates the feature. Other labs and solubility simulation logic were not edited.

## Verification

- Three clips imported: 8.664, 8.472 and 8.088 seconds.
- Runtime replay, rapid replay, mute, unmute/replay and stop passed.
- All transcripts fit without overflow.
- Runtime screenshot inspected: card leaves apparatus and tablet controls visible.
- Narration transitions checked with tutorial-only preparation flags, followed by temporary finished-state values while the simulation component and its coroutines were disabled. No experiment completion methods or progress-writing actions invoked. Temporary state discarded on leaving Play mode.

## Limits

Human pronunciation review, full experiment completion, keyboard-only navigation and alternative aspect ratios remain unverified. Service-output licensing was not independently assessed.

## Evidence

Backups and playback/cue checks: output/lab5-voice/. Runtime capture: output/agent-captures/988b1468391d4653b975fa9af7bc0fdb/capture.png.

Implementation: Assets/Script/3d/Lab5VoiceInstructions.cs; Lab5TutorialManager.cs; Assets/Script/Editor/Lab5VoiceSetup.cs; narrow integration in Lab5_3D_Setup.cs; Assets/Scenes/Lab5_3D.unity; Assets/Audio/Lab5/Instructions/; Tools/GenerateLab5Voice.py.
