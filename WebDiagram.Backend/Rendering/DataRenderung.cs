using SkiaSharp;
using System;

public static class DataRendering
{
    public static void DrawCross(SKCanvas canvas, RenderInfo renderInfo,
        float x, float y, float size, SKColor color)
    {
        float px = renderInfo.Margin.Left + (x - renderInfo.ViewPort.XMin) * renderInfo.PixelsPerXUnit;
        float py = renderInfo.Height - renderInfo.Margin.Top - (y - renderInfo.ViewPort.YMin) * renderInfo.PixelsPerYUnit;

        using var paint = new SKPaint
        {
            Color = color,
            StrokeWidth = 2,
            IsAntialias = true
        };

        canvas.DrawLine(px - size, py - size, px + size, py + size, paint);
        canvas.DrawLine(px - size, py + size, px + size, py - size, paint);
    }
}
