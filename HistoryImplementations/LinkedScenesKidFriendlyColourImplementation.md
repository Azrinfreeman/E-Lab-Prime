# Linked Scenes — Kid-Friendly Colour Rollout Plan

Status: PROPOSED — planning only. No scene, script, material, configuration, or build-list changes until approved.
Date: 10 September 2026

## Goal

Extend the implemented MainMenu colour language across the scenes shown in the user's Scene List screenshot. Reuse the existing KidFriendlyPalette rather than introducing another theme. Make menus, lab UI, instructions, controls, and decorative presentation consistently bright and welcoming while preserving experiment readability and behaviour.

## Scene scope and build-list discrepancy

The screenshot is the explicit scope reference for this proposal: 11 scenes, including MainMenu as the existing baseline. All paths below are under Assets/Scenes/.

| Screenshot order | Scene | Planned treatment |
|---|---|---|
| 0 | Boot.unity | Loading/splash UI, progress and status presentation; preserve branding artwork and timing |
| 1 | MainMenu.unity | Reference palette and regression check; retain its approved appearance |
| 2 | Login.unity | Light panels/inputs, bright actions, readable placeholders and validation messages |
| 3 | Lab1_3D.unity | Available HUD, instructions, notebook, quiz, pause/settings, result and return controls |
| 4 | Lab2_3D.unity | Same shared UI roles; inspect scene-specific controls and runtime feedback |
| 5 | Lab3_3D.unity | Same shared UI roles; preserve scientific heat/water indicators |
| 6 | Lab4_3D.unity | Same shared UI roles; preserve experiment measurements and cues |
| 7 | Lab5_3D.unity | Same shared UI roles; preserve experiment measurements and cues |
| 8 | Lab6_0.unity | Same shared UI roles, mapped to its actual hierarchy |
| 9 | Lab7.unity | Same shared UI roles, mapped to its actual hierarchy |
| 10 | Lab8.unity | Same shared UI roles, mapped to its actual hierarchy |

The saved ProjectSettings/EditorBuildSettings.asset currently lists 18 enabled scenes. Its additional entries are Lab1, Lab2, Lab3, Lab4_0, Lab5, Lab5_0, and Lab6. These seven are excluded from this proposal because they are absent from the supplied screenshot. The discrepancy may reflect a different profile or saved state; its cause has not been established.

Before implementation, verify the active build profile and scene list through Unity. Keep the 11-scene allowlist unless the user expands it. Do not reorder, remove, enable, or disable build entries. Check scene transitions by path/name and existing bindings; do not assume the screenshot indexes match saved build indexes.

## Existing palette to reuse

| Role | sRGB hex | Use |
|---|---|---|
| Sky | #EAF7FF | Light background and decorative washes |
| Surface | #FFFEF7 | Main panels and readable content cards |
| Slot | #DDF1FF | Inputs, nested surfaces and secondary controls |
| Ink | #183153 | Main text and icons |
| Muted | #405777 | Supporting text on sufficiently light surfaces |
| Yellow | #FFD84D | Primary actions and selection |
| Blue | #55C4F5 | Supporting accents |
| Mint | #66DDB0 | Progress and success surfaces |
| Lavender | #B6A0F5 | Settings and supporting accents |
| Coral | #FF927F | Exit, reset, or attention controls |
| Neutral | #D6E1EB | Disabled/locked backgrounds |
| Success / warning / error text | #176447 / #805000 / #AC303C | Readable feedback on light surfaces |

KidFriendlyPalette currently converts its authored colours for the project's colour space. Reuse those values for compatible UI rendering; do not apply another linear conversion. Confirm actual rendering for world-space UI, legacy Text, TMP, UI Toolkit, sprites, and materials before sharing the same conversion path. UI palette values are not a universal replacement for material/shader colour inputs.

Navy text on the accent surfaces remains the default. Retain status icons/labels so meaning does not rely on colour alone. Configure normal, hover/focus, pressed, selected, and disabled states together.

## Coverage and boundaries

Include every relevant visible and inactive UI state that actually exists in each target scene: tutorials, objectives, measurements, notebooks, quiz questions and feedback, pause/settings, confirmations, results, loading, and return navigation. Absence of a feature is recorded as not applicable; do not create new game features just to complete a styling checklist.

Include non-scientific decorative surfaces where they materially affect the scene's overall mood: panel frames, non-functional trims, background art/washes, and selected furniture/environment accents. Inventory their materials and rendering first; use scene-specific variants where required. Keep the lab environment calm enough that apparatus and interactive objects remain easy to distinguish. Do not globally recolour every mesh, brighten exposure, or replace lighting profiles as a shortcut.

Preserve scientific and functional colour meanings: chemicals, flames, temperature states, water/ice, polarity/wiring, measurement diagrams, warning lights, correct/incorrect semantics, and interaction cues. Translate a UI feedback colour to a readable semantic palette role only after confirming its meaning. Keep logos, professor artwork, texture detail, and experiment illustrations unless a separately reviewed decorative variant is required.

Layout, wording, fonts, game rules, physics, camera behaviour, audio, opening timing, scoring, accounts, and progress storage remain unchanged. If a pre-existing overlap becomes apparent, report it separately; do not quietly turn the colour rollout into a layout rewrite.

## Inspection before editing

1. Confirm active project/profile, current open scenes, dirty state, and the approved 11-scene allowlist. Preserve current user work before switching scenes; do not save unrelated dirty changes.
2. Record a compact baseline per scene: UI roots, component/rendering types, relevant sprites/materials, shared prefabs, colour writers, animations, and existing errors. Use scoped UnityTaskSnapshot queries rather than full scene dumps.
3. Map each targeted element to a palette role using a scene-specific manifest. Include full paths or validated references; duplicate names must be reported as ambiguous.
4. Trace runtime updates: OnEnable/Start, tutorial stages, selection, quiz answers, validation, notebook feedback, progress updates, and rich-text colour tags. Search only scripts actually referenced by the scene plus their relevant dependencies.
5. Inspect shared assets and consumers outside the allowlist. Prefer local overrides/copies over changing a shared asset that would recolour excluded scenes.

Source inspection already found LoginManager status colours, notebook feedback colours, quiz rich-text colour tags, and tutorial highlight colours. Their presence establishes likely override locations, not proof that every script is attached to every target scene. Attachment mapping is implementation-stage work.

## Implementation approach

### Shared roles, explicit scene adoption

Keep Assets/Script/KidFriendlyPalette.cs as the colour source. Add a small role-binding mechanism only where useful for reuse, proposed as Assets/Script/UI/KidFriendlyThemeBinding.cs. Bind the intended graphic and role explicitly; do not scan every frame or use global name-based recolouring. Runtime feedback writers must cooperate with these roles rather than being overwritten in LateUpdate.

Add a scene-scoped editor utility, proposed as Assets/Script/Editor/KidFriendlySceneColourSetup.cs, with separate inventory/dry-run, apply, and verify operations. Its allowlist contains the 11 approved scene paths. Reuse the proven MainMenu setup where appropriate rather than generalizing its hardcoded panel names into lab scenes.

Dry-run should list expected changes, skipped scientific/artwork targets, ambiguous targets, and shared-asset dependencies. Apply must validate critical targets, support Undo, preserve references, and be repeatable without duplicate objects or stacked overlays. Stop a scene's application on missing critical mappings; do not report partial application as complete. Saving is a separate explicit step after verification.

Update scene-specific runtime colour fields and relevant setup defaults so entering a scene, opening a panel, or rerunning setup cannot restore the old styling. Shared runtime scripts must opt into the theme by an explicit binding/scope, keeping excluded scenes unchanged. Avoid blanket text replacement of every Color expression.

### Efficient rollout sequence

1. Boot and Login: establish entry-flow consistency and validate error/success/loading states.
2. Lab1_3D: pilot the lab UI role mapping and any decorative environment treatment.
3. Lab2_3D through Lab5_3D: reuse verified patterns with individual scene mappings.
4. Lab6_0, Lab7, Lab8: adapt mappings to their actual scene structure.
5. MainMenu regression check, complete scene-transition review, and final scene coverage report.

Approval of this plan authorizes the complete scoped rollout, not a separate permission request for every scene. Use internal verification checkpoints between batches. Ask only if a material scope conflict or destructive/unrecoverable action requires a user decision.

Batch coherent source edits, compile once when needed, and run focused checks. Process Unity scene changes sequentially. Do not repeatedly regenerate assets or capture unchanged views. No model/configuration changes or package upgrades are included.

## Validation and completion criteria

For each scene, record passed, failed, skipped, or not-applicable for:

- Expected surfaces, typography, controls, feedback and decorative targets mapped and applied.
- Runtime states retain their colours after opening/closing panels and changing steps.
- Hover/focus/pressed/selected/disabled states are distinguishable; labels remain readable.
- Normal text contrast targets 4.5:1, large text and essential control boundaries 3:1. Inspect actual backgrounds, including world-space and translucent presentation.
- Scientific colour cues, materials, labels, measurements and apparatus remain meaningful.
- No missing scripts/references or broken persistent event targets/signatures. Structural helper checks alone do not prove behavioural correctness.
- Relevant navigation and experiment interactions work; no new compilation or runtime errors attributable to the change.
- Reference resolution plus a smaller supported resolution and another supported aspect ratio reviewed for affected UI.
- Baseline/final images linked by exact returned paths; final images match saved scene content.

Validate the asynchronous capture helper in an authorized preview before using it as acceptance evidence. It is currently provisional. Do not use the known recursive EditorApplication.Step screenshot path repeatedly; use a supported alternative or clearly report a capture limitation.

Use isolated test state or non-writing presentation probes for account, quiz and progress states. Never reset real progress, unlock achievements, or submit real accounts for appearance checks. If a runtime probe writes state, isolate it before execution rather than trying to repair real data afterward.

At the end, verify all 11 scoped scenes and the existing linked navigation, confirm excluded scenes/shared assets were not accidentally changed, and restore the user's original editor scene setup where possible. If shared runtime scripts changed, run a relevant build/compilation check and report the exact validation performed. Do not claim a full platform build if none was run.

## Deliverables after approval

- Consistent palette across all 11 screenshot scenes, with MainMenu preserved as the reference.
- Shared roles and scoped editor tooling only as necessary; updated runtime writers/setup defaults.
- Scene coverage manifest, including reviewed decorative surfaces and protected scientific assets.
- Final screenshots and a concise verification report under output/linked-scene-colours and HistoryImplementations.
- Updated project guidance describing the approved scene coverage.

Implementation approved and applied on 10 September 2026. See LinkedScenesKidFriendlyColourReport.md for coverage, evidence and outstanding validation limits.
