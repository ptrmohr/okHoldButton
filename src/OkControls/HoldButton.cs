using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace OkControls;

/// <summary>
/// Halte-Button (wie bei DJI-Drohnen): gedrückt halten, der Button füllt sich von links
/// nach rechts. Erst wenn er ganz voll ist, wird beim Loslassen <see cref="HoldCompleted"/>
/// ausgelöst. Wer vorher loslässt oder mit dem Finger/der Maus aus dem Button rutscht,
/// bricht ab – es passiert nichts.
/// Ein kurzes Antippen zeigt kurz den Hinweis <see cref="HintText"/>.
/// Funktioniert mit Maus, Touch (Windows-„Gedrückt halten = Rechtsklick“ ist abgeschaltet)
/// und Tastatur (Leertaste halten).
/// </summary>
[DefaultEvent(nameof(HoldCompleted))]
public class HoldButton : TileButton
{
    private const int TickMs = 30;
    private const int HintMs = 2000;

    private readonly System.Windows.Forms.Timer _timer = new() { Interval = TickMs };
    private readonly System.Windows.Forms.Timer _hintTimer = new() { Interval = HintMs };
    private readonly Stopwatch _watch = new();

    private int _holdDuration = 5000;
    private float _progress;      // 0..1
    private bool _holding;
    private bool _armed;          // voll – Loslassen löst aus
    private bool _rewinding;      // nach Abbruch zurücklaufen lassen
    private bool _showHint;
    private string _releaseText = "Jetzt loslassen";
    private string _hintText = "Gedrückt halten";
    private Color _holdFill = Palette.HoldFill;
    private Color _holdText = Palette.HoldText;

    public HoldButton()
    {
        _timer.Tick += (_, _) => OnTimerTick();
        _hintTimer.Tick += (_, _) => { _hintTimer.Stop(); _showHint = false; Invalidate(); };
    }

    /// <summary>Wird ausgelöst, wenn der Button voll war und dann losgelassen wurde.</summary>
    [Category("Action")]
    [Description("Button wurde lange genug gehalten und dann losgelassen.")]
    public event EventHandler? HoldCompleted;

    [Category("Behavior")]
    [DefaultValue(5000)]
    [Description("Haltedauer in Millisekunden, bis der Button voll ist.")]
    public int HoldDuration
    {
        get => _holdDuration;
        set => _holdDuration = Math.Max(200, value);
    }

    [Category("Appearance")]
    [DefaultValue("Jetzt loslassen")]
    [Description("Text, der erscheint, sobald der Button voll ist.")]
    public string ReleaseText
    {
        get => _releaseText;
        set { _releaseText = value ?? string.Empty; Invalidate(); }
    }

    [Category("Appearance")]
    [DefaultValue("Gedrückt halten")]
    [Description("Kurzer Hinweis nach einem einfachen Antippen.")]
    public string HintText
    {
        get => _hintText;
        set => _hintText = value ?? string.Empty;
    }

    [Category("Appearance")]
    [Description("Farbe des Füllbalkens, solange gehalten wird (voll: AccentColor).")]
    public Color HoldFillColor
    {
        get => _holdFill;
        set { _holdFill = value; Invalidate(); }
    }

    private bool ShouldSerializeHoldFillColor() => _holdFill != Palette.HoldFill;

    private void ResetHoldFillColor() => HoldFillColor = Palette.HoldFill;

    [Category("Appearance")]
    [Description("Schriftfarbe, solange gehalten wird.")]
    public Color HoldTextColor
    {
        get => _holdText;
        set { _holdText = value; Invalidate(); }
    }

    private bool ShouldSerializeHoldTextColor() => _holdText != Palette.HoldText;

    private void ResetHoldTextColor() => HoldTextColor = Palette.HoldText;

    /// <summary>Aktueller Füllstand 0..1 (z. B. für Tests oder Snapshots).</summary>
    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public float Progress
    {
        get => _progress;
        set { _progress = Math.Clamp(value, 0f, 1f); Invalidate(); }
    }

    protected override void OnHandleCreated(EventArgs e)
    {
        base.OnHandleCreated(e);
        // Windows-Touch: „Gedrückt halten“ würde sonst als Rechtsklick gedeutet.
        const int TabletDisablePressAndHold = 0x00000001;
        SetProp(Handle, "MicrosoftTabletPenServiceProperty", TabletDisablePressAndHold);
    }

    protected override void OnMouseDown(MouseEventArgs e)
    {
        base.OnMouseDown(e);
        if (e.Button == MouseButtons.Left && Enabled)
        {
            StartHold();
        }
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        base.OnMouseMove(e);
        if (_holding && !ClientRectangle.Contains(e.Location))
        {
            CancelHold(); // aus dem Button gerutscht
        }
    }

    protected override void OnMouseUp(MouseEventArgs e)
    {
        base.OnMouseUp(e);
        if (e.Button == MouseButtons.Left)
        {
            EndHold();
        }
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        if (e.KeyCode == Keys.Space && !_holding && Enabled)
        {
            StartHold();
            e.Handled = true;
        }
        base.OnKeyDown(e);
    }

    protected override void OnKeyUp(KeyEventArgs e)
    {
        // bewusst NICHT base.OnKeyUp: ein Tastendruck darf nicht wie ein Klick wirken
        if (e.KeyCode == Keys.Space)
        {
            EndHold();
            e.Handled = true;
        }
    }

    protected override void OnClick(EventArgs e)
    {
        // Ein normaler Klick löst nichts aus. Auslöser ist ausschließlich HoldCompleted.
    }

    protected override void OnLostFocus(EventArgs e)
    {
        CancelHold();
        base.OnLostFocus(e);
    }

    protected override void OnEnabledChanged(EventArgs e)
    {
        if (!Enabled)
        {
            CancelHold();
        }
        base.OnEnabledChanged(e);
    }

    private void StartHold()
    {
        _holding = true;
        _armed = false;
        _rewinding = false;
        _showHint = false;
        _hintTimer.Stop();
        _watch.Restart();
        _timer.Start();
        Invalidate();
    }

    private void EndHold()
    {
        if (!_holding)
        {
            return;
        }

        bool fire = _armed;
        long heldMs = _watch.ElapsedMilliseconds;
        _holding = false;
        _armed = false;
        _watch.Stop();

        if (fire)
        {
            _timer.Stop();
            _progress = 0f;
            Invalidate();
            HoldCompleted?.Invoke(this, EventArgs.Empty);
            return;
        }

        if (heldMs < 400 && _hintText.Length > 0)
        {
            _showHint = true;
            _hintTimer.Start();
        }

        _rewinding = true; // Abbruch: Balken läuft zügig zurück
        Invalidate();
    }

    private void CancelHold()
    {
        if (!_holding)
        {
            return;
        }

        _holding = false;
        _armed = false;
        _watch.Stop();
        _rewinding = true;
        Invalidate();
    }

    private void OnTimerTick()
    {
        if (_holding)
        {
            _progress = Math.Min(1f, _watch.ElapsedMilliseconds / (float)_holdDuration);
            if (_progress >= 1f && !_armed)
            {
                _armed = true;
            }
        }
        else if (_rewinding)
        {
            _progress -= TickMs / 300f; // in ca. 0,3 s zurück
            if (_progress <= 0f)
            {
                _progress = 0f;
                _rewinding = false;
                _timer.Stop();
            }
        }
        else
        {
            _timer.Stop();
        }

        Invalidate();
    }

    protected override void PaintOverlay(Graphics g, RectangleF rect, float radius)
    {
        if (_progress <= 0f)
        {
            return;
        }

        Color fill = _armed ? AccentColor : _holdFill;
        using var path = DrawHelper.RoundedRect(rect, radius);
        Region old = g.Clip;
        g.SetClip(path);
        using (var brush = new SolidBrush(fill))
        {
            g.FillRectangle(brush, 0, 0, rect.Width * _progress, rect.Height);
        }
        g.Clip = old;
    }

    protected override void PaintContent(Graphics g, Color textColor)
    {
        if (_armed)
        {
            PaintContent(g, Color.White, _releaseText, string.Empty);
            return;
        }

        Color color = _holding ? _holdText : textColor;
        PaintContent(g, color, Text, _showHint ? _hintText : SubText);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _timer.Dispose();
            _hintTimer.Dispose();
        }
        base.Dispose(disposing);
    }

    [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern bool SetProp(IntPtr hWnd, string lpString, nint hData);
}
