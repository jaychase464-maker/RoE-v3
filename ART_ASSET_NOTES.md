# UI Artwork Notes

## Assets

| File | Role | Provenance |
|---|---|---|
| `TrooperStudiosSplash.png` | Full-screen studio splash | Supplied by the project owner on 2026-07-17 |
| `PhotosensitivityWarning.png` | Full-screen warning and legal acknowledgment | Supplied by the project owner on 2026-07-17 |
| `TacticalMenuBackground.png` | Title and menu background | Clean background plate created for Rules of Entry from the project owner's supplied menu composition on 2026-07-17; all baked UI, title, footer, icons, and readable markings were removed |

All three source files are 1672×941 PNG images and are imported by the setup tool as uncompressed single sprites without mipmaps. The scene uses an envelope aspect fitter so 16:9 is pixel-complete while other aspect ratios crop at the outer edges instead of distorting the artwork.

The tactical menu background intentionally contains no title, menu labels, footer, social icons, logos, flags, badges, department marks, or copied game UI. Its left side is reserved for live uGUI navigation while the skyline, lone officer, wet road, and patrol sedan carry the cinematic scene on the right.

Menu and refreshed HUD text use `LatinModernSansDemiCondensed.otf`, distributed under the GUST Font License. The full license and upstream attribution are included in `LatinModern-LICENSE.txt` beside the font.

Before commercial release, Trooper Studios should retain the original source records, approve final branding and legal language, and have the warning/legal copy reviewed for target territories and storefront requirements.
