using SkiaSharp;

public static class DataRendering
{
    public static void DrawCross(SKCanvas canvas, RenderInfo renderInfo,
        float x, float y, float size, SKColor color, bool hovered)
    {
        float px = renderInfo.ToGlobalX(x);
        float py = renderInfo.ToGlobalY(y);

        using var paint = new SKPaint
        {
            Color = color,
            StrokeWidth = hovered ? 4 : 2,
            IsAntialias = true
        };

        canvas.DrawLine(px - size, py - size, px + size, py + size, paint);
        canvas.DrawLine(px - size, py + size, px + size, py - size, paint);
    }

    public static bool IsNearCross(float hoverX, float hoverY, float x, float y, float size, float v)
    {
        // Punkte des Kreuzes (im selben Koordinatensystem wie hoverX, hoverY)
        float x1a = x - size;
        float y1a = y - size;
        float x1b = x + size;
        float y1b = y + size;

        float x2a = x - size;
        float y2a = y + size;
        float x2b = x + size;
        float y2b = y - size;

        var p = new SKPoint(hoverX, hoverY);
        var a1 = new SKPoint(x1a, y1a);
        var b1 = new SKPoint(x1b, y1b);
        var a2 = new SKPoint(x2a, y2a);
        var b2 = new SKPoint(x2b, y2b);

        float dist1 = DistancePointToLine(p, a1, b1);
        float dist2 = DistancePointToLine(p, a2, b2);

        return dist1 <= v || dist2 <= v;
    }

    private static float DistancePointToLine(SKPoint p, SKPoint a, SKPoint b)
    {
        float dx = b.X - a.X;
        float dy = b.Y - a.Y;

        if (dx == 0 && dy == 0)
            return Distance(p, a);

        float t = ((p.X - a.X) * dx + (p.Y - a.Y) * dy) / (dx * dx + dy * dy);
        t = MathF.Max(0, MathF.Min(1, t));

        float projX = a.X + t * dx;
        float projY = a.Y + t * dy;

        return Distance(p, new SKPoint(projX, projY));
    }

    private static float Distance(SKPoint p1, SKPoint p2)
    {
        return MathF.Sqrt((p1.X - p2.X) * (p1.X - p2.X) + (p1.Y - p2.Y) * (p1.Y - p2.Y));
    }


    public static void DrawSine(SKCanvas canvas, RenderInfo renderInfo,
        float amplitude, float frequency, float phase, SKColor color, bool hovered)
    {
        using var paint = new SKPaint
        {
            Color = color,
            StrokeWidth = hovered ? 4 : 2,
            IsAntialias = true,
            Style = SKPaintStyle.Stroke
        };

        using var path = new SKPath();
        bool started = false;

        int pointCount = renderInfo.Width - renderInfo.Margin.Left - renderInfo.Margin.Right;

        float xRange = renderInfo.ViewPort.RangeX();
        float originY = renderInfo.ToGlobalY(0);

        for (int i = 0; i <= pointCount; i++)
        {
            // x linear interpolieren über den Bereich [xMin, xMax]
            float x = renderInfo.ViewPort.XMin + xRange * i / pointCount;

            // Y-Wert der Funktion relativ zum aktuellen Viewport
            float y = amplitude * MathF.Sin(frequency * x + phase);

            // Pixelkoordinaten berechnen
            float px = renderInfo.ToGlobalX(x);
            float py = renderInfo.ToGlobalY(y);

            if (!started)
            {
                path.MoveTo(px, py);
                started = true;
            }
            else
            {
                path.LineTo(px, py);
            }
        }

        canvas.DrawPath(path, paint);
    }

    public static bool IsNearSine(float hoverX, float hoverY, float amplitude, float frequency, float phase, float v)
    {
        // Berechne Kurven-y am Punkt x
        float yCurve = amplitude * MathF.Sin(frequency * hoverX + phase);

        // Prüfe Abstand
        return MathF.Abs(hoverY - yCurve) <= v;
    }
}
