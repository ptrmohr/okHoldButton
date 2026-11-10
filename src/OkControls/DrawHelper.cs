using System.Drawing.Drawing2D;

namespace OkControls;

/// <summary>
/// Small drawing helpers for rounded shapes, used by TileButton and HoldButton.
/// (The app has its own copy in OkPriceTag.UI.Draw so the library stays independent.)
/// </summary>
internal static class DrawHelper
{
    /// <summary>
    /// Builds a path for a rectangle with rounded corners. A radius of 0.5 or less gives a plain rectangle;
    /// the radius is limited so the corners never overlap on small rectangles. The caller disposes the path.
    /// </summary>
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

    /// <summary>Fills a rounded rectangle with a solid colour.</summary>
    public static void FillRounded(Graphics g, Color color, RectangleF r, float radius)
    {
        using GraphicsPath path = RoundedRect(r, radius);
        using var brush = new SolidBrush(color);
        g.FillPath(brush, path);
    }

    /// <summary>
    /// Draws the outline of a rounded rectangle. The path is inset by half the pen width so the whole border
    /// stays inside <paramref name="r"/> (otherwise half of it would be clipped at the control edge).
    /// </summary>
    public static void BorderRounded(Graphics g, Color color, RectangleF r, float radius, float width = 1f)
    {
        float inset = width / 2f;
        var inner = new RectangleF(r.X + inset, r.Y + inset, r.Width - width, r.Height - width);
        using GraphicsPath path = RoundedRect(inner, radius);
        using var pen = new Pen(color, width);
        g.DrawPath(pen, path);
    }
}
