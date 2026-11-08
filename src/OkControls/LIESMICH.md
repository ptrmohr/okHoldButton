# OkControls – HoldButton für Windows Forms

Ein Button, den man **gedrückt halten** muss, bevor er auslöst.
Gedacht für Aktionen, die nicht versehentlich passieren dürfen:
Daten neu laden, Verbindung testen, Löschen, Lizenz ändern.

Windows Forms bringt so etwas nicht mit (auch .NET 10 nicht). Diese Bibliothek
schließt die Lücke mit einem einzigen, selbst gezeichneten Steuerelement.

## Verhalten

1. Gedrückt halten → der Button füllt sich von links nach rechts (Standard: 5 Sekunden).
2. Bei 100 % wird er in der Akzentfarbe gefüllt und zeigt **„Jetzt loslassen“**.
3. **Erst beim Loslassen** wird `HoldCompleted` ausgelöst.
4. Früher loslassen oder mit Finger/Maus aus dem Button rutschen = **Abbruch**.
   Der Balken läuft sichtbar zurück, es passiert nichts.
5. Kurzes Antippen zeigt für 2 Sekunden den Hinweis **„Gedrückt halten“** – so lernt
   jeder die Bedienung, ohne dass ein Erklärtext dauerhaft auf dem Button steht.

## Was ihn besonders macht

- **Touch-tauglich:** Windows deutet langes Drücken auf Touchscreens normalerweise als
  Rechtsklick. Der HoldButton schaltet das für sich ab
  (`MicrosoftTabletPenServiceProperty`), damit Tablets und Kassen-Touchscreens funktionieren.
- **Kein versehentliches Auslösen:** Ein normaler Klick, Enter oder ein Doppelklick lösen
  nichts aus – nur das vollständige Halten.
- **Tastatur:** Leertaste halten funktioniert genauso.
- **Zustand „ausgegraut, aber bedienbar“:** Mit `TileStyle = Muted` sieht der Button
  deaktiviert aus, lässt sich aber trotzdem halten. Beispiel: „Neu laden“ ist grau, weil heute
  schon geladen wurde – wer es wirklich will, hält 5 Sekunden.
- **Designer-tauglich:** Alle Eigenschaften erscheinen im Visual-Studio-Designer;
  das Standardereignis ist `HoldCompleted`.
- **DPI-fest:** Abstände und Rundungen skalieren mit der Bildschirmauflösung.

## Verwendung

```csharp
var btnReload = new OkControls.HoldButton
{
    Text = "Neu laden",
    SubText = "Stand: 07.11.2026",   // optionale zweite Zeile
    HoldDuration = 5000,             // Millisekunden
    TileStyle = OkControls.TileStyle.Muted,
    Size = new Size(240, 56)
};
btnReload.HoldCompleted += (s, e) => ReloadData();
```

## Eigenschaften (Auswahl)

| Eigenschaft      | Standard            | Bedeutung                                         |
|------------------|---------------------|---------------------------------------------------|
| `HoldDuration`   | 5000                | Haltezeit in ms bis „voll“                        |
| `ReleaseText`    | „Jetzt loslassen“   | Text bei vollem Button                            |
| `HintText`       | „Gedrückt halten“   | Hinweis nach kurzem Antippen (leer = aus)         |
| `HoldFillColor`  | #C4D7E4             | Füllfarbe während des Haltens                     |
| `AccentColor`    | #3E6377             | Füllfarbe bei 100 %, Primary-Stil, Fokusrahmen    |
| `HoldTextColor`  | #2F4D5E             | Schriftfarbe während des Haltens                  |
| `SubText`, `Icon`, `TileStyle`, `CornerRadius` | | vom `TileButton` geerbt              |
| `Progress`       | 0                   | aktueller Füllstand 0..1 (nur Code)               |

## TileButton

`HoldButton` erbt von `TileButton`, einem großen, abgerundeten Button mit Icon und
optionaler zweiter Textzeile. Stile: `Normal` (weiß), `Primary` (Akzentfarbe),
`Warning` (orange), `Muted` (grau, aber bedienbar). Verhält sich wie ein normaler Button
(`Click`, `DialogResult`, Enter/Leertaste).

## Technik

- .NET 10, Windows Forms, keine weiteren Abhängigkeiten
- Dateien: `HoldButton.cs`, `TileButton.cs`, `DrawHelper.cs`, `Palette.cs`
- Lizenz: MIT (siehe LICENSE im Hauptordner)

Entstanden für **okPriceTag** (OnlineKraft), eine Etikettendruck-App für Kioske.
