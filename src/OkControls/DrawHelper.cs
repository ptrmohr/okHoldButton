using System.Drawing.Drawing2D;

namespace OkControls;

/// <summary>Zeichenhilfen für abgerundete Flächen.</summary>
internal static class DrawHelper
{
    public static GraphicsPath RoundedRect(RectangleF r, float radius)
    {
        var path = new GraphicsPath();
        if (radius <= 0.5f)
        {
            path.AddRectangle(r);
            return path;
        }

        float d = Math.Min(radius * 2, Math.Min(r.Width, r.Height));
        path.AddArc(r.X, r.Y, d, d, 180, 90);
        path.AddArc(r.Right - d, r.Y, d, d, 270, 90);
        path.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
        path.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
        path.CloseFigure();
        return path;
    }

    public static void FillRounded(Graphics g, Color color, RectangleF r, float radius)
    {
        using GraphicsPath path = RoundedRect(r, radius);
        using var brush = new SolidBrush(color);
        g.FillPath(brush, path);
    }

    public static void BorderRounded(Graphics g, Color color, RectangleF r, float radius, float width = 1f)
    {
        float inset = width / 2f;
        var inner = new RectangleF(r.X + inset, r.Y + inset, r.Width - width, r.Height - width);
        using GraphicsPath path = RoundedRect(inner, radius);
        using var pen = new Pen(color, width);
        g.DrawPath(pen, path);
    }
}
