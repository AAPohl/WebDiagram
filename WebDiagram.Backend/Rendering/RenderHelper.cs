using SkiaSharp;
using System;

public static class RenderHelper
{
    public static float ToGlobalX(this RenderInfo renderInfo, float x)
    {
        return renderInfo.Margin.Left + (x - renderInfo.ViewPort.XMin) * renderInfo.PixelsPerXUnit;
    }

    public static float ToGlobalY(this RenderInfo renderInfo, float y)
    {
        return renderInfo.Height - renderInfo.Margin.Top - (y - renderInfo.ViewPort.YMin) * renderInfo.PixelsPerYUnit;
    }
}