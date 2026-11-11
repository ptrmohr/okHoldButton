# okHoldButton — HTML / JavaScript

**Hold-to-confirm buttons for the web. No framework, no dependencies.**

Accidental clicks should not trigger irreversible actions. `okHoldButton` makes users **press, hold until the progress indicator is full, then release** before an action is confirmed.

A quick tap, an early release, moving the pointer off the button, losing focus, or switching browser tabs cancels the attempt. Nothing fires until the complete hold **and release**.

[**C# / Windows Forms version**](https://github.com/ptrmohr/okHoldButton/tree/main) · [**HTML / JavaScript version**](https://github.com/ptrmohr/okHoldButton/tree/html) · [Deutsche Anleitung](docs/README.de.md)

> This `html` branch contains the standalone web implementation. The original .NET / Windows Forms implementation on `main` is unchanged.

## Features

- Native **ES module** and scoped CSS; no build tools or runtime dependencies.
- Mouse, touch and **Space bar** interaction.
- Hold time configurable in milliseconds: 2,000 ms, 5,000 ms or any value from 250 to 60,000 ms.
- Linear progress indicator, release-to-confirm state and explicit success feedback.
- Cancellation on early release, pointer departure, lost focus or backgrounding the tab.
- Public properties, lifecycle methods, callbacks and standard **DOM CustomEvents**.
- MIT licensed by OnlineKraft UG.

## Live demo (local)

Open [`index.html`](index.html) using a local HTTP server. The demo includes a 2-second / 5-second selector and can be tried using a mouse, touch or keyboard.

```bash
# Run in the repository root (Python 3)
python -m http.server 8000
```

Then open **http://localhost:8000/**. ES modules may not load from `file://` URLs; use a local HTTP server or deploy the static files to your host.

## Quick start

Include the CSS, add a regular `<button>`, and instantiate the class:

```html
<link rel="stylesheet" href="./src/ok-hold-button.css">

<button id="confirm" type="button"></button>

<script type="module">
  import { OkHoldButton } from "./src/ok-hold-button.js";

  const hold = new OkHoldButton("#confirm", {
    holdDuration: 2000,             // 2 seconds (default: 5000)
    text: "DELETE ITEMS",
    subText: "Press and hold to confirm",
    releaseText: "RELEASE TO CONFIRM",
    successText: "CONFIRMED",
    accentColor: "#3e6377",
    fillColor: "#c4d7e4",
    onComplete: () => deleteSelectedItems(),
    onCancel: ({ reason }) => console.log("Cancelled:", reason)
  });

  // You can change public properties at runtime:
  hold.holdDuration = 5000;
  hold.text = "RELOAD DATA";
</script>
```

Replace `deleteSelectedItems()` with your own application logic. It is intentionally not part of this library.

## Load directly from a CDN

If you do not want to host the two small files yourself, **jsDelivr** can serve them from this public GitHub repository:

```html
<link rel="stylesheet"
      href="https://cdn.jsdelivr.net/gh/ptrmohr/okHoldButton@html/src/ok-hold-button.css">

<button id="confirm" type="button"></button>

<script type="module">
  import { OkHoldButton } from
    "https://cdn.jsdelivr.net/gh/ptrmohr/okHoldButton@html/src/ok-hold-button.js";

  new OkHoldButton("#confirm", {
    holdDuration: 5000,
    text: "CONFIRM",
    onComplete: () => console.log("Action confirmed!")
  });
</script>
```

**Production recommendation:** the `html` branch can change. Replace `@html` in **both** CDN URLs with a tested commit SHA or immutable release tag, and be aware that an external CDN introduces a third-party dependency. Hosting locally works just as well.

## Public API

### Options and writable properties

| Property | Default | Description |
| --- | --- | --- |
| `holdDuration` | `5000` | Hold time in milliseconds (250–60000) |
| `text` | `"PRESS AND HOLD"` | Main button label |
| `subText` | `"Hold to confirm"` | Secondary label |
| `releaseText` | `"RELEASE TO CONFIRM"` | Label when hold reaches 100% |
| `hintText` | `"Keep holding to confirm"` | Hint shown on a very short press |
| `successText` | `"CONFIRMED"` | Text after successful release |
| `fillColor` | `"#c4d7e4"` | Progress indicator |
| `accentColor` | `"#3e6377"` | Ready-state background |
| `textColor` | `"#2f4d5e"` | Normal text colour |
| `disabled` | `false` | Disables the button and cancels an active hold |
| `onComplete` | `null` | Callback invoked after successful release |
| `onCancel` | `null` | Callback invoked after cancellation |

### Read-only properties

- `progress`: progress as a number between `0` and `1`
- `state`: `"idle"`, `"holding"`, `"ready"` or `"success"`
- `completionCount`: number of successfully confirmed holds

### Methods

- `reset()`: return to the initial state
- `cancel(reason?)`: cancel the active hold
- `destroy()`: remove listeners and restore the original button markup

### DOM events

All events bubble and provide data in `event.detail`.

| Event | When |
| --- | --- |
| `holdstart` | Hold starts |
| `holdprogress` | Progress changes |
| `holdready` | Required duration is complete; waiting for release |
| `holdcompleted` | Button is released after the required duration |
| `holdcancelled` | Hold is cancelled |

```js
document.querySelector("#confirm").addEventListener("holdcompleted", (event) => {
  console.log("Confirmed after", event.detail.elapsedMs, "ms");
});
```

## Accessibility and safety notes

The HTML implementation uses a native `<button>`, which supports browser focus and semantics. It accepts the Space bar as a hold gesture. Enter is intentionally not a confirmation shortcut. Pointer interaction uses Pointer Events and disables long-press context menus on the control.

**Important:** this is a user-interface safeguard against accidental actions, **not an authorization or security mechanism**. Always validate destructive or privileged operations on the server.

## Project structure

```text
src/
  ok-hold-button.js    JavaScript ES module
  ok-hold-button.css   Component styles
index.html             Interactive English-language demo
docs/
  README.de.md         Optional German-language guide
LICENSE                MIT license
```

## License

MIT © 2026 OnlineKraft UG. See [LICENSE](LICENSE).
