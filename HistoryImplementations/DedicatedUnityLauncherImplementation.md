# Dedicated Unity Launcher Implementation

## Goal

Create a separate desktop launcher for E-Lab Prime that appears before the actual game executable. The launcher should feel like a professional product shell: it lets the player adjust settings, read important information, see version/build details, and press Play to start the published game.

This is possible with Unity. The recommended approach is to build a second lightweight Unity application as the launcher, then package it beside the main game build.

## Recommended Direction

Use a separate Unity launcher build instead of making the game boot scene behave like a launcher.

Why:
- The launcher remains fast and lightweight.
- The game can stay focused on gameplay and loading.
- The launcher can be updated visually without changing gameplay scenes.
- It feels more premium for desktop releases.
- It can expose settings before the game allocates heavy scenes, 3D assets, or simulation systems.

Target platform for Phase 1:
- Windows desktop build.

Possible later platforms:
- macOS: possible, but launching app bundles needs platform-specific handling.
- Linux: possible, with path and permission handling.
- Mobile: not recommended, because mobile platforms do not normally allow a separate pre-game launcher.

## Final Published Folder Shape

Example Windows distribution:

```text
E-Lab Prime/
  E-Lab Prime Launcher.exe
  E-Lab Prime/
    E-Lab Prime.exe
    E-Lab Prime_Data/
    UnityPlayer.dll
  Launcher/
    launcher_config.json
    news.json
    patch_notes.json
```

The player opens:

```text
E-Lab Prime Launcher.exe
```

The launcher starts:

```text
E-Lab Prime/E-Lab Prime.exe
```

## Launcher Features

Phase 1 should include:

- Play button.
- Resolution selector.
- Window mode selector.
- Quality preset selector.
- Master volume slider.
- Language selector, if the game supports it.
- "Open Save Folder" button.
- "Reset Settings" button.
- Build/version display.
- Short game information panel.
- Patch notes panel.
- Footer with USM/APEX branding.

Phase 2 can add:

- News cards.
- Last played lab shortcut.
- Student profile status.
- Account sign-in status mirror.
- Repair/check files button.
- Data consent or school lab notice.
- Launch options such as "Start in Main Menu" or "Resume Last Experiment".

Phase 3 can add:

- Auto updater.
- Download progress.
- Integrity validation.
- Remote announcement feed.
- Support links.
- Crash log upload prompt.

## Important Limitation

A Unity launcher cannot safely change every possible Unity runtime setting after the game has already started. The launcher should write settings before launch, then the game reads those settings during startup.

Settings that can be handled easily:

- Resolution.
- Fullscreen/windowed mode.
- Quality preset.
- Master volume.
- Language.
- Accessibility toggles.
- Skip intro toggle.
- Preferred monitor, if implemented carefully.

Settings that need extra game-side support:

- UI scale.
- Audio mixer groups.
- Camera sensitivity.
- Lab assist/tutorial mode.
- Professor guide voice on/off.
- Any setting currently stored only in `PlayerPrefs`.

## Shared Settings File

Use a JSON file shared between launcher and game.

Recommended location:

```text
%USERPROFILE%/AppData/LocalLow/USM/E-Lab Prime/launcher-settings.json
```

Example:

```json
{
  "version": 1,
  "resolutionWidth": 1920,
  "resolutionHeight": 1080,
  "fullscreenMode": "FullscreenWindow",
  "qualityPreset": "High",
  "masterVolume": 0.85,
  "language": "ms",
  "skipStudioSplash": false,
  "professorVoice": true,
  "lastUpdatedUtc": "2026-07-10T00:00:00Z"
}
```

The launcher writes this file when the user presses Play or Apply.

The game reads this file in its first startup scene before showing the main menu.

## Launch Command

The launcher should start the game executable with `System.Diagnostics.Process`.

Example Windows launch:

```csharp
ProcessStartInfo startInfo = new ProcessStartInfo
{
    FileName = gameExePath,
    WorkingDirectory = Path.GetDirectoryName(gameExePath),
    Arguments = "-screen-width 1920 -screen-height 1080 -screen-fullscreen 1",
    UseShellExecute = true
};

Process.Start(startInfo);
Application.Quit();
```

Use command line arguments for core display settings, and use the shared JSON file for game-specific settings.

## Launcher Scene Design

Create one dedicated scene:

```text
Assets/Scenes/Launcher.unity
```

Recommended hierarchy:

```text
LauncherRoot
  LauncherCanvas
    Background
    TopBar
      Logo
      VersionText
    MainPanel
      PlayButton
      SettingsTabs
      InfoPanel
      PatchNotesPanel
    Footer
      BuildInfoText
      OpenFolderButton
      QuitButton
  LauncherAudio
  LauncherController
  EventSystem
```

Visual direction:

- Use the USM/APEX logo at the top.
- Use a clean scientific dashboard style.
- Keep the Play button large and unmistakable.
- Avoid a busy full game menu duplicate.
- Use the same teal/white/charcoal identity from the boot splash.
- Add subtle lab-grid motion or a soft animated scan line.
- Keep all controls readable at 1366x768 and 1920x1080.

## Scripts To Add

### `LauncherSettings`

Data model for launcher settings.

Responsibilities:

- Store resolution.
- Store fullscreen mode.
- Store quality preset.
- Store audio volume.
- Store language.
- Store launcher/game toggles.

### `LauncherSettingsStore`

Reads and writes the JSON settings file.

Responsibilities:

- Create default settings if no file exists.
- Save user selections.
- Validate missing or invalid settings.
- Return the correct LocalLow path.

### `LauncherController`

Main launcher UI controller.

Responsibilities:

- Populate resolution/quality dropdowns.
- Bind buttons and sliders.
- Save settings.
- Launch the game executable.
- Show errors if the game executable cannot be found.

### `GameStartupSettingsLoader`

This is added to the main game, not the launcher.

Responsibilities:

- Read `launcher-settings.json` during startup.
- Apply resolution and fullscreen fallback if command line values are missing.
- Apply quality preset.
- Apply master volume.
- Apply language and tutorial toggles.

## Game-Side Integration

The current project already has settings-related scripts, including:

```text
Assets/Script/SettingsManagerUI.cs
```

Phase 1 should avoid replacing that system. Instead:

- Add a small startup loader that applies launcher settings before the main menu appears.
- Let the existing in-game settings UI continue to work.
- When in-game settings are changed, optionally write back to the same shared JSON file later.

This keeps the implementation low-risk.

## Build Pipeline

There are two build outputs:

1. Main game build.
2. Launcher build.

Recommended project setup:

- Keep the launcher inside the same Unity project at first.
- Add `Assets/Scenes/Launcher.unity`.
- Create an editor build script that builds only the launcher scene to a separate folder.
- Create another editor build script that builds the full game scenes.

Example build folders:

```text
Builds/Windows/Game/
Builds/Windows/Launcher/
Builds/Windows/Package/
```

Then copy the game folder beside the launcher executable for final packaging.

## Same Project vs Separate Project

### Same Unity Project

Recommended for Phase 1.

Pros:

- Reuses existing fonts, logo, colors, audio, and UI assets.
- Faster implementation.
- Easier to keep branding consistent.
- One repository to maintain.

Cons:

- Launcher build can accidentally include unnecessary assets if build settings are not controlled.
- Needs a careful build script to keep it lightweight.

### Separate Unity Project

Better later if the launcher becomes advanced.

Pros:

- Smaller launcher build.
- Cleaner dependency separation.
- Easier updater architecture.

Cons:

- More setup.
- More duplicated assets.
- More maintenance.

## Phase 1 Implementation Plan

1. Create `Assets/Scenes/Launcher.unity`.
2. Create launcher UI with:
   - USM/APEX logo.
   - Play button.
   - Settings panel.
   - Info panel.
   - Patch notes panel.
   - Quit button.
3. Add scripts:
   - `Assets/Script/Launcher/LauncherSettings.cs`
   - `Assets/Script/Launcher/LauncherSettingsStore.cs`
   - `Assets/Script/Launcher/LauncherController.cs`
   - `Assets/Script/Startup/GameStartupSettingsLoader.cs`
4. Add a simple JSON settings file flow.
5. Add game executable detection.
6. Add error message if the executable is missing.
7. Add editor menu:
   - `E-Lab Prime/Build Launcher`
   - `E-Lab Prime/Build Game`
   - `E-Lab Prime/Build Windows Package`
8. Build locally and test:
   - Launcher opens.
   - Settings save.
   - Play launches game.
   - Game applies settings.
   - Quit exits cleanly.

## Phase 1 UI Layout

Recommended first screen:

```text
+------------------------------------------------------------+
| USM APEX                                      v0.1.0        |
|------------------------------------------------------------|
|                                                            |
|  E-LAB PRIME                                               |
|  Digital Science Simulations                               |
|                                                            |
|  [ PLAY ]                                                  |
|                                                            |
|  Settings                                                  |
|  Resolution:   [1920 x 1080 v]                             |
|  Display:      [Fullscreen v]                              |
|  Quality:      [High v]                                    |
|  Volume:       [========----]                              |
|  Language:     [Malay v]                                   |
|                                                            |
|  Info / Patch Notes                                        |
|  - Latest lab simulations ready.                           |
|  - Student progress is saved in-game.                      |
|                                                            |
|  [Open Save Folder] [Reset Settings] [Quit]                |
+------------------------------------------------------------+
```

## Premium Polish Ideas

Add these after the basic launcher works:

- Animated boot-line background matching the studio splash.
- Soft hover sound on controls.
- Confirm sound when pressing Play.
- Small "System Ready" status indicator.
- Last launched timestamp.
- Current installed version.
- Build channel label such as `School Lab Build`.
- Small rotating science fact or safety reminder.
- Professor guide mini portrait in the info panel.
- "Recommended settings detected" button.
- Auto-detect native monitor resolution.

## Error States

The launcher should gracefully handle:

- Game executable missing.
- Settings file cannot be written.
- Unsupported resolution.
- User tries to launch twice.
- Game process fails to start.
- Patch notes file missing.

Example user-facing errors:

```text
Game executable was not found.
Please reinstall E-Lab Prime or place the launcher beside the game folder.
```

```text
Settings could not be saved.
The game will launch with default settings.
```

## Packaging Notes

For a school/lab deployment:

- Keep all files in one folder.
- Avoid requiring admin permission.
- Store user settings in LocalLow.
- Keep save data separate from installation files.
- Make the launcher work offline.
- Avoid remote update features until the game is stable.

For Steam:

- Steam usually launches one executable.
- The launcher can be the executable Steam opens.
- The launcher then starts the actual game executable.
- Achievements/cloud save should still be handled by the main game.

For itch.io:

- Package the launcher as the main executable.
- Keep the game executable in a subfolder.
- Test the itch app launch behavior.

## Risks

- A Unity launcher may be larger than a native launcher.
- If built from the same project, the launcher can include unnecessary assets.
- Antivirus tools can be sensitive to launchers starting another executable.
- Auto-updating is a separate complex feature and should not be part of Phase 1.
- Store platforms may have rules about launchers, installers, and update mechanisms.

## Recommendation

Proceed with a Phase 1 Unity launcher inside the same project.

Keep it offline, simple, and polished:

- Settings.
- Info.
- Patch notes.
- Play.
- Quit.

After that works, consider Phase 2 features like news cards, student profile status, and a professor guide preview.

