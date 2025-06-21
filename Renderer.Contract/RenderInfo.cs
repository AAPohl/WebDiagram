public struct RenderInfo
{
    public RenderInfo(Margin margin, int width, int height, float pixelsPerXUnit, float pixelsPerYUnit, ViewPort viewPort)
    {
        Margin = margin;
        Width = width;
        Height = height;
        PixelsPerXUnit = pixelsPerXUnit;
        PixelsPerYUnit = pixelsPerYUnit;
        ViewPort = viewPort;
    }
    public Margin Margin { get; }
    public int Width { get; }
    public int Height { get; }
    public float PixelsPerXUnit { get; }
    public float PixelsPerYUnit { get; }
    public ViewPort ViewPort { get; }
}

public struct Margin
{
    public Margin(int top, int bottom, int left, int right)
    {
        Top = top;
        Bottom = bottom;
        Left = left;
        Right = right;
    }
    public int Top { get; }
    public int Bottom { get; }
    public int Left { get; }
    public int Right { get; }
}

public struct ViewPort
{
    public ViewPort(float xMin, float xMax, float yMin, float yMax)
    {
        XMin = xMin;
        XMax = xMax;
        YMin = yMin;
        YMax = yMax;
    }
    public float XMin { get; }
    public float XMax { get; }
    public float YMin { get; }
    public float YMax { get; }    
}
