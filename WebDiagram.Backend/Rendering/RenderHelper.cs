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

    public static bool IsInvisibleX(this RenderInfo renderInfo, float x)
    {
        return x < renderInfo.Margin.Left || x > renderInfo.Width - renderInfo.Margin.Right; 
    }

    public static bool IsInvisibleY(this RenderInfo renderInfo, float y)
    {
        return y < renderInfo.Margin.Top || y > renderInfo.Height - renderInfo.Margin.Bottom; 
    }

    public static float RangeX(this ViewPort viewPort)
    {
        return viewPort.XMax - viewPort.XMin;
    }

    public static float RangeY(this ViewPort viewPort)
    {        
        return viewPort.YMax - viewPort.YMin;
    }
}