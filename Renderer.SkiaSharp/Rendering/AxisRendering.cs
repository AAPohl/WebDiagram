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

        var metrics = font.Metrics;
        float textHeight = metrics.Descent - metrics.Ascent;

        drawTicksAndLabelsX(canvas, renderInfo, textHeight, font, paint);
        drawTicksAndLabelsY(canvas, renderInfo, textHeight, font, paint);        
    }
    private static void drawTicksAndLabelsX(SKCanvas canvas, RenderInfo renderInfo, float textHeight, SKFont font, SKPaint paint)
    {
        int tickSize = 4;
        float tickStepX = calculateNiceTickStep(renderInfo.ViewPort.RangeX(), 5); // z. B. 0.2, 0.5, 1, 2, 5
        float firstTickX = (float)Math.Floor(renderInfo.ViewPort.XMin / tickStepX) * tickStepX;
        int originY = renderInfo.Height - renderInfo.Margin.Bottom;

        for (float val = firstTickX; val <= renderInfo.ViewPort.XMax; val += tickStepX)
        {
            float px = renderInfo.ToGlobalX(val);
            if (renderInfo.IsInvisibleX(px))
                continue;

            canvas.DrawLine(px, originY - tickSize, px, originY + tickSize, paint);

            string label = val.ToString("0.00");
            float textWidth = font.MeasureText(label);
            canvas.DrawText(label, px, originY + tickSize + textHeight + 2, SKTextAlign.Center, font, paint);
        }
    }

    private static void drawTicksAndLabelsY(SKCanvas canvas, RenderInfo renderInfo, float textHeight, SKFont font, SKPaint paint)
    {
        int tickSize = 4;
        float tickStepY = calculateNiceTickStep(renderInfo.ViewPort.RangeY(), 5);
        float firstTickY = (float)Math.Floor(renderInfo.ViewPort.YMin / tickStepY) * tickStepY;
        int originX = renderInfo.Margin.Left;

        for (float val = firstTickY; val <= renderInfo.ViewPort.YMax; val += tickStepY)
        {
            float py = renderInfo.ToGlobalY(val);
            if (renderInfo.IsInvisibleY(py))
                continue;

            canvas.DrawLine(originX - tickSize, py, originX + tickSize, py, paint);

            string label = val.ToString("0.00");
            canvas.DrawText(label, originX - tickSize - 5, py + textHeight / 2, SKTextAlign.Right, font, paint);
        }
    }

    private static float calculateNiceTickStep(float range, int desiredTickCount)
    {
        float roughStep = range / desiredTickCount;
        float exponent = (float)Math.Floor(Math.Log10(roughStep));
        float baseStep = (float)Math.Pow(10, exponent);

        float[] niceSteps = { 1, 2, 5, 10 };
        foreach (var step in niceSteps)
        {
            if (baseStep * step >= roughStep)
                return baseStep * step;
        }

        return baseStep * 10; // Fallback
    }

}
