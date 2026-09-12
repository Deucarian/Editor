# Changelog

## [1.13.0] - Unreleased

- Unify package/project asset selection, explicit create/customize/default actions and origin hints. Retain native object fields across dependent content updates and scale Overview search rows with workspace typography.

## [1.12.0] - Unreleased

- Keep Overview review navigation on one project-check list, including issues contributed by domain sections; filter by severity without switching pages, preserve filters on refresh, and name explicit tool destinations.
- Add shared asset/declarative-C# definition synchronization, stable identity and conflict handling, domain schema adapters, Definitions UI, generated assembly setup and build validation.
- Align declared package dependencies with the definition-authoring development wave.

- Add subtle background and accent-icon feedback when hovering or keyboard-focusing sidebar category headings; preserve selected tool styling and layout.

- Rebase Control Center 100% to the previous 75% size (after the earlier 90% rebase), retaining fixed-size scale controls and migrating non-default preferences.
- Cover the new baseline, preference migration, fixed controls and viewport layouts with editor tests.
- Add a compact, scoped Project Settings shell for package providers without changing Control Center page styling.
- Keep feature switches compact in Project Settings and isolate its custom-font text rendering from Unity 6's ATG worker-thread failure.
- Support real rendered scene textures in spatial previews and a wrapping, evenly sized navigation toolbar.


## [1.11.0] - 2026-09-11

- Keep Advanced reachable while sidebar categories expand, with independent scrolling for the tool list.
- Give primary, secondary and quiet buttons equal dimensions and aligned baselines.
- Describe the Component Gallery, project settings and Control Center by their actual editor tasks.
- Expose camera-projected preview geometry for interactive package-owned navigation sessions.
- Share one final notification preview between Test and Appearance, with generic row appearance controls.
- Add layout regressions for expanded navigation, button alignment and interactive preview composition.

## [1.10.8] - 2026-09-11

- Include Unity 2021.3's editor field namespace for the Control Center appearance dropdown, completing the numeric and choice-field compatibility fixes.

## [1.10.7] - 2026-09-11

- Import the editor UI field namespace used by the numeric stepper on Unity 2021.3, while preserving Unity 6 behavior and the shared visual design.

## [1.10.6] - 2026-09-11

- Match loading skeletons, package change rows, commit placement and history cards to the design references.
- Keep long file paths and status text separated, use shared checkbox styling, and retain commit drafts below both review panes.
- Add an inline presentation for existing workflow steps and responsive review geometry coverage.

## [1.10.5] - 2026-09-11

- Preserve per-window navigation expansion, selection visibility and scroll position across pages, search and UI scales.
- Keep compact page titles, project-name ellipsis, read-only baselines and disabled icon-button colors consistent.
- Constrain notification preview columns and scope spatial toolbar styling to its own component; cover 28 responsive size/scale combinations.
- Clarify Advanced checks without duplicating domain tools, and finish native Inspector, authoring, gallery and sample presentation.
- Extend shared controls with dark/light contrast, attached geometry, source-aware collection and spatial-preview regressions.

## [1.10.4] - 2026-09-11

- Match centered session panels, side-by-side navigation previews and anchored collection actions to the approved designs.
- Clip spatial-preview meshes to their own viewport and correct face winding, with zoom and geometry regression checks.
- Keep contextual operation activity out of the stationary scale dock and add the licensed Lucide user-session icon.

## [1.10.3] - 2026-09-11

- Align native filters, paneled collections, trailing switches and action rows with the approved reference compositions.
- Give code examples and change reviews a bundled, licensed monospace font.
- Keep empty descriptions out of status/list layouts and correct compact operation-footer button typography.

## [1.10.2] - 2026-09-11

- Preserve Overview content on repeated navigation and keep form/preview columns at their intended widths.
- Keep keyboard-focused controls visible inside scaled scrolling forms, including narrow layouts.
- Refine palette context, color-field proportions and spacing using the shared visual system.
- Keep notification playback with its owning package; the shared lab exposes a preview region and tab event.

## [1.10.1] - 2026-09-11

- Keep wrapped headings and context filters scrollable without starving page content at large UI scales.
- Replace the unsupported last-child style selector with an explicit navigation-arrow class.
- Avoid hiding Unity's built-in slider fill member with the shared fill renderer.

## [1.10.0] - 2026-09-11

- Recompose the shared workspace and native forms around the approved visual references: charcoal gradient, typography, Lucide icons, reusable buttons and fixed colored scale dock.
- Add native serialized Inspector styling with source/Undo/multi-selection coverage; retain legacy adapters for compatibility.
- Keep project settings and page navigation inside the current Control Center, with persistent Advanced expansion state.
- Support integrated Workspace / Changes / History composition, literal colored diffs, explicit staging and caller-owned publishing controls.

Visual acceptance and the portfolio consumer rollout are tracked separately; this package does not own domain actions.

## [1.9.1] - 2026-09-10

- Keep shared switches compact in stretched form columns and give slider value fields enough height to remain readable.
- Cover resolved control geometry in narrow and wide attached Inspector layouts.

## [1.9.0] - 2026-09-10

- Unify charcoal/teal surfaces, reusable button roles, colored sliders, switches and submenu icons. Keep the compact 100% baseline and stationary scale dock.
- Add capability-disabled presentation and optional step-based change review without owning package policy.

## [1.8.1] - 2026-09-10

### Added

- Shared change-review controls for existing Control Center pages: contextual forms, explicit actions, selected staged/unstaged rows, read-only diffs, and optional compact history.
- Keyed refresh preserves surviving controls and focus without running commands; display bounds, binary/partial-diff notices and responsive layout stay owned by Editor.
- Contract and live layout coverage for selection, refresh, narrow/scaled rendering and disposal. Domain owners retain Git, validation, source switching and session state.
## [1.8.0] - 2026-09-10

- Add composed feature sections with accessible on/off controls, aligned settings, truthful connection states and responsive layouts.
- Allow domain packages to contribute a group icon and concise submenu labels without changing tool identities or window titles.
- Keep styling in Editor and commands in consuming packages. The existing compact default scale and fixed scale dock are unchanged.

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
