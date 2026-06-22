# Changelog

## 1.0.0 - 2026-06-22

- Marked the shared Deucarian Editor package as the current 1.0.0 editor-helper surface.
- Kept the documented editor chrome, status badge, icon, asset field, and menu helper APIs unchanged from the existing release stream.

## 0.1.2 - 2026-06-15

- Added reusable fixed-rect status badge drawing helpers for row and table UIs.
- Exposed the shared status badge style factory for packages that need custom layout.

## 0.1.1 - 2026-06-15

- Documented `Tools/Deucarian/<PackageName>/...` as the package-owned tooling menu convention.
- Clarified that `Deucarian/Package Installer` is the bootstrap menu exception.
- Added `PackageToolMenuRoot` and updated UX menu path helpers to build `Tools/Deucarian/...` paths.

## 0.1.0 - 2026-06-15

- Added the initial editor-only Deucarian Editor package.
- Added fixed Deucarian editor colors, styles, icon fallback handling, chrome helpers, asset field helpers, status badges, and layout helpers.
- Documented Deucarian editor UX standards for menus, asset fields, and actions sections.
- Added editor tests covering constants, style accessors, icon fallbacks, status helpers, and asset field API availability.
