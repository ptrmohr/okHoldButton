using System.ComponentModel;
using System.Drawing.Drawing2D;

namespace OkControls;

/// <summary>Darstellung eines <see cref="TileButton"/>.</summary>
public enum TileStyle
{
    /// <summary>Weiß mit grauem Rahmen.</summary>
    Normal,
    /// <summary>Blaugrau gefüllt, weiße Schrift (Hauptaktion).</summary>
    Primary,
    /// <summary>Orange gefüllt (z. B. Drucker nicht erreichbar).</summary>
    Warning,
    /// <summary>Sieht ausgegraut aus, bleibt aber bedienbar (für Halte-Buttons).</summary>
    Muted
}

/// <summary>
/// Großer, tablettauglicher Button wie in den Mockups: abgerundete Ecken, Icon links
/// vom Text, optional eine zweite kleinere Textzeile (<see cref="SubText"/>).
/// Wird komplett selbst gezeichnet, damit er auf allen Windows-Versionen gleich aussieht.
/// Basis für <see cref="HoldButton"/>.
/// </summary>
[DefaultEvent(nameof(Click))]
public class TileButton : Control, IButtonControl
{
    private string _subText = string.Empty;
    private Image? _icon;
    private TileStyle _style = TileStyle.Normal;
    private int _radius = 6;
    private bool _hover;
    private bool _pressed;
    private Color _accent = Palette.Accent;

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

    [Category("Appearance")]
    [DefaultValue("")]
    [Description("Zweite, kleinere Textzeile unter dem Text (z. B. \"Stand: 07.11.2026\").")]
    public string SubText
    {
        get => _subText;
        set { _subText = value ?? string.Empty; Invalidate(); }
    }

    [Category("Appearance")]
    [DefaultValue(null)]
    [Description("Icon links vom Text.")]
    public Image? Icon
    {
        get => _icon;
        set { _icon = value; Invalidate(); }
    }

    [Category("Appearance")]
    [DefaultValue(TileStyle.Normal)]
    public TileStyle TileStyle
    {
        get => _style;
        set { _style = value; Invalidate(); }
    }

    [Category("Appearance")]
    [DefaultValue(6)]
    public int CornerRadius
    {
        get => _radius;
        set { _radius = Math.Max(0, value); Invalidate(); }
    }

    [Category("Appearance")]
    [Description("Akzentfarbe: Füllung bei Primary, Fokusrahmen, Haltefüllung im vollen Zustand.")]
    public Color AccentColor
    {
        get => _accent;
        set { _accent = value; Invalidate(); }
    }

    private bool ShouldSerializeAccentColor() => _accent != Palette.Accent;

    private void ResetAccentColor() => AccentColor = Palette.Accent;

    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public DialogResult DialogResult { get; set; } = DialogResult.None;

    protected bool IsPressed => _pressed;

    public void NotifyDefault(bool value)
    {
    }

    public void PerformClick()
    {
        if (CanSelect)
        {
            OnClick(EventArgs.Empty);
        }
    }

    protected override void OnTextChanged(EventArgs e)
    {
        base.OnTextChanged(e);
        Invalidate();
    }

    protected override void OnMouseEnter(EventArgs e)
    {
        _hover = true;
        Invalidate();
        base.OnMouseEnter(e);
    }

    protected override void OnMouseLeave(EventArgs e)
    {
        _hover = false;
        _pressed = false;
        Invalidate();
        base.OnMouseLeave(e);
    }

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

    protected override void OnMouseUp(MouseEventArgs e)
    {
        _pressed = false;
        Invalidate();
        base.OnMouseUp(e);
    }

    protected override void OnClick(EventArgs e)
    {
        base.OnClick(e);
        if (DialogResult != DialogResult.None && FindForm() is Form form)
        {
            form.DialogResult = DialogResult;
        }
    }

    protected override void OnKeyUp(KeyEventArgs e)
    {
        if (e.KeyCode is Keys.Space or Keys.Enter)
        {
            PerformClick();
            e.Handled = true;
        }
        base.OnKeyUp(e);
    }

    protected override void OnEnabledChanged(EventArgs e)
    {
        base.OnEnabledChanged(e);
        Invalidate();
    }

    protected override void OnGotFocus(EventArgs e)
    {
        base.OnGotFocus(e);
        Invalidate();
    }

    protected override void OnLostFocus(EventArgs e)
    {
        base.OnLostFocus(e);
        Invalidate();
    }

    /// <summary>Hintergrundfarbe abhängig von Stil und Zustand.</summary>
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

    protected virtual Color GetTextColor()
    {
        if (!Enabled || _style == TileStyle.Muted)
        {
            return Palette.DisabledText;
        }

        return _style is TileStyle.Primary or TileStyle.Warning ? Color.White : ForeColor;
    }

    protected Color GetBorderColor() => _style switch
    {
        TileStyle.Primary => AccentColor,
        TileStyle.Warning => Palette.Warning,
        TileStyle.Muted => Color.FromArgb(213, 220, 222),
        _ => Palette.Border
    };

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

    /// <summary>Für abgeleitete Klassen: zwischen Hintergrund und Inhalt zeichnen (z. B. Fortschritt).</summary>
    protected virtual void PaintOverlay(Graphics g, RectangleF rect, float radius)
    {
    }

    /// <summary>Icon und Text(e) mittig zeichnen.</summary>
    protected virtual void PaintContent(Graphics g, Color textColor)
    {
        PaintContent(g, textColor, Text, _subText);
    }

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
            icon = null; // ausgegraute Buttons ohne farbiges Icon
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
