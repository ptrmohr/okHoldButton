# okHoldButton HTML

**Dependency-free hold-to-confirm button for browsers.**

Press and hold until the progress reaches 100%. The action is confirmed **only when the user releases after the full hold**. A short click, early release, pointer leaving the button, losing window focus, or switching tabs cancels the action.

This is the separate **HTML/JavaScript implementation** in the [html branch](https://github.com/ptrmohr/okHoldButton/tree/html). The original [C# / Windows Forms implementation](https://github.com/ptrmohr/okHoldButton/tree/main) remains unchanged on main.

## Features

- Native JavaScript ES module and CSS, no dependencies or framework.
- Works with mouse, touch and Space bar; Enter does not confirm.
- Public properties for 2/5-second (or custom) duration, button text and colours.
- Complete-then-release confirmation; early release cancels.
- Standard DOM events for completion, cancellation and progress.
- MIT License, copyright 2026 OnlineKraft UG.

## Interactive demo

Serve this branch as static files using any HTTP server, e.g.:

~~~sh
python -m http.server 8000
~~~

Open http://localhost:8000/ and choose 2 or 5 seconds. ES modules may not load correctly when index.html is opened from a local file:// address.

## Integrate into your own HTML

~~~html
<link rel="stylesheet" href="./src/ok-hold-button.css">
<button id="confirm" type="button"></button>
<script type="module">
  import { OkHoldButton } from "./src/ok-hold-button.js";

  const hold = new OkHoldButton("#confirm", {
    holdDuration: 5000,
    text: "DATEN LÖSCHEN",
    subText: "Zum Bestätigen gedrückt halten",
    releaseText: "JETZT LOSLASSEN",
    fillColor: "#c4d7e4",
    accentColor: "#3e6377",
    onComplete: () => console.log("Confirmed"),
    onCancel: ({ reason }) => console.log("Cancelled", reason)
  });

  // Change public properties while the page is open:
  hold.holdDuration = 2000;
  hold.text = "NEU LADEN";
</script>
~~~

## Use directly from GitHub through a CDN

After publishing the branch, jsDelivr can serve both public source files without requiring you to store them on your own website:

~~~html
<link rel="stylesheet" href="https://cdn.jsdelivr.net/gh/ptrmohr/okHoldButton@html/src/ok-hold-button.css">
<button id="confirm" type="button"></button>
<script type="module">
  import { OkHoldButton } from
    "https://cdn.jsdelivr.net/gh/ptrmohr/okHoldButton@html/src/ok-hold-button.js";

  new OkHoldButton("#confirm", {
    holdDuration: 2000,
    text: "BESTÄTIGEN",
    onComplete: () => alert("Bestätigt!")
  });
</script>
~~~

**Important:** The html branch can change. Pin a tested commit SHA or release tag in production (replace @html with @COMMIT_SHA or @VERSION_TAG). Alternatively, host these two small files yourself. Always authorize sensitive server actions on the server as well.

## Public options and properties

| Property | Default | Description |
| --- | --- | --- |
| holdDuration | 5000 | Milliseconds, range 250-60000 |
| text | GEDRÜCKT HALTEN | Default main label |
| subText | Zum Bestätigen gedrückt halten | Default subtitle |
| releaseText | JETZT LOSLASSEN | Text after full hold |
| hintText | Bitte länger gedrückt halten | Short-press hint |
| successText | BESTÄTIGT | Confirmation text |
| fillColor | #c4d7e4 | Progress fill |
| accentColor | #3e6377 | Completed hold colour |
| textColor | #2f4d5e | Regular label colour |
| disabled | false | Disables input / cancels current hold |
| onComplete | null | Callback after successful release |
| onCancel | null | Callback after cancellation |
| progress | read-only | Number between 0 and 1 |
| state | read-only | idle / holding / ready / success |
| completionCount | read-only | Number of successful holds |

Methods: reset(), cancel(reason), destroy().

DOM events: holdstart, holdprogress, holdready, holdcompleted, holdcancelled. The events bubble and supply details through event.detail.

~~~javascript
document.querySelector("#confirm")
  .addEventListener("holdcompleted", (event) => {
    console.log("Confirmed in ms:", event.detail.elapsedMs);
  });
~~~

## Project files

- src/ok-hold-button.js — ES module and exported OkHoldButton class
- src/ok-hold-button.css — scoped styles
- index.html — interactive demonstration
- LICENSE — MIT license inherited from the original repository

## License

MIT © 2026 OnlineKraft UG. See LICENSE.
