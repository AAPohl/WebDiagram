using SkiaSharp;

public static class DataRendering
{
    public static void DrawCross(SKCanvas canvas, RenderInfo renderInfo,
        float x, float y, float size, SKColor color)
    {
        float px = renderInfo.ToGlobalX(x);
        float py = renderInfo.ToGlobalY(y);

        using var paint = new SKPaint
        {
            Color = color,
            StrokeWidth = 2,
            IsAntialias = true
        };

        canvas.DrawLine(px - size, py - size, px + size, py + size, paint);
        canvas.DrawLine(px - size, py + size, px + size, py - size, paint);
    }

    public static void DrawSine(SKCanvas canvas, RenderInfo renderInfo,
        float amplitude, float frequency, float phase, SKColor color)
    {
        using var paint = new SKPaint
        {
            Color = color,
            StrokeWidth = 2,
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
}
