# Changelog

Newest version on top.

## Documentation – 2026-11-10
- New `CLAUDE.md` with working rules ("collect before building"). Commit 711eafa. No code changes (still 1.0.0).

## Documentation – 2026-11-09
- README: section "The idea: the hold is the confirmation" and a comparison table HoldButton vs. TileButton.
- Removed the comparison with drone apps (README, German description, code comment) – it did not describe them correctly.
- No code changes (still 1.0.0).

## 1.0.0 – 2026-11-08
- First public release, taken from okPriceTag (Beta 6).
- `HoldButton`: hold to confirm, fills while held, fires `HoldCompleted` on release; cancel on early release
  or sliding off; hint after a short tap; touch long-press fix; space bar support; `Muted` style still holdable.
- `TileButton`: large rounded touch-friendly button with icon, second text line and styles
  `Normal`, `Primary`, `Warning`, `Muted`.
- MIT license.
