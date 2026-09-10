### E-Lab Prime UI/UX Design Language (AAA Standard)
When creating or modifying UI elements in this project, strictly adhere to the following "Dark Glassmorphism" design language to maintain a premium, modern, AAA educational software aesthetic:

1. **Panels (Glassmorphism):** Use dark, semi-transparent frosted glass for main panels (e.g., `new Color(0.1f, 0.15f, 0.2f, 0.7f)`). Add a very subtle, highly transparent white outline edge (e.g., `new Color(1f, 1f, 1f, 0.15f)`) to create depth without clutter.
2. **Typography (Clean & Minimal):** Do NOT use thick, solid dark outlines on text. Rely on crisp, bold white fonts (`Color.white`). If contrast is needed against bright backgrounds, use a microscopic drop-shadow or a very subtle dark outline (e.g., `outlineWidth = 0.05f`, `Color(0,0,0, 0.8f)`). Links should be clean, underlined soft white text without outlines.
3. **Input Fields (Neumorphic Slots):** Input fields should appear as recessed physical slots in the glass, not solid white blocks. Use a dark, translucent color (e.g., `new Color(0f, 0f, 0f, 0.35f)`). Always ensure 9-slicing is configured correctly and set `pixelsPerUnitMultiplier` to `2f` for refined rounded corners. Use soft white placeholders (`alpha 0.5f`) and solid white for typed text.
4. **Primary Buttons (Vibrant CTA):** Use highly saturated, vibrant colors for primary action buttons (like Emerald Green `new Color(0.2f, 0.85f, 0.45f, 1f)`) to make them pop against the muted glass. Ensure text inside buttons is clean with no heavy outlines.
5. **Tactile Feedback:** Ensure interactive elements (buttons, text links) have the `BouncyButton` script attached for physical UI responsiveness.

### Defensive Programming & Preventing Silent Failures
When creating or modifying C# scripts (especially Editor setup scripts or UI configuration scripts), you MUST write defensive code that prevents silent failures:

1. **Explicit Error Logging:** Do NOT use silent early exits (e.g., `if (obj == null) return;`) for expected critical components. If a targeted `GameObject`, `Transform`, component, or asset is not found, you must log an explicit warning or error (e.g., `Debug.LogWarning($"Failed to find {name}");`) so the failure is visible in the Unity Console.
2. **Recursive Searching over Hardcoding:** When searching for UI elements or GameObjects, prefer recursive searches (like `GetComponentsInChildren`) over hardcoded paths (`Transform.Find("Parent/Child")`) to make scripts resilient to hierarchy restructuring.
3. **Asset Loading Validation:** When loading assets via `AssetDatabase`, always verify the asset was loaded successfully and is of the expected type. Log an error if the load fails.
4. **Self-Verification:** After executing a script or tool intended to modify a scene, independently verify that the intended modifications actually took effect (e.g., by inspecting the state of the scene post-execution).

### Pre-Flight Verification & Testing Rule
When implementing a new feature or making changes, you MUST perform a thorough recheck of the implementation logic before concluding. Additionally, verify that the game state or Editor state functions properly (e.g., using execute_code or un_tests in Unity) to simulate runtime testing and validate that UI interactability, logic, and component references are unbroken before handing the task back to the user.

## Web Design & Layout Guidelines (Pixenco Studio Inspired)
When creating or modifying web layouts and UI in this project, adhere to the following design language:

### 1. Color Scheme
- **Backgrounds**: Use a modern dark mode design as the base (e.g., `bg-slate-950` or `bg-slate-900`).
- **Accent Color**: Use vibrant Teal/Cyan (`teal-400`, `teal-500`, `teal-600`) for primary actions, highlights, and text gradients to contrast against the dark background.
- **Typography Colors**: Use off-white and soft grays (`text-slate-300`, `text-slate-400`, `text-white`) for standard text readability.
- **Conversion Elements**: Use bright brand green (`#25D366` - WhatsApp green) for floating chat buttons or direct contact links.

### 2. Typography
- **Font Family**: Clean, professional sans-serif.
- **Headings**: Use bold, high-impact titles, often employing teal-to-white or teal gradient text treatments for hero sections to establish visual hierarchy.

### 3. Layout Architecture
- **Header**: Implement sticky navigation with left-aligned branding, center-aligned menu anchors, and a prominent right-aligned CTA button.
- **Hero Section**: Left-aligned headline and subheadline with a primary/secondary button pair, followed by credibility stats.
- **Grids**: Use responsive grid structures for Services, Portfolios, and Pricing (e.g., 4-column layouts with checkmark lists for pricing tiers).
- **Contact Forms**: Use structured 2-column conversion sections (Contact details & value props on the left, lead capture form on the right).

### 4. Interactivity & Tech
- **Styling**: Strongly prefer Tailwind CSS utility classes for styling.
- **Animations**: Include native CSS transitions (`transition-all duration-300`), hover scaling (`hover:scale-110`, `hover:scale-[1.02]`), button ripple effects, and custom pulse/bounce animations for floating conversion buttons.
