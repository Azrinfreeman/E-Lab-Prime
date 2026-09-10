# Prof. Hakim Dialogue: Once Per Game Session

## Status

Implemented and runtime-verified on 14 July 2026.

## Requested Behaviour

- Show Prof. Hakim's automatic introduction once, at the beginning of a game session.
- Do not automatically show any professor dialogue when the player returns to `MainMenu` from an experiment or another scene.
- If the player leaves the menu before finishing or skipping the introduction, returning to `MainMenu` must still not restart it.
- Starting a new game process, or stopping and starting Play Mode again, should allow the introduction to play again.
- Existing button-hover guidance should remain available, but each hover prompt should play only once per game session, even after the menu scene reloads.

## Current Cause

`MainMenuProfessorGuide.Start()` runs whenever `MainMenu` is loaded. After the opening overlay disappears, `BeginAfterStartupDelay()` currently does one of two things:

1. If `ELabPrime_ProfessorGuideSeenIntro` is not set, it starts the complete introduction queue.
2. If the key is already set, it still calls `PlaySingleLine(GetSignedInContext())`.

This means returning to `MainMenu` always produces at least one automatic dialogue. The complete sequence can also replay when the player leaves before `FinishIntroSequence()` writes the preference key.

The hover-prompt history is also stored in an instance `HashSet`. Loading `MainMenu` creates a new guide instance and clears that history.

## Implementation Decision

Use an in-memory, per-application-session state inside `MainMenuProfessorGuide`.

Do not keep the professor GameObject alive with `DontDestroyOnLoad`. Persist only the small amount of state required to know whether the launch introduction and individual hover prompts have already been used.

### Session State

Add:

```csharp
private static bool startupGuideConsumed;
private static readonly HashSet<ProfessorGuideContext> playedPromptsThisSession = new();
```

Reset both values when Unity initializes a new runtime session:

```csharp
[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
private static void ResetSessionState()
{
    startupGuideConsumed = false;
    playedPromptsThisSession.Clear();
}
```

`SubsystemRegistration` is important because the project uses Enter Play Mode with domain reload disabled. It ensures Stop/Play behaves like a fresh game launch during editor testing.

## Startup Flow

Update `BeginAfterStartupDelay()` as follows:

1. Continue waiting for `OpeningSequenceController` to finish, preserving the current opening-overlay timing.
2. If `startupGuideConsumed` is already `true`, keep the professor root hidden and exit without playing audio, typewriter text, animation, or highlights.
3. Otherwise, set `startupGuideConsumed = true` immediately, before starting the introduction.
4. Play the existing introduction queue normally.

Setting the flag before the first line is essential. It prevents a replay when the player enters an experiment before pressing `NEXT` or `SKIP`.

Proposed logic:

```csharp
if (startupGuideConsumed)
{
    SetRootVisible(false, true);
    HideAllHighlights();
    yield break;
}

startupGuideConsumed = true;
PlayIntroSequence();
```

## Preference-Key Cleanup

The current `ELabPrime_ProfessorGuideSeenIntro` preference represents an installation-level state, while the requested behaviour is session-level.

Remove its use from:

- `BeginAfterStartupDelay()`
- `FinishIntroSequence()`

The old stored PlayerPrefs value can be left on existing installations because it becomes harmless once no code reads it. A migration or `DeleteKey` call is unnecessary and could unexpectedly modify player preferences.

## Hover-Prompt Behaviour

Replace the instance `playedHoverPrompts` collection with the static `playedPromptsThisSession` collection.

`PlayPrompt()` will then:

- Ignore a context that has already played during the current game run.
- Preserve the existing cooldown.
- Continue suppressing hover prompts while the launch introduction is active.
- Remember prompts across experiment-to-menu scene changes.
- Reset prompt availability after a full relaunch or a new Play Mode run.

## Implemented File

### `Assets/Script/MainMenuProfessorGuide.cs`

- Add the launch-session flag and shared prompt-history set.
- Add the `SubsystemRegistration` reset method.
- Replace the PlayerPrefs startup branch with the session guard.
- Mark the introduction consumed before playback begins.
- Remove the PlayerPrefs write from `FinishIntroSequence()`.
- Point `PlayPrompt()` to the session-level prompt history.

No Unity scene, prefab, dialogue asset, audio clip, or navigation script should require modification.

## Verification Result

An automated Play Mode check loaded the saved `MainMenu`, waited for the first introduction to begin, reloaded `MainMenu`, and inspected the newly created guide instance.

```text
FirstGuideExists=True
FirstLaunchVisible=True
FirstLaunchConsumed=True
ReturnGuideExists=True
ReturnHidden=True
ReturnIntroInactive=True
ReturnRoutineInactive=True
SessionConsumedAfterReturn=True
```

Unity compiled the final project state without new C# errors, `NullReferenceException`, or `MissingReferenceException` messages.

## Verification Checklist

1. Start Play Mode from a clean run: the opening sequence finishes, then Prof. Hakim's introduction appears once.
2. Enter an experiment after completing the introduction, return to `MainMenu`: no automatic professor dialogue appears.
3. Enter an experiment midway through the introduction, return to `MainMenu`: the introduction does not restart.
4. Press `SKIP`, enter an experiment, return: the introduction remains hidden.
5. Move through several experiments and return repeatedly: no startup dialogue, voice, text blip, or target highlight is triggered.
6. Hover a menu button: its prompt appears once. Return from an experiment and hover the same button: it does not replay in the same session.
7. Stop Play Mode and start it again: the launch introduction and hover-prompt history reset correctly.
8. Confirm the opening overlay and automatic home-panel activation still behave exactly as before.
9. Check the Unity Console for compilation errors and missing-reference exceptions.

## Acceptance Criteria

- The professor introduction runs no more than once per game process or Play Mode run.
- Reloading `MainMenu` never starts automatic professor dialogue.
- Leaving during an unfinished introduction does not allow it to replay on return.
- Hover prompts remain one-time-per-context for the whole session.
- A fresh launch resets the guide naturally.
- No changes are made to login state, progress data, opening-sequence timing, or menu navigation.
