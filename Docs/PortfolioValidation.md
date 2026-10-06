# Portfolio documentation validation

Reviewed on **2026-10-06**, against commit `f4ef9c20fb99dc174f979d08ee9fa67f9d4742aa`.

## Passed source and documentation checks

- Confirmed Unity `6000.5.9f1`, URP `17.5.0`, Input System `1.20.0`, Cinemachine `3.1.7`, and Unity UI `2.5.0` from project/package metadata.
- Confirmed all 11 enabled scenes are tracked and Boot is first in the build list.
- Matched the eight lab topics to controller references in the enabled scenes: pendulum records, electrolysis, water heating, evaporation, dissolution, air composition, reflection, and refraction.
- Checked source descriptions for tutorials, voice guidance, notebooks, main menu, local accounts, progress, rewards, and achievements.
- Confirmed the MainMenu account prototype stores profile/password data locally in `PlayerPrefs`; the separate Login scene controller uses mock authentication. No production authentication or cloud sync is claimed.
- Reviewed relevant historical reports for the linked-scene colour rollout and Lab 2–5 textbook work. Their recorded compilation/previews are prior evidence, not new validation results.
- Verified README links and whitespace, and confirmed this change touches only README.md and this file.

## Skipped checks

Unity import/compilation, Play Mode, complete experiment and quiz flows, login interactions, persistence/reset behavior, target-device performance, accessibility, classroom content review, and platform builds were not run. Tracked builds/media were not executed or visually certified. No credential/history, binary, or asset-license audit was performed.

## Before a portfolio demonstration

1. Open the recorded Unity version, restore dependencies, and check for compile errors and missing scene references.
2. Use isolated test data to exercise Boot, MainMenu, Login, and all eight enabled lab activities. Verify tutorials, voice replay, apparatus interactions, records, graphs, quizzes, rewards, and return navigation as applicable.
3. Review simulation assumptions and instructional content with a qualified educator; distinguish expected reference values from actual user records.
4. Check local profile storage and reset controls. Replace prototype password handling before real accounts or sensitive student data are introduced.
5. Validate the intended build and device, then capture representative screenshots/video and publish results with their date and limitations.
6. Before any public release, separately review repository history, stored artifacts, personal data, and third-party asset permissions. The repository remains private during this documentation work.
