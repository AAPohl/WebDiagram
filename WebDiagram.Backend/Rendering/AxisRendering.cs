using SkiaSharp;

public static class AxisRendering
{
    public static void DrawAxes(SKCanvas canvas, RenderInfo renderInfo)
    {
        using var paint = new SKPaint
        {
            Color = SKColors.Black,
            IsAntialias = true,
            StrokeWidth = 2,
            Style = SKPaintStyle.Stroke
        };

        int originX = renderInfo.Margin.Left;
        int originY = renderInfo.Height - renderInfo.Margin.Bottom;

        // X-Achse
        canvas.DrawLine(originX, originY, renderInfo.Width - renderInfo.Margin.Right, originY, paint);
        drawArrowHeadX(canvas, renderInfo.Width - renderInfo.Margin.Right, originY, paint);

        // Y-Achse
        canvas.DrawLine(originX, originY, originX, renderInfo.Margin.Top, paint);
        drawArrowHeadY(canvas, originX, renderInfo.Margin.Top, paint);
    }
    
    public static void DrawTicksAndLabels(SKCanvas canvas, RenderInfo renderInfo)
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

        int originX = renderInfo.Margin.Left;
        int originY = renderInfo.Height - renderInfo.Margin.Bottom;
        int tickSize = 4;

        var metrics = font.Metrics;
        float textHeight = metrics.Descent - metrics.Ascent;

        float xRange = renderInfo.ViewPort.RangeX();
        float yRange = renderInfo.ViewPort.RangeY();

        // X-Achse Ticks - rund auf sinnvolle Schritte (hier grob 5 ticks)
        int xTicksCount = 5;
        for (int i = 0; i <= xTicksCount; i++)
        {
            float val = renderInfo.ViewPort.XMin + i * xRange / xTicksCount;
            float px = renderInfo.ToGlobalX(val);
            if (renderInfo.IsInvisibleX(px))
                continue;

            canvas.DrawLine(px, originY - tickSize, px, originY + tickSize, paint);

            string label = val.ToString("0.00");
            float textWidth = font.MeasureText(label);
            canvas.DrawText(label, px, originY + tickSize + textHeight + 2, SKTextAlign.Center, font, paint);
        }

        // Y-Achse Ticks - auch 5 Ticks
        int yTicksCount = 5;
        for (int i = 0; i <= yTicksCount; i++)
        {
            float val = renderInfo.ViewPort.YMin + i * yRange / yTicksCount;
            float py = renderInfo.ToGlobalY(val);
            if (renderInfo.IsInvisibleY(py))
                continue;

            canvas.DrawLine(originX - tickSize, py, originX + tickSize, py, paint);

            string label = val.ToString("0.00");
            canvas.DrawText(label, originX - tickSize - 5, py + textHeight / 2, SKTextAlign.Right, font, paint);
        }
    }

    private static void drawArrowHeadX(SKCanvas canvas, float x, float y, SKPaint paint)
    {
        float size = 10;
        using var path = new SKPath();


        path.MoveTo(x, y);
        path.LineTo(x - size, y - size / 2);
        path.LineTo(x - size, y + size / 2);

        path.Close();
        canvas.DrawPath(path, paint);
    }

    private static void drawArrowHeadY(SKCanvas canvas, float x, float y, SKPaint paint)
    {
        float size = 10;
        using var path = new SKPath();
        
        path.MoveTo(x, y);
        path.LineTo(x - size / 2, y + size);
        path.LineTo(x + size / 2, y + size);
        
        path.Close();
        canvas.DrawPath(path, paint);
    }
}