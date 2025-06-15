using SkiaSharp;
using System;

public static class Renderer
{
    public static byte[] RenderCameraView(float xMin, float xMax, float yMin, float yMax)
    {
        
        int width = 600;
        int height = 400;
        int margin = 40;
        var margin1 = new Margin(margin, margin, margin, margin);
        var viewPort1 = new ViewPort(xMin, xMax, yMin, yMax);
        float pixelsPerXUnit = (width - 2 * margin) / (xMax - xMin);
        float pixelsPerYUnit = (height - 2 * margin) / (yMax - yMin);
        var renderInfo = new RenderInfo(margin1, width, height, pixelsPerXUnit, pixelsPerYUnit, viewPort1);

        using var bitmap = new SKBitmap(width, height);
        using var canvas = new SKCanvas(bitmap);

        ClearBackground(canvas);
        AxisRendering.DrawAxes(canvas, renderInfo);
        AxisRendering.DrawTicksAndLabels(canvas, renderInfo);

        DataRendering.DrawSine(canvas, renderInfo, 1f, 1f, 0f, SKColors.Red);
        DataRendering.DrawSine(canvas, renderInfo, 0.5f, 2f, (float)Math.PI / 4, SKColors.Green);
        DataRendering.DrawCross(canvas, renderInfo, 1f, 0.5f, 5, SKColors.Blue);

        DrawLabel(canvas, margin, margin / 2, $"View X:[{xMin:0.00},{xMax:0.00}] Y:[{yMin:0.00},{yMax:0.00}]");

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
