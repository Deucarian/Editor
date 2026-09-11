# Lucide icon subset

This package vendors the package-owned Lucide 1.22.0 catalog used by Deucarian editor chrome.

- Source: https://github.com/lucide-icons/lucide/releases/tag/1.22.0
- License: ISC, with Feather-derived icons retaining their MIT terms as described in `LICENSE`.
- The files in `svg/` are the unmodified upstream source icons.
- The corresponding package icons are 128 px transparent PNG renders with white strokes so Unity can tint them and retain crisp edges at enlarged UI scales.
- Rebuild the PNGs with `node render-icons.cjs` using Sharp 0.35.4. SVG paths remain unchanged; only output size and stroke color are set during rendering. No generated raster is hand-edited.

No Lucide runtime or Unity Vector Graphics dependency is included.
