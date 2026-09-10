# Kid-Friendly Colour Implementation Plan

Status: IMPLEMENTED for MainMenu after user approval. See MainMenuKidFriendlyColourVerification.md for results and review limits.
Date: 10 September 2026
First target: `Assets/Scenes/MainMenu.unity` (confirmed active in Unity).

## Goal and scope

Give E-Lab Prime a bright, colourful, welcoming science-game identity for children. Establish the colour language in the main menu first, including its settings, achievements, activity selection, account views, session badges, professor dialogue, and opening overlay. Review the completed main menu before applying the direction to other scenes.

This phase is a colour treatment: retain existing layout, wording, navigation, animations, professor artwork, account behaviour, progress data, and experiment content. No gameplay or other scene changes are part of phase one.

The user's bright, kid-friendly direction supersedes the dark-glass colour requirements in `.agents/AGENTS.md` for this work. Keep its useful requirements for readable typography, rounded UI, responsive controls, defensive implementation, and verification. Do not rewrite that guidance during this planning step.

## Findings from project inspection

- The active scene uses Canvas UI and includes `PanelLogin`, `AmaliPanel`, `PanelPengaturan`, `PanelPencapaian`, `PanelAkaun`, `SIdemenu`, `SessionStatusStrip`, `ProfessorDialoguePanel`, and `OpeningSequenceOverlay`.
- `Assets/Script/Editor/MainMenuSceneSetup.cs` applies dark translucent panels and cards, white text, and a blurred classroom background. Running it unchanged could restore the old appearance.
- `Assets/Script/MenuManager.cs` defines `MainMenuManager`; its navigation colours are also serialized in the scene. Changing script defaults alone will not replace those saved values.
- Account and session scripts apply colours during state changes. Their colours must be included so the menu stays consistent after login, logout, tab changes, and navigation.
- Professor, session, account, and home-navigation setup tools contain additional styling defaults.

Inspection was limited to scene structure and source data. Full rendered screenshots and contrast measurements remain implementation-stage checks; this document does not claim visual validation of the current menu.

## Proposed visual direction

Use a soft sky-blue background, warm white cards, strong navy lettering, and cheerful colour accents. Keep large surfaces light and calm so colourful actions and science illustrations stand out. Preserve the existing rounded shapes and button motion. Avoid full-screen neon saturation or a different random colour for every element.

### Palette

Hex values below are proposed sRGB colours, opaque unless specified.

| Role | Colour | Hex | Intended use |
|---|---|---|---|
| Sky base | Pale sky blue | `#EAF7FF` | Background base and empty areas |
| Main surface | Warm white | `#FFFEF7` | Main panels and dialogue cards |
| Secondary surface | Light blue | `#DDF1FF` | Input backgrounds, nested cards, idle navigation |
| Main text | Deep navy | `#183153` | Titles, labels, button text |
| Secondary text | Slate blue | `#405777` | Descriptions and placeholders |
| Primary accent | Sunshine yellow | `#FFD84D` | Main play/activity action and selected navigation |
| Blue accent | Bright sky blue | `#55C4F5` | Home and supporting actions |
| Green accent | Fresh mint | `#66DDB0` | Success, progress fills, student badge |
| Purple accent | Soft lavender | `#B6A0F5` | Settings and account decoration |
| Warm accent | Coral | `#FF927F` | Small warm accents and exit/attention controls |
| Neutral state | Blue-grey | `#D6E1EB` | Disabled or locked surfaces |
| Success text | Deep green | `#176447` | Success messages on light backgrounds |
| Warning text | Deep amber | `#805000` | Warning messages on light backgrounds |
| Error text | Deep red | `#AC303C` | Error messages on light backgrounds |

Use navy text on bright accent fills. Reserve white lettering for surfaces proven dark enough. Status text uses the deeper semantic colours, not pale decorative accents. Validate final composited colour pairs before accepting them.

## Main menu application

| Area | Proposed treatment |
|---|---|
| Background | Keep the classroom theme; introduce the sky base and a light sky wash. Inspect the existing bitmap first: Image tint alone cannot brighten baked-in dark pixels. If it remains muddy, prepare a separate menu-only background variant for review. |
| Home / `PanelLogin` / `Middle` | Warm white panels, navy headings, sunny primary activity action; blue, lavender, and coral supporting accents assigned by function. |
| `SIdemenu` | Light surface with navy icons; pale blue idle controls and yellow selected controls. Keep a persistent selection marker so selection is not communicated only by colour. |
| `AmaliPanel` | Light cards with repeating blue, mint, lavender, and yellow accent bands. Preserve experiment thumbnails and meaningful scientific colours. Locked cards use a visible lock and label with readable neutral styling. |
| `PanelPengaturan` | Warm white panel, lavender section accents, mint slider fills, pale blue tracks, and navy labels. Selected dropdown and toggle states remain obvious. |
| `PanelPencapaian` | Light rows, sunny reward accents, mint completed states, neutral unfinished states. Keep completion labels/icons and readable progress values. |
| `PanelAkaun` | Light input fields, navy typed text, slate placeholders, yellow active tab, lavender decorative accents. Use deep green/amber/red for status text. Include signed-out, registration, guest, and logged-in views. |
| `SessionStatusStrip` | Light strip; mint student pill, yellow guest pill, blue-grey signed-out pill, all with navy text and explicit state labels. |
| Professor guide | Warm white dialogue surface, lavender/blue edging, navy speech text and readable subtitles, yellow continue control. Preserve avatar colours and existing highlight timing. |
| Opening and confirmation overlays | Align visible UI cards and controls to the palette; retain enough dimming behind modal content and preserve the opening sequence's timing and artwork. |

## Interaction and readability rules

- Define normal, hover, pressed, selected, and disabled states for every button, tab, toggle, dropdown, and slider that exists in this scene.
- Hover may lighten a fill slightly; pressed may deepen it slightly. Selected navigation retains a marker; disabled controls keep readable labels and cannot look selected.
- Check Image base colours together with Selectable colour tint, animation-driven colours, and runtime assignments; avoid accidental double tinting.
- Target at least 4.5:1 text contrast for normal text and 3:1 for large text and essential control boundaries. Measure against the rendered background, particularly for translucent layers.
- Keep existing fonts and sizes initially. Avoid thick text outlines and avoid relying on colour alone for success, warning, errors, locks, or selection.
- Do not globally tint all Images: illustrations, avatars, masks, scientific diagrams, and transparent utility graphics need separate treatment.

## Implementation sequence after approval

1. Capture the main menu baseline and record existing changes before editing. Inventory active and inactive UI, runtime colour writers, animation colour tracks, and shared asset references. Do not save over unrelated unsaved scene changes.
2. Establish one reusable palette definition, proposed as `Assets/Script/UI/KidFriendlyPalette.cs`, with named surface, text, accent, and semantic roles. Keep its first application explicitly limited to MainMenu. Prefer existing serialized colour fields for runtime consumers where possible.
3. Add a focused, repeatable colour application tool, proposed as `Assets/Script/Editor/MainMenuKidFriendlyColourSetup.cs`. Verify the scene identity, resolve targets recursively including inactive children, validate expected components, log missing targets, and support Undo. Apply only colour styling and necessary menu-only sprite variants; do not rebuild the hierarchy or event wiring.
4. Apply the surface and typography pass first, then action accents, interaction states, inputs, badges, guide UI, and overlays. Keep rounded/sliced sprites correctly configured.
5. Update saved navigation, account, and session colour fields together with any necessary script-level colour writers. Review `MenuManager.cs`, `MainMenuAccountPanelController.cs`, `MainMenuSessionStatusUI.cs`, and `MainMenuProfessorGuide.cs` for overrides; change only relevant colour handling.
6. Align relevant editor defaults in `MainMenuSceneSetup.cs`, `MainMenuHomeNavButtonSetup.cs`, `MainMenuAccountPanelSetup.cs`, `MainMenuLoggedInAccountPanelSetup.cs`, `MainMenuSessionStatusSetup.cs`, and `MainMenuProfessorGuideSetup.cs`. Ensure later setup runs cannot reinstate the old theme. Do not run broad rebuild tools just to recolour the menu.
7. Inspect shared prefabs, materials, textures, and TMP font materials before editing. Use scene overrides or menu-specific copies when a shared change could affect other scenes. Do not change global lighting, render settings, or common science assets for this UI pass.
8. Verify the result in Unity, save only intended changes, and present before/after images of home and the major panels for user review. Stop before rolling out the theme to other scenes.

Proposed filenames are implementation choices, not existing files. Reuse a suitable existing palette mechanism if one is found during the detailed inventory.

## Acceptance checks after implementation

- Home, activity selection, settings, achievements, account views, session badges, professor guide, and menu overlays consistently use the approved light palette.
- Inspect at the project's reference resolution, a smaller supported window, and another supported aspect ratio. Text, icons, controls, and guide content remain readable and unclipped.
- Exercise hover, press, keyboard/controller focus where supported, selection, disabled states, and panel close/reopen. Check that runtime transitions never restore dark styling.
- Verify guest, signed-out, signed-in, registration, and validation-message presentation without modifying real user progress or accounts; use disposable test state where required.
- Verify sliders, toggles, dropdowns, navigation, activity launch, opening sequence, professor highlights, and existing button motion. Verify scene return where practical without altering lab assets.
- Confirm no missing references or new compilation/runtime errors. Run existing relevant tests if available; use focused runtime interaction checks for the visual change.
- Confirm no unintended modifications to other scenes or shared visual assets. Rerunning the colour tool must not duplicate objects or change behaviour.

## Later rollout — separate stage

After the user reviews and accepts the main menu, inventory the remaining scenes and propose their order. Carry the approved palette roles into Login/Boot presentation and then lab UI, preserving each experiment's functional colour meanings. Review each scene before moving on. Do not recolour chemicals, heat indicators, wiring, diagrams, or correctness cues merely to match decorative colours.

## Deliverables

Now: this implementation Markdown plan only.

After approval: a recoloured MainMenu, reusable palette and scoped application tooling where needed, consistent menu runtime/editor colours, before/after screenshots, and a concise verification report. Other scenes remain a later review stage.
