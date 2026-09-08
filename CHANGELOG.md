# Changelog

## 1.3.0 - Unreleased

- Add project-scoped preferences, calm editor appearance, searchable recent/favorite tools, stable refresh navigation, accessible action states and a composable task-layout sample.

## 1.2.0 - 2026-08-31

- Replaced the Project Setup surface with the responsive Deucarian Control Center workbench.
- Added explicit card, section, and stable tool registries with deterministic ordering, search, bounded snapshots, and failure isolation.
- Added governed Control Center and Advanced menu entries, conditional areas, readiness focus, and a one-release obsolete API redirect.
- Renamed Project Settings and validation guidance to Deucarian Control Center and added registry, search, menu, window, and lifecycle tests.

## 1.1.0 - 2026-08-26

- Added package-contributed project checks with stable issue codes and an
  actionable Project Setup UI.
- Added shared Play Mode, build preprocessing, and command-line CI gates.
- Added package-metadata footer resolution so editor workflows no longer need
  hardcoded installed versions.

## 1.0.5 - 2026-07-19

- Added the approved Tideline light/dark mark, logo, and quiet wallpaper assets as package-owned editor resources.
- Added DINish Light, Regular, and SemiBold under the SIL Open Font License 1.1 with shared display, body, and strong typography roles.
- Reworked the shared UI Toolkit and IMGUI shell around semantic light/dark Tideline colors and introduced full-color brand-header helpers without changing the existing package-header APIs.
- Added the complete Grove, Cobalt, Tideline, Oxblood, and Mineral territory palette plus shared graph surfaces, interaction states, edges, and package-status roles for downstream editor tools.

## 1.0.4 - 2026-07-17

- Expanded the package-owned Lucide 1.22.0 catalog with generic action, status, navigation, package, graph, platform, and workflow glyphs, each backed by its exact upstream SVG and a matching 32 px white PNG.
- Added safe dynamic Lucide lookup, canonical package fallback behavior, direct icon-ID package lookup, and complete Lucide mappings for the legacy package aliases.
- Added stable shared icon constants, Lucide status rows, and a branded responsive callback-based editor dialog with wrapped details, icon actions, and deterministic Enter, Escape, and window-close completion.

## 1.0.3 - 2026-07-17

- Added reusable selection-and-ping editor helpers and an isolated editor-shell example.
- Qualified Unity object references in the selection helper test for Unity 6000 compiler compatibility.

## 1.0.2 - 2026-07-15

- Preserved the released Package Installer IMGUI status-row color composition while retaining exception-safe restoration of `GUI.contentColor` in the shared workbench helper.

## 1.0.1 - 2026-07-15

- Added a domain-neutral hybrid workbench scaffold with exact 900/1180 responsive modes, shared toolbar/drawer/footer USS contracts, and Package Installer compatibility selectors.
- Added shared Installer-calibrated IMGUI surface styles, 24 px action styles, key/value and status rows, and exception-safe card, inline-card, foldout, and panel scopes.
- Added shared frosted-glass editor theme primitives for Deucarian product windows: ambient wallpaper layers, glass cards, sidebar items, branded buttons, status panels, spacing tokens, texture helpers, and fixed wallpaper chrome.
- Standardized the canonical Deucarian editor menu root constant on `Tools/Deucarian`.

## 1.0.0 - 2026-06-22

- Marked the shared Deucarian Editor package as the current 1.0.0 editor-helper surface.
- Kept the documented editor chrome, status badge, icon, asset field, and menu helper APIs unchanged from the existing release stream.

## 0.1.2 - 2026-06-15

- Added reusable fixed-rect status badge drawing helpers for row and table UIs.
- Exposed the shared status badge style factory for packages that need custom layout.

## 0.1.1 - 2026-06-15

- Documented `Tools/Deucarian/<PackageName>/...` as the package-owned tooling menu convention.
- Clarified that the Package Installer also lives under the shared `Tools/Deucarian/...` tooling root.
- Added `PackageToolMenuRoot` and updated UX menu path helpers to build `Tools/Deucarian/...` paths.

## 0.1.0 - 2026-06-15

- Added the initial editor-only Deucarian Editor package.
- Added fixed Deucarian editor colors, styles, icon fallback handling, chrome helpers, asset field helpers, status badges, and layout helpers.
- Documented Deucarian editor UX standards for menus, asset fields, and actions sections.
- Added editor tests covering constants, style accessors, icon fallbacks, status helpers, and asset field API availability.
