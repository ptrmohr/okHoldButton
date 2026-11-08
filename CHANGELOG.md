# Changelog

Newest version on top.

## Documentation – 2026-11-09
- README: section "The idea: hold to take off, like a drone" and a comparison table HoldButton vs. TileButton.
- No code changes (still 1.0.0).

## 1.0.0 – 2026-11-08
- First public release, taken from okPriceTag (Beta 6).
- `HoldButton`: hold to confirm, fills while held, fires `HoldCompleted` on release; cancel on early release
  or sliding off; hint after a short tap; touch long-press fix; space bar support; `Muted` style still holdable.
- `TileButton`: large rounded touch-friendly button with icon, second text line and styles
  `Normal`, `Primary`, `Warning`, `Muted`.
- MIT license.
