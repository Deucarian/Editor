# Third-party notices

This notice describes the dependency and distribution inventory for `com.deucarian.editor` `1.0.0`. It does not replace the repository's [MIT license](LICENSE.md), and it does not grant rights to software supplied separately.

## Review basis

The reviewed baseline is `origin/main` commit `635064c4ce3e73d794435faa5c44487a35941fbe`. Its `npm pack --dry-run` inventory contained 102 package files. The tracked and packed inventories were checked for common vendor/third-party directories, compiled binaries and archives, Git submodules, Git LFS pointers, separate license markers, and media/font assets.

That inventory identified no files marked or located as vendored third-party source, no compiled binary/archive candidates, no submodules, and no LFS pointers.

## Direct package dependencies

The reviewed `package.json` declares no direct package dependencies.

## Included visual assets

The package distribution includes these PNG assets:

| File | SHA-256 | Repository-history evidence |
|---|---|---|
| `Editor/Assets/Icons/DeucarianPackagePlaceholderIcon.png` | `496927481ff3d31f9b317ea98f2260baf14861d2d921d297803fae1ef4f9963c` | Added in `850e35996cb8c51a67773929914419b783ca7f69` by the repository owner |
| `Editor/Assets/Images/DeucarianInstallerBackground.png` | `3939dcd950cf688438e84b5bd460d3d4c75cf4309024d61bf968e73d1e4956bd` | Added in `50b09d249f804940a8991fa2daaaba0f47612597` by the repository owner |
| `Editor/Assets/Images/DeucarianPackageInstallerPlaceholderHero.png` | `754ba3487973604cc22156e7051ce4fb85905a2fffb2c5c09181df91e27d91db` | Added in `850e35996cb8c51a67773929914419b783ca7f69` by the repository owner |
| `Editor/Assets/Logos/DeucarianPlaceholderLogo.png` | `aa5df75de81ff70c4fdf69eb94b8d9c316ab8786a1d3483ce72a19a968c0f43f` | Added in `850e35996cb8c51a67773929914419b783ca7f69` by the repository owner |

No separate third-party license or attribution marker accompanies these assets, and their Git history records only the repository owner's authorship. On that evidence they are classified as Deucarian package content, not third-party components. This classification should be revisited when final brand assets replace the placeholders.

## Host platform

The manifest requires Unity `2021.3`. Unity is not included in this package and is governed by the applicable [Unity Editor Software Terms](https://unity.com/legal/editor-terms-of-service/software).

Re-run the inventory and update this notice whenever dependencies or distributed content change.
