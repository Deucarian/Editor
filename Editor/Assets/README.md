# Deucarian Editor Assets

Shared editor-only UI Toolkit assets live here so Deucarian tools do not each invent their own branding paths.

The canonical Tideline brand assets live at:

- `Editor/Assets/Logos/DeucarianMarkDark.png`
- `Editor/Assets/Logos/DeucarianMarkLight.png`
- `Editor/Assets/Logos/DeucarianLogoDark.png`
- `Editor/Assets/Logos/DeucarianLogoLight.png`
- `Editor/Assets/Images/DeucarianBackgroundDark.png`
- `Editor/Assets/Images/DeucarianBackgroundLight.png`

DINish is bundled under the SIL Open Font License 1.1 for editor UI use:

- `Editor/Assets/Fonts/DINish-Light.otf` for display headings
- `Editor/Assets/Fonts/DINish-Regular.otf` for body copy and forms
- `Editor/Assets/Fonts/DINish-SemiBold.otf` for buttons, tabs, and labels
- `Editor/Assets/Fonts/DINish-OFL.txt` for the license text

Legacy placeholders remain at these paths for backwards compatibility:

- `Editor/Assets/Logos/DeucarianPlaceholderLogo.png`
- `Editor/Assets/Images/DeucarianPackageInstallerPlaceholderHero.png`
- `Editor/Assets/Icons/DeucarianPackagePlaceholderIcon.png`

Reusable UI Toolkit styles live in:

- `Editor/Assets/Styles/DeucarianEditor.uss`

Shared tintable editor icons live in `Editor/Assets/Icons/Lucide/`. Use the
stable `DeucarianEditorIconIds` constants where available, or pass a safe
vendored Lucide slug to `DeucarianEditorIcons.GetIcon`.

Package-specific UXML and USS files should stay in the package that owns the window. Shared logos, fonts, colors, graph semantics, and editor brand imagery belong in `com.deucarian.editor`; consuming packages map domain classes to those shared roles.
