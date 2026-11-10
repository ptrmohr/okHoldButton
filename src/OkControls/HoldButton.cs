using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace OkControls;

/// <summary>
/// Hold button: press and hold, the button fills from left to right.
/// Only when it is completely full is <see cref="HoldCompleted"/> raised on release.
/// Releasing earlier, or sliding out of the button with the finger/mouse, cancels –
/// nothing happens. Protects dangerous actions (reload, reset) against accidental taps.
/// A short tap briefly shows the hint <see cref="HintText"/>.
/// Works with mouse, touch (Windows "press and hold = right click" is switched off)
/// and keyboard (hold the space bar).
/// </summary>
[DefaultEvent(nameof(HoldCompleted))]
public class HoldButton : TileButton
{
    // animation step and how long the hint stays visible (ms)
    private const int TickMs = 30;
    private const int HintMs = 2000;

    // animation timer, hint timer and a stopwatch measuring the hold time
    private readonly System.Windows.Forms.Timer _timer = new() { Interval = TickMs };
    private readonly System.Windows.Forms.Timer _hintTimer = new() { Interval = HintMs };
    private readonly Stopwatch _watch = new();

    private int _holdDuration = 5000;
    private float _progress;      // 0..1
    private bool _holding;
    private bool _armed;          // full – releasing triggers
    private bool _rewinding;      // after a cancel, let the bar run back
    // hint visible, texts and colours while holding
    private bool _showHint;
    private string _releaseText = "Jetzt loslassen";
    private string _hintText = "Gedrückt halten";
    private Color _holdFill = Palette.HoldFill;
    private Color _holdText = Palette.HoldText;

    /// <summary>Wires the animation and hint timers.</summary>
    public HoldButton()
    {
        _timer.Tick += (_, _) => OnTimerTick();
        _hintTimer.Tick += (_, _) => { _hintTimer.Stop(); _showHint = false; Invalidate(); };
    }

    /// <summary>Raised when the button was full and then released.</summary>
    [Category("Action")]
    [Description("Button wurde lange genug gehalten und dann losgelassen.")]
    public event EventHandler? HoldCompleted;

    /// <summary>Hold time in milliseconds until the button is full (at least 200).</summary>
    [Category("Behavior")]
    [DefaultValue(5000)]
    [Description("Haltedauer in Millisekunden, bis der Button voll ist.")]
    public int HoldDuration
    {
        get => _holdDuration;
        set => _holdDuration = Math.Max(200, value);
    }

    /// <summary>Text shown as soon as the button is full.</summary>
    [Category("Appearance")]
    [DefaultValue("Jetzt loslassen")]
    [Description("Text, der erscheint, sobald der Button voll ist.")]
    public string ReleaseText
    {
        get => _releaseText;
        set { _releaseText = value ?? string.Empty; Invalidate(); }
    }

    /// <summary>Short hint after a simple tap.</summary>
    [Category("Appearance")]
    [DefaultValue("Gedrückt halten")]
    [Description("Kurzer Hinweis nach einem einfachen Antippen.")]
    public string HintText
    {
        get => _hintText;
        set => _hintText = value ?? string.Empty;
    }

    /// <summary>Colour of the fill bar while holding (when full: AccentColor).</summary>
    [Category("Appearance")]
    [Description("Farbe des Füllbalkens, solange gehalten wird (voll: AccentColor).")]
    public Color HoldFillColor
    {
        get => _holdFill;
        set { _holdFill = value; Invalidate(); }
    }

    /// <summary>Designer support for HoldFillColor (serialise only if changed, Reset).</summary>
    private bool ShouldSerializeHoldFillColor() => _holdFill != Palette.HoldFill;

    private void ResetHoldFillColor() => HoldFillColor = Palette.HoldFill;

    /// <summary>Text colour while holding.</summary>
    [Category("Appearance")]
    [Description("Schriftfarbe, solange gehalten wird.")]
    public Color HoldTextColor
    {
        get => _holdText;
        set { _holdText = value; Invalidate(); }
    }

    /// <summary>Designer support for HoldTextColor (serialise only if changed, Reset).</summary>
    private bool ShouldSerializeHoldTextColor() => _holdText != Palette.HoldText;

    private void ResetHoldTextColor() => HoldTextColor = Palette.HoldText;

    /// <summary>Current fill level 0..1 (e.g. for tests or snapshots).</summary>
    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public float Progress
    {
        get => _progress;
        set { _progress = Math.Clamp(value, 0f, 1f); Invalidate(); }
    }

    /// <summary>Switches off the Windows touch "press and hold" gesture for this window handle.</summary>
    protected override void OnHandleCreated(EventArgs e)
    {
        base.OnHandleCreated(e);
        // Windows touch: "press and hold" would otherwise be interpreted as a right click.
        const int TabletDisablePressAndHold = 0x00000001;
        SetProp(Handle, "MicrosoftTabletPenServiceProperty", TabletDisablePressAndHold);
    }

    /// <summary>Left button down starts holding.</summary>
    protected override void OnMouseDown(MouseEventArgs e)
    {
        base.OnMouseDown(e);
        if (e.Button == MouseButtons.Left && Enabled)
        {
            StartHold();
        }
    }

    /// <summary>Leaving the button area while holding cancels.</summary>
    protected override void OnMouseMove(MouseEventArgs e)
    {
        base.OnMouseMove(e);
        if (_holding && !ClientRectangle.Contains(e.Location))
        {
            CancelHold(); // slid out of the button
        }
    }

    /// <summary>Left button up ends holding (triggers only if full).</summary>
    protected override void OnMouseUp(MouseEventArgs e)
    {
        base.OnMouseUp(e);
        if (e.Button == MouseButtons.Left)
        {
            EndHold();
        }
    }

    /// <summary>Space bar down starts holding.</summary>
    protected override void OnKeyDown(KeyEventArgs e)
    {
        if (e.KeyCode == Keys.Space && !_holding && Enabled)
        {
            StartHold();
            e.Handled = true;
        }
        base.OnKeyDown(e);
    }

    /// <summary>Space bar up ends holding.</summary>
    protected override void OnKeyUp(KeyEventArgs e)
    {
        // deliberately NOT base.OnKeyUp: a key press must not act like a click
        if (e.KeyCode == Keys.Space)
        {
            EndHold();
            e.Handled = true;
        }
    }

    /// <summary>Swallows normal clicks.</summary>
    protected override void OnClick(EventArgs e)
    {
        // a normal click triggers nothing. The only trigger is HoldCompleted.
    }

    /// <summary>Losing the focus cancels.</summary>
    protected override void OnLostFocus(EventArgs e)
    {
        CancelHold();
        base.OnLostFocus(e);
    }

    /// <summary>Being disabled cancels.</summary>
    protected override void OnEnabledChanged(EventArgs e)
    {
        if (!Enabled)
        {
            CancelHold();
        }
        base.OnEnabledChanged(e);
    }

    /// <summary>Starts a new hold: resets the state and starts the stopwatch and animation.</summary>
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

    /// <summary>
    /// Release: if the button was full, raise HoldCompleted. Otherwise let the bar run back, and after a very
    /// short tap (under 400 ms) show the hint text.
    /// </summary>
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

        _rewinding = true; // cancelled: the bar runs back quickly
        Invalidate();
    }

    /// <summary>Cancels a running hold without triggering; the bar runs back.</summary>
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

    /// <summary>Animation step: while holding, fill according to the elapsed time; after a cancel, run back in about 0.3 s.</summary>
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
            _progress -= TickMs / 300f; // back in about 0.3 s
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

    /// <summary>Draws the fill bar (hold colour, accent colour when full) clipped to the rounded button shape.</summary>
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

    /// <summary>Text: "release now" text when full, otherwise the normal text with the hint or the sub text.</summary>
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

    /// <summary>Disposes both timers.</summary>
    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _timer.Dispose();
            _hintTimer.Dispose();
        }
        base.Dispose(disposing);
    }

    // Windows API: sets a window property (used to disable press-and-hold)
    [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern bool SetProp(IntPtr hWnd, string lpString, nint hData);
}
