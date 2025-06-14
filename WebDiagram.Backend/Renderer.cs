using SkiaSharp;
using System;

public static class Renderer
{
    public static byte[] RenderCameraView(float xMin, float xMax, float yMin, float yMax)
    {
        int width = 600;
        int height = 400;
        int margin = 40;

        using var bitmap = new SKBitmap(width, height);
        using var canvas = new SKCanvas(bitmap);

        ClearBackground(canvas);
        DrawAxes(canvas, width, height, margin);
        DrawTicksAndLabels(canvas, width, height, margin, xMin, xMax, yMin, yMax);

        float pixelsPerXUnit = (width - 2 * margin) / (xMax - xMin);
        float pixelsPerYUnit = (height - 2 * margin) / (yMax - yMin);

        // Ursprung in Pixelkoordinaten
        float originX = margin - xMin * pixelsPerXUnit;          // x=0 pixel-Position relativ zum margin
        float originY = height - margin + yMin * pixelsPerYUnit; // y=0 pixel-Position relativ zum margin (invertiert y-Achse!)

        // Die Achse bleibt an (margin, height-margin) für (xMin,yMin)
        // Deshalb müssen wir Sinus-Kurven relativ zu (xMin,yMin) zeichnen

        DrawSine(canvas, margin, width, height, pixelsPerXUnit, pixelsPerYUnit, xMin, xMax, yMin, yMax, 1f, 1f, 0f, SKColors.Red);

        DrawSine(canvas, margin, width, height, pixelsPerXUnit, pixelsPerYUnit, xMin, xMax, yMin, yMax, 0.5f, 2f, (float)Math.PI / 4, SKColors.Green);

        DrawCross(canvas, 1f, 0.5f, margin, width, height, pixelsPerXUnit, pixelsPerYUnit, xMin, xMax, yMin, yMax, 5, SKColors.Blue);

        DrawLabel(canvas, margin, margin / 2, $"View X:[{xMin:0.00},{xMax:0.00}] Y:[{yMin:0.00},{yMax:0.00}]");

        using var image = SKImage.FromBitmap(bitmap);
        using var data = image.Encode(SKEncodedImageFormat.Png, 100);
        return data.ToArray();
    }

    private static void ClearBackground(SKCanvas canvas)
    {
        canvas.Clear(SKColors.White);
    }

    private static void DrawAxes(SKCanvas canvas, int width, int height, int margin)
    {
        using var paint = new SKPaint
        {
            Color = SKColors.Black,
            IsAntialias = true,
            StrokeWidth = 2,
            Style = SKPaintStyle.Stroke
        };

        int originX = margin;
        int originY = height - margin;

        // X-Achse
        canvas.DrawLine(originX, originY, width - margin, originY, paint);
        DrawArrowHead(canvas, width - margin, originY, true, paint);

        // Y-Achse
        canvas.DrawLine(originX, originY, originX, margin, paint);
        DrawArrowHead(canvas, originX, margin, false, paint);
    }

    private static void DrawArrowHead(SKCanvas canvas, float x, float y, bool horizontal, SKPaint paint)
    {
        float size = 10;
        using var path = new SKPath();

        if (horizontal)
        {
            path.MoveTo(x, y);
            path.LineTo(x - size, y - size / 2);
            path.LineTo(x - size, y + size / 2);
        }
        else
        {
            path.MoveTo(x, y);
            path.LineTo(x - size / 2, y + size);
            path.LineTo(x + size / 2, y + size);
        }
        path.Close();
        canvas.DrawPath(path, paint);
    }

    private static void DrawTicksAndLabels(SKCanvas canvas, int width, int height, int margin, float xMin, float xMax, float yMin, float yMax)
    {
        using var paint = new SKPaint
        {
            Color = SKColors.Black,
            IsAntialias = true,
            StrokeWidth = 1,
            Style = SKPaintStyle.Stroke
        };

        using var font = new SKFont
        {
            Size = 12,
            Edging = SKFontEdging.Antialias,
            Subpixel = true
        };

        int originX = margin;
        int originY = height - margin;
        int tickSize = 4;

        var metrics = font.Metrics;
        float textHeight = metrics.Descent - metrics.Ascent;

        float xRange = xMax - xMin;
        float yRange = yMax - yMin;
        float pixelsPerXUnit = (width - 2 * margin) / xRange;
        float pixelsPerYUnit = (height - 2 * margin) / yRange;

        // X-Achse Ticks - rund auf sinnvolle Schritte (hier grob 5 ticks)
        int xTicksCount = 5;
        for (int i = 0; i <= xTicksCount; i++)
        {
            float val = xMin + i * xRange / xTicksCount;
            float px = originX + (val - xMin) * pixelsPerXUnit;
            if (px < margin || px > width - margin) continue;

            canvas.DrawLine(px, originY - tickSize, px, originY + tickSize, paint);

            string label = val.ToString("0.00");
            float textWidth = font.MeasureText(label);
            canvas.DrawText(label, px, originY + tickSize + textHeight + 2, SKTextAlign.Center, font, paint);
        }

        // Y-Achse Ticks - auch 5 Ticks
        int yTicksCount = 5;
        for (int i = 0; i <= yTicksCount; i++)
        {
            float val = yMin + i * yRange / yTicksCount;
            float py = originY - (val - yMin) * pixelsPerYUnit;
            if (py < margin || py > height - margin) continue;

            canvas.DrawLine(originX - tickSize, py, originX + tickSize, py, paint);

            string label = val.ToString("0.00");
            canvas.DrawText(label, originX - tickSize - 5, py + textHeight / 2, SKTextAlign.Right, font, paint);
        }
    }

    private static void DrawSine(SKCanvas canvas, float margin, int width, int height,
    float pixelsPerXUnit, float pixelsPerYUnit,
    float xMin, float xMax, float yMin, float yMax,
    float amplitude, float frequency, float phase,
    SKColor color)
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

    int pointCount = width - 2 * (int)margin; // Zeichne pro Pixel innerhalb der Box

    float xRange = xMax - xMin;
    float originY = height - margin + yMin * pixelsPerYUnit; // y=0 in Pixeln

    for (int i = 0; i <= pointCount; i++)
    {
        // x linear interpolieren über den Bereich [xMin, xMax]
        float x = xMin + (xRange * i) / pointCount;

        // Y-Wert der Funktion relativ zum aktuellen Viewport
        float y = yMin + amplitude * MathF.Sin(frequency * x + phase);

        // Pixelkoordinaten berechnen
        float px = margin + (x - xMin) * pixelsPerXUnit;
        float py = originY - (y - yMin) * pixelsPerYUnit;

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


    private static void DrawLabel(SKCanvas canvas, int x, int y, string text)
    {
        using var paint = new SKPaint
        {
            Color = SKColors.Black,
            IsAntialias = true
        };

        using var font = new SKFont
        {
            Size = 16,
            Edging = SKFontEdging.Antialias,
            Subpixel = true
        };

        canvas.DrawText(text, x, y, SKTextAlign.Left, font, paint);
    }
    
    private static void DrawCross(SKCanvas canvas, float x, float y,
    float margin, int width, int height,
    float pixelsPerXUnit, float pixelsPerYUnit,
    float xMin, float xMax, float yMin, float yMax,
    float size, SKColor color)
{
    // Weltkoordinaten → Pixelkoordinaten
    float px = margin + (x - xMin) * pixelsPerXUnit;
    float py = height - margin - (y - yMin) * pixelsPerYUnit;

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
