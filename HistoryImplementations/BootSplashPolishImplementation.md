# Boot Splash Polish Implementation

## Current State

The project already has a boot splash flow:

- Scene: `Assets/Scenes/Boot.unity`
- Runtime controller: `Assets/Script/Startup/StudioSplashController.cs`
- Editor setup tool: `Assets/Script/Editor/StudioSplashBootSetup.cs`
- Profile asset: `Assets/Branding/StudioSplash/Profiles/StudioSplashProfile.asset`

The current splash is functional and shows:

- Dark lab-style background
- `USM` title text
- `presents E-LAB PRIME`
- Progress bar
- Loading status text
- Animated light sweep
- Async load into `MainMenu`

The screenshot shows it is clean but still too plain. The main things to polish are visual hierarchy, brand authenticity, cinematic motion, audio, and the transition into the main menu.

## Goal

Upgrade the boot splash into a premium studio-style intro that feels like a professional educational simulation product.

The boot splash should feel like:

> A digital science lab system coming online under the USM brand.

It should not feel like a simple loading screen.

## Key Problems To Improve

### 1. Brand Mark Is Too Plain

The current screen shows `USM` as text. This works as a fallback, but it does not feel official or premium.

Implementation direction:

- Use a valid USM/APEX logo image when available.
- Keep the `USM` text fallback only if the logo asset is missing or corrupted.
- Add a subtle logo glow and reflection-like sheen.
- Use `USM` as the institutional identity and `E-LAB PRIME` as the product identity.

Important: earlier logo files were corrupted/zero-filled, so this phase should include asset validation before assigning any logo.

### 2. Background Is Too Empty

The dark background looks clean, but it lacks depth and science-lab identity.

Implementation direction:

- Add a very subtle animated grid or molecular-line field.
- Add soft vignette and radial light behind the logo.
- Add tiny slow-moving particles, like dust/light motes or data points.
- Keep the background restrained, not noisy.

This should remain dark and premium, but not flat.

### 3. Light Sweep Looks Like A Chunky Bar

The current teal sweep on the left side appears as a large rectangular block in the screenshot.

Implementation direction:

- Replace the thick light sweep with one or more thin scanning lines.
- Use additive-looking cyan alpha.
- Animate across the logo or across the full screen diagonally.
- Clamp visibility so it never becomes a large visible block.

### 4. Progress Area Feels Too Low And Detached

The progress bar sits near the bottom and feels separate from the brand moment.

Implementation direction:

- Move progress/status closer to the lower-middle of the frame.
- Add compact boot status labels:
  - `Initializing lab systems...`
  - `Loading experiment modules...`
  - `Preparing student interface...`
  - `Launching E-Lab Prime...`
- Use a thin segmented progress line instead of a simple fill bar.
- Add small status pips or ticks.

### 5. Motion Timing Can Feel More Cinematic

The current flow is serviceable, but it can use a more deliberate sequence.

Recommended timeline:

1. Fade from black.
2. Background grid/particles softly appear.
3. USM logo fades/scales in.
4. Light scan passes across the logo.
5. `presents` appears.
6. `E-LAB PRIME` appears as the main product title.
7. Boot status/progress animates.
8. Screen fades to main menu.

Target duration: 4.0 to 5.0 seconds.

Skip should be available after 1.0 second.

### 6. Audio Is Too Minimal

The splash currently uses the existing `snap.wav` stinger when available.

Implementation direction:

- Add a custom boot chime.
- Add a soft scan pass sound.
- Add a tiny progress tick.
- Fade audio out before scene activation.

No voice is needed for the boot splash. It should feel sleek and fast.

## Proposed Visual Design

### Layout

```text
 ------------------------------------------------
|                                                |
|        subtle animated science/data field       |
|                                                |
|                                                |
|                    USM LOGO                    |
|                                                |
|              presents E-LAB PRIME              |
|                                                |
|                                                |
|             segmented progress line             |
|             Initializing lab systems...         |
|                                                |
 ------------------------------------------------
```

### Visual Style

- Background: near-black teal, not pure black
- Accent: cyan/teal
- Text: soft white with slight shadow
- Motion: slow, precise, premium
- UI density: minimal, but with enough system detail to feel intentional

## Implementation Plan

### Phase 1: Clean Foundation

Files to update:

- `Assets/Script/Startup/StudioSplashController.cs`
- `Assets/Script/Editor/StudioSplashBootSetup.cs`
- `Assets/Branding/StudioSplash/Profiles/StudioSplashProfile.asset`
- `Assets/Scenes/Boot.unity`

Tasks:

- Repair or replace the splash implementation documentation.
- Validate the logo asset before assignment.
- Update setup script so invalid logo files cannot break the splash.
- Keep text fallback if no valid logo exists.
- Reduce or replace the current thick light sweep.
- Improve layout spacing for progress/status.

### Phase 2: Premium Motion Layer

Add new UI elements through `StudioSplashBootSetup.cs`:

- `BackgroundGlow`
- `ScienceGrid`
- `ParticleField`
- `LogoSheen`
- `ScanLineA`
- `ScanLineB`
- `StatusPips`
- `SegmentedProgressRoot`

Add runtime animation in `StudioSplashController.cs`:

- Logo fade and scale
- Product title delayed reveal
- Background shimmer
- Scanline movement
- Progress status cycling
- Final fade to black

### Phase 3: Audio Polish

Create or add:

- `Assets/Branding/StudioSplash/Audio/splash_boot_chime.wav`
- `Assets/Branding/StudioSplash/Audio/splash_scan.wav`
- `Assets/Branding/StudioSplash/Audio/splash_progress_tick.wav`

Controller additions:

- `AudioClip bootChime`
- `AudioClip scanSfx`
- `AudioClip progressTickSfx`
- Play boot chime when logo appears.
- Play scan SFX when logo sheen crosses.
- Play progress tick when status changes.

### Phase 4: Transition Polish

Improve transition into main menu:

- Hold black for a very short beat before activating `MainMenu`.
- Ensure audio fade ends before scene activation.
- Avoid showing both boot splash and main menu overlay in the same moment.
- Keep async load logic intact.

## Runtime Controller Changes

### Add Fields

Add these references to `StudioSplashController`:

```csharp
public CanvasGroup productTitleGroup;
public CanvasGroup statusGroup;
public RectTransform logoSheen;
public RectTransform scanLineA;
public RectTransform scanLineB;
public Image[] progressSegments;
public TextMeshProUGUI[] statusStepTexts;
public AudioClip bootChime;
public AudioClip scanSfx;
public AudioClip progressTickSfx;
```

### Add Behavior

Add these runtime behaviors:

- `AnimateLogoReveal`
- `AnimateBackgroundSystems`
- `AnimateSegmentedProgress`
- `UpdateBootStatusStep`
- `PlayOneShotSafe`
- `ValidateSplashReferences`

The current `AnimateSplash` method can be extended rather than replaced.

## Editor Setup Changes

### Asset Validation

`StudioSplashBootSetup.cs` should validate image signatures:

- PNG: `89 50 4E 47 0D 0A 1A 0A`
- JPG: `FF D8 FF`

If the source logo is invalid:

- Do not copy it.
- Do not assign it.
- Use text fallback.
- Log a clear warning.

### Rebuildable Scene

The setup tool should create every polish element so the scene remains reproducible.

Recommended menu item:

```text
E-Lab Prime/Setup Premium Boot Splash Polish
```

This can either replace the existing setup menu or sit beside it.

## Suggested Status Copy

Use short status lines:

```text
Initializing lab systems...
Loading experiment modules...
Preparing student interface...
Calibrating virtual instruments...
Launching E-Lab Prime...
```

Avoid too many long technical sentences. The boot splash should move quickly.

## Suggested Timing

```text
Fade in: 0.45s
Logo reveal: 0.70s
Title reveal: 0.45s
System progress: 2.40s
Fade out: 0.70s
Total: about 4.7s
Skippable after: 1.0s
```

## Acceptance Criteria

The polish pass is ready when:

- Boot splash uses a valid logo image or a clean fallback without errors.
- Light sweep no longer appears as a thick block.
- Background has subtle premium science-lab motion.
- Progress/status area feels integrated into the composition.
- Splash can be skipped after 1 second.
- Main menu loads cleanly after the splash.
- No Unity console errors.
- Boot scene can be rebuilt from the editor setup tool.
- Screenshot looks professional at 16:9.

## Recommended First Implementation

Start with a focused polish pass:

1. Fix logo validation and fallback.
2. Replace thick sweep with thin scan lines.
3. Add segmented progress and status cycling.
4. Add subtle background particles/grid.
5. Add custom SFX.
6. Rebuild `Boot.unity`.
7. Verify with screenshot and console check.

This gives the biggest premium improvement without risking the boot flow.

