# UI guidance

MainMenu uses KidFriendlyPalette and MainMenuKidFriendlyColourSetup. Keep the current bright sky, warm-white, yellow, mint, lavender, coral, and dark-text roles. The approved rollout also covers Boot, Login, Lab1_3D, Lab2_3D, Lab3_3D, Lab4_3D, Lab5_3D, Lab6_0, Lab7 and Lab8 through KidFriendlySceneTheme and KidFriendlySceneColourSetup. Other scenes still require explicit scope.

Preserve fonts, sizes, layout, and scientific imagery unless the task requests changes. Keep existing BouncyButton behaviour. Verify both saved component colours and runtime colour writers. Check normal, focus, pressed, selected, disabled, and semantic states. Use labels/icons as well as colour.

Prefer readable dark text on bright surfaces; measure actual rendered contrast when important. Palette colours account for this project's linear colour space: do not apply a second linear conversion. Never edit shared TMP font materials to fix a scene-local label. Retain correct sprite slicing; the MainMenu slider handles use Simple rendering with preserved aspect because the previous sliced sprite disappeared.

Avoid universal Image recolouring: masks, heat/water indicators, professor art, and thumbnails need separate handling. Preserve event wiring and user progress. Re-run relevant setup tools only when required; the menu colour tool is available separately and does not save automatically.

