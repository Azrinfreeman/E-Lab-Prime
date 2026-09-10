# Main Menu Professor Guide Implementation

## Goal

Add a premium main menu guide: a 2D male professor character who welcomes the player, explains the menu, reacts to account status, and gently guides students toward the next useful action.

The feature should feel like part of a polished digital science lab, not a pop-up tutorial. The professor should add warmth, motion, voice, and context while keeping the existing main menu clean and usable.

## Creative Direction

Recommended character concept:

- Name: Prof. Hakim or Dr. Farid
- Role: friendly digital lab mentor
- Visual style: clean 2D illustrated professor, lab coat, teal science accent, subtle outline, readable at menu scale
- Personality: calm, encouraging, professional, slightly playful
- Language style: Malay first, English subtitle in brackets, matching the current menu style
- Screen position: lower-left or lower-right, outside the main button grid so he frames the menu instead of covering it

Main menu fantasy:

The player is not just entering a menu. They are arriving at a guided digital science lab, and the professor is there to prepare them before experiments.

## Premium Feature Ideas

### 1. Animated Professor Avatar

Use a 2D half-body professor sprite with simple state animation:

- Idle breathing
- Blink every few seconds
- Talk mouth flap while dialogue text is typing
- Point gesture when highlighting a menu button
- Smile pose after the player signs in
- Thinking pose when opening achievements or settings

Implementation can start with separate PNG sprites, then upgrade later to a 2D rig or Spine-like layered animation.

### 2. Smart Dialogue Box

Add a frosted-glass dialogue box beside the professor:

- Professor nameplate
- Typewriter text reveal
- Small continue button
- Skip button
- Voice toggle icon
- "Do not show intro again" option after first completion

The dialogue box should use the same dark glass and cyan accent style already used in the main menu.

### 3. Contextual Guidance

The professor should react to menu state:

- First launch: welcome and explain the lab
- Student not signed in: recommend signing in to save progress
- Student signed in: welcome them back by name if available
- Hover or select Start Experiments: explain that this opens lab activities
- Hover or select Settings: explain audio and accessibility settings
- Hover or select Achievements: explain progress tracking
- After returning from a lab: congratulate and suggest checking achievements

This makes the menu feel alive without requiring complex AI.

### 4. Guided First-Time Flow

On first launch, run a short premium onboarding sequence:

1. Professor fades in.
2. He welcomes the student.
3. The sign-in panel pulses if the student is not signed in.
4. Start Experiments button gets a soft highlight.
5. Professor says the player can begin anytime.
6. Sequence ends and normal menu interaction begins.

Keep it short: 15 to 25 seconds maximum, skippable after 2 seconds.

### 5. Button Highlight Callouts

When the professor mentions a menu action, add a professional highlight:

- Cyan glow around the target button
- Small pointer line from dialogue box to button
- Soft pulse for 1.5 seconds
- Optional science UI ping sound

This gives the professor a real guiding role instead of being decorative.

### 6. Voice and SFX Layer

Optional premium audio layer:

- Short voice clips per dialogue line
- Procedural text blip sound while text types
- Soft UI chime when professor appears
- Small whoosh when the dialogue box opens
- Gentle "lab ambience" bed underneath the main menu

If full voice generation is not available in the local toolchain, start with high-quality text blips and UI SFX. Voice clips can be added later without changing the UI system if dialogue lines already have optional AudioClip references.

### 7. Player Progress Awareness

Make the professor feel personal:

- If no labs completed: "Mari kita mulakan eksperimen pertama."
- If some labs completed: "Bagus, anda sudah mula membina kemahiran makmal."
- If all labs completed: "Hebat. Semua amali telah selesai."
- If achievements exist: suggest opening achievements
- If audio is muted: do not auto-play voice

This can use local saved progress first, then account data later.

### 8. Premium Idle Lines

If the player stays on the menu for more than 10 seconds, the professor can say short non-intrusive lines:

- "Pilih amali apabila anda bersedia."
- "Kemajuan anda akan disimpan selepas log masuk."
- "Eksperimen yang baik bermula dengan pemerhatian yang teliti."

Limit idle lines so they do not become annoying.

## Proposed File Structure

```text
Assets/
  Branding/
    ProfessorGuide/
      Audio/
        professor_appear.wav
        professor_text_blip.wav
        voice_welcome_ms.wav
      Dialogue/
        MainMenuProfessorDialogue.asset
      Materials/
      Sprites/
        professor_idle.png
        professor_talk.png
        professor_point.png
        professor_smile.png
  Script/
    MainMenuProfessorGuide.cs
    ProfessorAvatarAnimator.cs
    ProfessorDialogueDatabase.cs
    ProfessorDialogueLine.cs
  Script/
    Editor/
      MainMenuProfessorGuideSetup.cs
Docs/
  MainMenuProfessorGuideImplementation.md
```

## Runtime Components

### MainMenuProfessorGuide.cs

Main controller attached to a `ProfessorGuideRoot` object under the main menu Canvas.

Responsibilities:

- Detect first launch and current account state
- Play dialogue sequences
- Control typewriter text
- Trigger avatar states
- Trigger optional voice and SFX
- Highlight relevant menu buttons
- Listen for menu actions from `MainMenuManager`

### ProfessorDialogueDatabase.cs

A ScriptableObject that stores dialogue lines by context.

Example contexts:

- `FirstLaunchWelcome`
- `SignedOutPrompt`
- `SignedInWelcome`
- `HoverStartExperiments`
- `HoverSettings`
- `HoverAchievements`
- `ReturnFromLab`
- `IdleHint`

Each line should include:

- Malay text
- English subtitle
- Avatar pose
- Optional AudioClip
- Optional target UI object key
- Whether player can skip

### ProfessorAvatarAnimator.cs

Lightweight UI animation controller.

Responsibilities:

- Swap avatar sprites
- Blink timer
- Breathing scale animation
- Talk mouth animation while text is typing or voice is playing
- Point animation toward highlighted buttons

This can be implemented without Animator Controller at first for faster iteration.

### MainMenuProfessorGuideSetup.cs

Editor utility that builds the UI in `MainMenu.unity`.

Responsibilities:

- Create `ProfessorGuideRoot`
- Add avatar Image
- Add dialogue panel
- Add TMP text fields
- Add continue, skip, and voice buttons
- Wire references to `MainMenuProfessorGuide`
- Create default dialogue database
- Save the scene

## UI Layout Recommendation

Desktop layout:

- Professor avatar: bottom-left, about 24 percent of screen height
- Dialogue box: above or beside avatar, max width 520 px
- Main menu buttons remain centered and unobstructed
- Highlight targets appear on existing buttons

Mobile or narrow layout:

- Professor avatar: bottom-left, smaller
- Dialogue box: bottom band above navigation
- Text should wrap to 2 or 3 lines maximum
- Continue button remains reachable with thumb

## Dialogue Samples

First launch:

```text
Selamat datang ke Makmal Sains Digital.
[Welcome to the Digital Science Lab.]
```

Signed out:

```text
Log masuk dahulu supaya kemajuan eksperimen anda dapat disimpan.
[Sign in first so your experiment progress can be saved.]
```

Start experiments:

```text
Tekan Mulakan Amali untuk memilih eksperimen sains anda.
[Press Start Experiments to choose your science activity.]
```

Achievements:

```text
Di sini anda boleh melihat pencapaian dan kemajuan setiap amali.
[Here you can view achievements and progress for each experiment.]
```

Settings:

```text
Laraskan bunyi, paparan, dan pengalaman pembelajaran di sini.
[Adjust sound, display, and learning experience here.]
```

Returning player:

```text
Selamat kembali. Mari sambung penerokaan sains anda.
[Welcome back. Let us continue your science exploration.]
```

## Asset Generation Plan

### Professor 2D Art

Generate or create these transparent PNGs:

- `professor_idle.png`
- `professor_talk.png`
- `professor_point.png`
- `professor_smile.png`
- `professor_think.png`

Prompt direction:

```text
2D game character, friendly male science professor, Malaysian digital science lab mentor, white lab coat, teal accent tie, warm smile, clean educational game style, transparent background, half body, polished mobile game UI asset, crisp outline, professional but approachable
```

### SFX

Create lightweight WAV assets:

- `professor_appear.wav`: soft digital chime
- `professor_text_blip.wav`: short warm UI tick
- `professor_confirm.wav`: subtle success tone
- `professor_highlight.wav`: soft science ping

These can be generated procedurally if needed.

### Voice

Voice should be optional and data-driven.

Recommended voice direction:

- Male adult educator
- Calm Malaysian English/Malay pronunciation
- Short clips only
- No long monologues
- Respect settings mute state

If voice generation is unavailable, use text blip SFX first and add voice clips later.

## Implementation Phases

### Phase 1: Playable Premium Guide

- Add static professor sprite
- Add dialogue panel
- Add typewriter text
- Add first launch sequence
- Add skip and continue buttons
- Add Start, Settings, Achievements contextual lines
- Store "seen intro" in PlayerPrefs

### Phase 2: Professional Polish

- Add blink, breathing, and talk animation
- Add target button glow
- Add SFX
- Add signed-in and signed-out dialogue
- Add idle hints
- Add mobile responsive layout

### Phase 3: Asset Upgrade

- Generate final professor poses
- Add voice clips
- Add expression variants
- Add pointer animation
- Add return-from-lab dialogue

### Phase 4: Personalization

- Use student name if signed in
- React to completed labs
- Recommend next lab
- Mention achievements unlocked
- Respect accessibility preferences

## Integration Points

Existing project pieces to connect:

- `Assets/Scenes/MainMenu.unity`
- `Assets/Script/MenuManager.cs`
- `Assets/Script/MainMenuSessionStatusUI.cs`
- `Assets/Script/MainMenuAccountPanelController.cs`
- `Assets/Script/Editor/MainMenuSceneSetup.cs`
- `Assets/Script/Editor/MainMenuAccountPanelSetup.cs`

Recommended integration approach:

- Do not rewrite `MainMenuManager`.
- Add small public events or direct calls from menu button hover/click handlers.
- Keep professor dialogue independent from panel opening logic.
- Store UI references in the setup script so the scene can be rebuilt safely.

## Acceptance Criteria

The feature is ready when:

- Main menu opens with professor visible and not blocking buttons
- First-time dialogue plays and can be skipped
- Dialogue text types smoothly
- Professor reacts to at least Start, Settings, Achievements, and Sign In states
- User can disable voice or skip guidance
- Feature works at 16:9 and common mobile aspect ratios
- No Unity console errors
- Scene can be rebuilt using the editor setup tool

## Recommended First Build

Build Phase 1 first with a placeholder professor sprite and text blip SFX. After the interaction feels good, upgrade the art and voice. This keeps the premium feeling tied to behavior and timing, not only to visuals.

