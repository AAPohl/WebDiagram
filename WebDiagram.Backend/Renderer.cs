using SkiaSharp;
using System;

public static class Renderer
{
    public static byte[] RenderCameraView(float xMin, float xMax, float yMin, float yMax, int width, int height)
    {
        var margin = new Margin(40, 40, 40, 40);
        var viewPort = new ViewPort(xMin, xMax, yMin, yMax);
        float pixelsPerXUnit = (width - margin.Left - margin.Right) / viewPort.RangeX();
        float pixelsPerYUnit = (height - margin.Top - margin.Bottom) / viewPort.RangeY();
        var renderInfo = new RenderInfo(margin, width, height, pixelsPerXUnit, pixelsPerYUnit, viewPort);

        using var bitmap = new SKBitmap(width, height);
        using var canvas = new SKCanvas(bitmap);

        ClearBackground(canvas);
        AxisRendering.DrawAxes(canvas, renderInfo);
        AxisRendering.DrawTicksAndLabels(canvas, renderInfo);

        DataRendering.DrawSine(canvas, renderInfo, 1f, 1f, 0f, SKColors.Red);
        DataRendering.DrawSine(canvas, renderInfo, 0.5f, 2f, (float)Math.PI / 4, SKColors.Green);
        DataRendering.DrawCross(canvas, renderInfo, 1f, 0.5f, 5, SKColors.Blue);

        DrawLabel(canvas, 20, 20, $"View X:[{xMin:0.00},{xMax:0.00}] Y:[{yMin:0.00},{yMax:0.00}]");

        using var image = SKImage.FromBitmap(bitmap);
        using var data = image.Encode(SKEncodedImageFormat.Png, 100);
        return data.ToArray();
    }

    private static void ClearBackground(SKCanvas canvas)
    {
        canvas.Clear(SKColors.White);
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
