using System.ComponentModel;
using System.Drawing.Drawing2D;

namespace OkControls;

/// <summary>Look of a <see cref="TileButton"/>.</summary>
public enum TileStyle
{
    /// <summary>White with a grey border.</summary>
    Normal,
    /// <summary>Filled blue-grey, white text (main action).</summary>
    Primary,
    /// <summary>Filled orange (e.g. printer not reachable).</summary>
    Warning,
    /// <summary>Looks greyed out but stays usable (for hold buttons).</summary>
    Muted
}

/// <summary>
/// Large, tablet-friendly button as in the mockups: rounded corners, icon left
/// of the text, optionally a second smaller text line (<see cref="SubText"/>).
/// Completely owner-drawn so it looks the same on all Windows versions. Implements IButtonControl, so it
/// can be a form's AcceptButton/CancelButton and set a DialogResult.
/// Base class of <see cref="HoldButton"/>.
/// </summary>
[DefaultEvent(nameof(Click))]
public class TileButton : Control, IButtonControl
{
    // appearance values and the hover/pressed state
    private string _subText = string.Empty;
    private Image? _icon;
    private TileStyle _style = TileStyle.Normal;
    private int _radius = 6;
    private bool _hover;
    private bool _pressed;
    private Color _accent = Palette.Accent;

    /// <summary>Enables owner drawing, focus and standard click handling; transparent background, hand cursor.</summary>
    public TileButton()
    {
        SetStyle(ControlStyles.UserPaint
                 | ControlStyles.AllPaintingInWmPaint
                 | ControlStyles.OptimizedDoubleBuffer
                 | ControlStyles.ResizeRedraw
                 | ControlStyles.Selectable
                 | ControlStyles.StandardClick
                 | ControlStyles.SupportsTransparentBackColor, true);
        BackColor = Color.Transparent;
        ForeColor = Palette.Text;
        Cursor = Cursors.Hand;
        Size = new Size(160, 52);
        AccessibleRole = AccessibleRole.PushButton;
    }

    /// <summary>Second, smaller text line under the text (e.g. "Stand: 07.11.2026").</summary>
    [Category("Appearance")]
    [DefaultValue("")]
    [Description("Zweite, kleinere Textzeile unter dem Text (z. B. \"Stand: 07.11.2026\").")]
    public string SubText
    {
        get => _subText;
        set { _subText = value ?? string.Empty; Invalidate(); }
    }

    /// <summary>Icon left of the text (hidden when disabled).</summary>
    [Category("Appearance")]
    [DefaultValue(null)]
    [Description("Icon links vom Text.")]
    public Image? Icon
    {
        get => _icon;
        set { _icon = value; Invalidate(); }
    }

    /// <summary>Colour style of the button.</summary>
    [Category("Appearance")]
    [DefaultValue(TileStyle.Normal)]
    public TileStyle TileStyle
    {
        get => _style;
        set { _style = value; Invalidate(); }
    }

    /// <summary>Corner radius in logical pixels.</summary>
    [Category("Appearance")]
    [DefaultValue(6)]
    public int CornerRadius
    {
        get => _radius;
        set { _radius = Math.Max(0, value); Invalidate(); }
    }

    /// <summary>Accent colour: fill for Primary, focus border, hold fill when full.</summary>
    [Category("Appearance")]
    [Description("Akzentfarbe: Füllung bei Primary, Fokusrahmen, Haltefüllung im vollen Zustand.")]
    public Color AccentColor
    {
        get => _accent;
        set { _accent = value; Invalidate(); }
    }

    /// <summary>Designer support for AccentColor (serialise only if changed, Reset).</summary>
    private bool ShouldSerializeAccentColor() => _accent != Palette.Accent;

    private void ResetAccentColor() => AccentColor = Palette.Accent;

    /// <summary>IButtonControl: result set on the form when clicked (None = nothing).</summary>
    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public DialogResult DialogResult { get; set; } = DialogResult.None;

    /// <summary>True while the left mouse button is down on the button.</summary>
    protected bool IsPressed => _pressed;

    /// <summary>IButtonControl: not used (the default button is not drawn differently).</summary>
    public void NotifyDefault(bool value)
    {
    }

    /// <summary>IButtonControl: raises Click (e.g. via AcceptButton or the space bar).</summary>
    public void PerformClick()
    {
        if (CanSelect)
        {
            OnClick(EventArgs.Empty);
        }
    }

    /// <summary>Repaints when the text changes.</summary>
    protected override void OnTextChanged(EventArgs e)
    {
        base.OnTextChanged(e);
        Invalidate();
    }

    /// <summary>Hover highlight on.</summary>
    protected override void OnMouseEnter(EventArgs e)
    {
        _hover = true;
        Invalidate();
        base.OnMouseEnter(e);
    }

    /// <summary>Hover and pressed highlight off.</summary>
    protected override void OnMouseLeave(EventArgs e)
    {
        _hover = false;
        _pressed = false;
        Invalidate();
        base.OnMouseLeave(e);
    }

    /// <summary>Pressed look and focus on left button down.</summary>
    protected override void OnMouseDown(MouseEventArgs e)
    {
        if (e.Button == MouseButtons.Left)
        {
            _pressed = true;
            Focus();
            Invalidate();
        }
        base.OnMouseDown(e);
    }

    /// <summary>Pressed look off.</summary>
    protected override void OnMouseUp(MouseEventArgs e)
    {
        _pressed = false;
        Invalidate();
        base.OnMouseUp(e);
    }

    /// <summary>Raises Click and sets the form's DialogResult if one is assigned.</summary>
    protected override void OnClick(EventArgs e)
    {
        base.OnClick(e);
        if (DialogResult != DialogResult.None && FindForm() is Form form)
        {
            form.DialogResult = DialogResult;
        }
    }

    /// <summary>Keyboard: releasing Space or Enter triggers the button like a click.</summary>
    protected override void OnKeyUp(KeyEventArgs e)
    {
        if (e.KeyCode is Keys.Space or Keys.Enter)
        {
            PerformClick();
            e.Handled = true;
        }
        base.OnKeyUp(e);
    }

    /// <summary>Repaints in the enabled/disabled look.</summary>
    protected override void OnEnabledChanged(EventArgs e)
    {
        base.OnEnabledChanged(e);
        Invalidate();
    }

    /// <summary>Repaints with the focus border.</summary>
    protected override void OnGotFocus(EventArgs e)
    {
        base.OnGotFocus(e);
        Invalidate();
    }

    /// <summary>Repaints without the focus border.</summary>
    protected override void OnLostFocus(EventArgs e)
    {
        base.OnLostFocus(e);
        Invalidate();
    }

    /// <summary>Background colour depending on style and state (disabled, pressed, hover).</summary>
    protected virtual Color GetBackColor()
    {
        if (!Enabled)
        {
            return Palette.DisabledBack;
        }

        Color baseColor = _style switch
        {
            TileStyle.Primary => AccentColor,
            TileStyle.Warning => Palette.Warning,
            TileStyle.Muted => Palette.DisabledBack,
            _ => Color.White
        };

        if (_pressed)
        {
            return _style == TileStyle.Normal || _style == TileStyle.Muted ? Palette.Selection : ControlPaint.Dark(baseColor, 0.05f);
        }

        if (_hover)
        {
            return _style == TileStyle.Normal ? Color.FromArgb(246, 249, 251) : ControlPaint.Light(baseColor, 0.08f);
        }

        return baseColor;
    }

    /// <summary>Text colour: white on filled styles, grey when disabled or muted, otherwise ForeColor.</summary>
    protected virtual Color GetTextColor()
    {
        if (!Enabled || _style == TileStyle.Muted)
        {
            return Palette.DisabledText;
        }

        return _style is TileStyle.Primary or TileStyle.Warning ? Color.White : ForeColor;
    }

    /// <summary>Border colour depending on the style.</summary>
    protected Color GetBorderColor() => _style switch
    {
        TileStyle.Primary => AccentColor,
        TileStyle.Warning => Palette.Warning,
        TileStyle.Muted => Color.FromArgb(213, 220, 222),
        _ => Palette.Border
    };

    /// <summary>Paint order: background, overlay (for derived classes), border (accent while focused), content.</summary>
    protected override void OnPaint(PaintEventArgs e)
    {
        Graphics g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;
        var rect = new RectangleF(0, 0, Width, Height);
        float radius = LogicalToDeviceUnits(_radius);

        DrawHelper.FillRounded(g, GetBackColor(), rect, radius);
        PaintOverlay(g, rect, radius);
        DrawHelper.BorderRounded(g, Focused && ShowFocusCues ? AccentColor : GetBorderColor(), rect, radius,
            Focused && ShowFocusCues ? 2f : 1f);

        PaintContent(g, GetTextColor());
    }

    /// <summary>For derived classes: draw between background and content (e.g. progress).</summary>
    protected virtual void PaintOverlay(Graphics g, RectangleF rect, float radius)
    {
    }

    /// <summary>Draws icon and text(s) centred.</summary>
    protected virtual void PaintContent(Graphics g, Color textColor)
    {
        PaintContent(g, textColor, Text, _subText);
    }

    /// <summary>
    /// Draws icon, title and optional sub text as one centred block; text that does not fit is shortened with
    /// "…". Disabled buttons show no icon, muted buttons a greyed icon.
    /// </summary>
    protected void PaintContent(Graphics g, Color textColor, string title, string sub)
    {
        int gap = LogicalToDeviceUnits(10);
        using var subFont = new Font(Font.FontFamily, Font.Size * 0.78f, FontStyle.Regular);
        TextFormatFlags flags = TextFormatFlags.NoPadding | TextFormatFlags.SingleLine;

        Size titleSize = TextRenderer.MeasureText(g, title, Font, Size.Empty, flags);
        Size subSize = sub.Length > 0 ? TextRenderer.MeasureText(g, sub, subFont, Size.Empty, flags) : Size.Empty;
        int textWidth = Math.Max(titleSize.Width, subSize.Width);
        int textHeight = titleSize.Height + (sub.Length > 0 ? subSize.Height : 0);

        Image? icon = _icon;
        if (icon != null && !Enabled)
        {
            icon = null; // greyed-out buttons without a coloured icon
        }

        int iconWidth = icon?.Width ?? 0;
        int contentWidth = textWidth + (icon != null && title.Length > 0 ? iconWidth + gap : iconWidth);
        int maxWidth = Width - LogicalToDeviceUnits(16);
        if (contentWidth > maxWidth)
        {
            textWidth = Math.Max(0, textWidth - (contentWidth - maxWidth));
            contentWidth = maxWidth;
        }

        int x = (Width - contentWidth) / 2;
        if (icon != null)
        {
            if (_style == TileStyle.Muted)
            {
                ControlPaint.DrawImageDisabled(g, icon, x, (Height - icon.Height) / 2, Color.Transparent);
            }
            else
            {
                g.DrawImage(icon, x, (Height - icon.Height) / 2, icon.Width, icon.Height);
            }
            x += iconWidth + gap;
        }

        int y = (Height - textHeight) / 2;
        TextRenderer.DrawText(g, title, Font, new Rectangle(x, y, textWidth + 2, titleSize.Height), textColor,
            flags | TextFormatFlags.EndEllipsis | TextFormatFlags.Left);

        if (sub.Length > 0)
        {
            Color subColor = _style is TileStyle.Primary or TileStyle.Warning ? Color.FromArgb(214, 226, 234) : Palette.TextMuted;
            if (!Enabled || _style == TileStyle.Muted)
            {
                subColor = Palette.DisabledText;
            }
            TextRenderer.DrawText(g, sub, subFont, new Rectangle(x, y + titleSize.Height, textWidth + 2, subSize.Height), subColor,
                flags | TextFormatFlags.EndEllipsis | TextFormatFlags.Left);
        }
    }
}
