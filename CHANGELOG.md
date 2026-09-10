# Changelog

## [1.8.0] - 2026-09-10

### Added

- Shared change-review controls for existing Control Center pages: contextual forms, explicit actions, selected staged/unstaged rows, read-only diffs, and optional compact history.
- Keyed refresh preserves surviving controls and focus without running commands; display bounds, binary/partial-diff notices and responsive layout stay owned by Editor.
- Contract and live layout coverage for selection, refresh, narrow/scaled rendering and disposal. Domain owners retain Git, validation, source switching and session state.

## [1.7.0] - 2026-09-09

### Changed

- Adopt the shared Editor 1.7 workspace presentation: neutral surfaces, readable typography, consistent actions and aligned controls.
- Preserve package workflows and native serialized editing; this is an editor-only presentation update.
- Shared IMGUI text/input/action adapters and inspector composition; embedded headers are omitted explicitly by their owning window.

## [1.6.1] - 2026-09-09

- Keep sidebar expansion and scroll state with the owning window across cached pages, without sharing state between windows.
- Treat selecting the current page as a no-op unless explicitly routing; stop the previous page before activating the next and clean up failed activations.
- Replace the duplicate Notifications mockup with an Editor Component Gallery containing real, isolated control examples.
- Keep the notification composer's pinned primary action full-width and consistently sized.
- Add switching, scroll, independent-window, recovery and gallery regression coverage.

## [1.6.0] - 2026-09-09

- Keep Control Center tool rows, search results, status actions, and project-setup links in their owning window. Build expandable submenus from installed tool registrations. Add shared embedded IMGUI and explicit command/reference page adapters; retain the fixed compact scale dock.

## [1.5.3] - 2026-09-09

- Make the previous 75% workspace size the new 100% default, with a separate project-local preference for the normalized scale.
- Keep the scale slider and reset button in an unscaled, fixed-height dock. Their position and hit targets remain stable throughout dragging and responsive reflow.
- Reserve space for the dock so scaled content never overlaps it; preserve consumer status-footer replacement.

## [1.5.2] - 2026-09-09

- Give the overview one attention-first status, concise area summaries and quieter tool rows; retain full details in sections and search.
- Add a project-local 75–150% workspace scale slider and one-click 100% reset across shared pages and windows.
- Reflow the layout in logical pixels when scaling, preserve drafts, and release preference subscriptions on detach or disposal.

## [1.5.1] - 2026-09-09

- Flatten Control Center tool rows and remove duplicate overview headings and inset containers.
- Adapt fields and split panes to their own available width, including narrow docked layouts.
- Align input text, keep detail actions content-sized, and use a single selection surface with visible keyboard focus.
- Keep warning lifetime and progress beneath message text so actions stay reachable.
- Add layout regression checks at screenshot, live-window, floating-minimum, and docked widths.

## [1.5.0] - 2026-09-09

- Keep sidebar navigation in the current workspace and retain page drafts while switching tools.
- Support explicitly opening independent workspaces through the sidebar context menu.


## [1.4.1] - 2026-09-09

### Fixed
- Resolve workspace popup and numeric fields on Unity 2021.3 as well as Unity 6.
- Keep long search prompts and field captions inside compact workspace layouts.

## 1.4.0 - 2026-09-09

- Add an Editor-owned task workspace with responsive navigation, typography,
  shared form/preview layout, keyboard-accessible choices and display-only message
  rows, composed over the existing workbench without domain dependencies.
- Add an isolated component specimen through Control Center's Developer area.
  The specimen remains isolated; domain tools connect through their own adapters.

## 1.3.1 - 2026-09-09

- Give workbench controls named files and compose style-cache lifetime separately from drawing. Reuse safe style copying and verify cache identity/invalidation.

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
