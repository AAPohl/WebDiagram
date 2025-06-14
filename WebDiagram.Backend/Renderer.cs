using SkiaSharp;
using System;

public static class Renderer
{
    public static byte[] RenderCameraView(float x, float y, float z)
    {
        int width = 400;
        int height = 300;
        int margin = 40;

        using var bitmap = new SKBitmap(width, height);
        using var canvas = new SKCanvas(bitmap);

        ClearBackground(canvas);
        DrawAxes(canvas, width, height, margin);
        DrawTicksAndLabels(canvas, width, height, margin);

        float xRange = (float)(2 * Math.PI);
        float yScale = 40f;
        float xPixelsPerUnit = (width - 2 * margin) / xRange;

        DrawSine(canvas, margin, height - margin, xPixelsPerUnit, yScale, 1f, 1f, 0f, SKColors.Red);
        DrawSine(canvas, margin, height - margin, xPixelsPerUnit, yScale, 0.5f, 2f, (float)Math.PI / 4, SKColors.Green);

        DrawLabel(canvas, margin, margin / 2, $"Camera @ X:{x:0.0} Y:{y:0.0} Z:{z:0.0}");

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
        paint.IsAntialias = true;
        float size = 10;

        using var path = new SKPath();
        if (horizontal)
        {
            path.MoveTo(x, y);
            path.LineTo(x - size, y - size / 2);
            path.LineTo(x - size, y + size / 2);
            path.Close();
        }
        else
        {
            path.MoveTo(x, y);
            path.LineTo(x - size / 2, y + size);
            path.LineTo(x + size / 2, y + size);
            path.Close();
        }
        canvas.DrawPath(path, paint);
    }

    private static void DrawTicksAndLabels(SKCanvas canvas, int width, int height, int margin)
    {
        using var paint = new SKPaint
        {
            Color = SKColors.Black,
            IsAntialias = true,
            StrokeWidth = 1,
            Style = SKPaintStyle.Stroke
            // TextSize NICHT setzen!
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
        int tickSpacingX = 40;
        int tickSpacingY = 20;

        var metrics = font.Metrics;
        float textHeight = metrics.Descent - metrics.Ascent;

        // X-Achse Ticks + Beschriftung (0, π/2, π, ...)
        for (int i = 0; i <= 6; i++)
        {
            int xTick = originX + i * tickSpacingX;
            if (xTick > width - margin) break;

            canvas.DrawLine(xTick, originY - tickSize, xTick, originY + tickSize, paint);

            string label = i switch
            {
                0 => "0",
                2 => "π",
                4 => "2π",
                _ when i % 2 == 1 => $"{i}/2π",
                _ => ""
            };

            if (!string.IsNullOrEmpty(label))
            {
                float textWidth = font.MeasureText(label);
                canvas.DrawText(label, xTick, originY + tickSize + textHeight + 2, SKTextAlign.Center, font, paint);
            }
        }

        // Y-Achse Ticks + Beschriftung (-1 bis 1)
        float[] labels = { 1f, 0.5f, 0f, -0.5f, -1f };
        for (int i = 0; i < labels.Length; i++)
        {
            int yTick = originY - i * tickSpacingY;
            canvas.DrawLine(originX - tickSize, yTick, originX + tickSize, yTick, paint);

            string label = labels[i].ToString("0.0");
            float textWidth = font.MeasureText(label);
            canvas.DrawText(label, originX - tickSize - 5, yTick + textHeight / 2, SKTextAlign.Right, font, paint);
        }
    }

    private static void DrawSine(SKCanvas canvas, int originX, int originY,
        float xPixelsPerUnit, float yScale,
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

        for (float x = 0; x <= 2 * MathF.PI; x += 0.01f)
        {
            float y = amplitude * MathF.Sin(frequency * x + phase);
            float px = originX + x * xPixelsPerUnit;
            float py = originY - y * yScale;

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
}
