# okHoldButton – HTML / JavaScript

**Ein frei nutzbarer Halte-Button für Webseiten.** Ohne Framework und ohne zusätzliche Abhängigkeiten.

Der Benutzer muss die Taste vollständig gedrückt halten **und danach loslassen**. Erst dann wird das Ereignis `holdcompleted` ausgelöst. Ein kurzer Klick, vorzeitiges Loslassen, das Verlassen der Schaltfläche oder der Wechsel in einen anderen Browsertab führen zum Abbruch.

[English README](../README.md) · [HTML-Zweig](https://github.com/ptrmohr/okHoldButton/tree/html) · [C# / WinForms](https://github.com/ptrmohr/okHoldButton/tree/main)

## Schnellstart

```html
<link rel="stylesheet" href="./src/ok-hold-button.css">

<button id="confirm" type="button"></button>

<script type="module">
  import { OkHoldButton } from "./src/ok-hold-button.js";

  const button = new OkHoldButton("#confirm", {
    holdDuration: 2000,
    text: "DATEN LÖSCHEN",
    subText: "Zum Bestätigen gedrückt halten",
    releaseText: "JETZT LOSLASSEN",
    successText: "BESTÄTIGT",
    onComplete: () => console.log("Aktion bestätigt!"),
    onCancel: ({ reason }) => console.log("Abgebrochen:", reason)
  });

  // Eigenschaften lassen sich zur Laufzeit verändern:
  button.holdDuration = 5000;
  button.text = "NEU LADEN";
</script>
```

## Öffentliche Einstellungen

| Eigenschaft | Standard | Bedeutung |
| --- | --- | --- |
| `holdDuration` | 5000 | Haltedauer in Millisekunden (250–60000) |
| `text` | PRESS AND HOLD | Hauptbeschriftung (Englisch ist der Standard) |
| `subText` | Hold to confirm | Zweite Textzeile |
| `releaseText` | RELEASE TO CONFIRM | Aufforderung nach vollständigem Halten |
| `hintText` | Keep holding to confirm | Hinweis bei kurzem Antippen |
| `successText` | CONFIRMED | Text nach erfolgreicher Bestätigung |
| `fillColor` | #c4d7e4 | Farbe des Fortschrittsbalkens |
| `accentColor` | #3e6377 | Farbe bei abgeschlossener Haltedauer |
| `textColor` | #2f4d5e | Textfarbe |
| `disabled` | false | Deaktivierung |
| `onComplete` | null | Callback nach erfolgreichem Loslassen |
| `onCancel` | null | Callback beim Abbruch |

Zusätzlich gibt es die schreibgeschützten Eigenschaften `progress`, `state` und `completionCount`; die Methoden `reset()`, `cancel()`, `destroy()`; und die Ereignisse `holdstart`, `holdprogress`, `holdready`, `holdcompleted` und `holdcancelled`.

## Demo und CDN

Die englische Demo steht in [`index.html`](../index.html). Du kannst sie mit einem einfachen lokalen HTTP-Server starten:

```bash
python -m http.server 8000
```

Für eine Einbindung ohne eigene Dateikopie kannst du die öffentlichen Dateien über jsDelivr beziehen. Ein Beispiel mit den URLs findest du im [englischen README](../README.md#load-directly-from-a-cdn). Für produktive Seiten sollte ein geprüfter Commit oder Release-Tag statt des veränderlichen `html`-Zweigs verwendet werden.

## Hinweis

Die Komponente verhindert versehentliche Bedienung, ist aber **kein Sicherheits- oder Berechtigungssystem**. Kritische Aktionen müssen zusätzlich serverseitig abgesichert werden.

## Lizenz

MIT © 2026 OnlineKraft UG. Siehe [LICENSE](../LICENSE).
