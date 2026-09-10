# Level-select card redesign

Redesigned the eight MainMenu cards with light tinted surfaces, coloured accents, compact science diagrams drawn using Unity UI, actual experiment titles and Malay descriptions. Replaced generic ACTIVE with MULA / SAMBUNG / SELESAI according to existing progress. The progress value is clamped to 0–100% for display. No saved progress or unlock rules were changed.

Old cropped thumbnail children are retained inactive; the existing card buttons and their event bindings remain in place. A repeatable LevelCardPolishSetup tool builds the new content and is called by the MainMenu palette setup. The menu grid and navigation are preserved.

Validation: scripts compiled and console checks reported no errors. Runtime inspected all eight 480 × 140 cards: no text overflow. Initial runtime screenshot reviewed; final pass added subtle surface tints. Existing professor welcome overlay was present in the preview and was not redesigned. Full navigation into each lab and alternate aspect ratios were not tested.

Evidence: output/level-cards/before/ contains backups; runtime preview is output/agent-captures/e940e6ea7f3b45f5bd0741d296bd6748/capture.png (before final subtle tint). Changes: MainMenu.unity, AmaliCardProgressUI.cs, LevelCardPolishSetup.cs, MainMenuKidFriendlyColourSetup.cs.
