using Renderer.Contract;
using SkiaSharp;

public class SkiaSharpRenderer : IRenderer
{
	private readonly Action<byte[]> renderAction;
	private readonly int maxFps;
	private Thread? renderThread;
	private bool running;

	public Margin Margin { get; set; } = new Margin(40, 40, 40, 40);
    private RenderInfo renderInfo;
	public SkiaSharpRenderer(int maxFps, Action<byte[]> renderAction)
	{
		this.renderAction = renderAction;
		renderInfo = new RenderInfo(Margin, 100, 100, 1f, 1f, new ViewPort(-10f, 10f, -10f, 10f));

		if (maxFps <= 0) throw new ArgumentOutOfRangeException(nameof(maxFps));

		this.maxFps = maxFps;
		renderThread = new Thread(renderLoop) { IsBackground = true };
	}

	public void Start()
	{
		if (running) return;
		running = true;
		renderThread = new Thread(renderLoop) { IsBackground = true };
		renderThread.Start();
	}

	public void Stop()
	{
		running = false;
		renderThread?.Join();
	}

	private void renderLoop()
	{
		var frameTimeMs = 1000.0 / maxFps;

		while (running)
		{
			var start = DateTime.UtcNow;

			var image = Render();

			renderAction(image);			

			var elapsed = (DateTime.UtcNow - start).TotalMilliseconds;
			var sleepTime = frameTimeMs - elapsed;
			if (sleepTime > 0)
			{
				Thread.Sleep((int)sleepTime);
			}
		}
	}

	public void UpdateViewport(float xMin, float xMax, float yMin, float yMax)
    {
        var viewPort = new ViewPort(xMin, xMax, yMin, yMax);
		float pixelsPerXUnit = (renderInfo.Width - Margin.Left - Margin.Right) / viewPort.RangeX();
		float pixelsPerYUnit = (renderInfo.Height - Margin.Top - Margin.Bottom) / viewPort.RangeY();
		renderInfo = new RenderInfo(Margin, renderInfo.Width, renderInfo.Height, pixelsPerXUnit, pixelsPerYUnit, viewPort);
	}

	public void UpdateSize(int width, int height)
	{
		float pixelsPerXUnit = (width - Margin.Left - Margin.Right) / renderInfo.ViewPort.RangeX();
		float pixelsPerYUnit = (height - Margin.Top - Margin.Bottom) / renderInfo.ViewPort.RangeY();
		renderInfo = new RenderInfo(Margin, width, height, pixelsPerXUnit, pixelsPerYUnit, renderInfo.ViewPort);
	}
	public byte[] Render()
    {
        using var bitmap = new SKBitmap(renderInfo.Width, renderInfo.Height);
        using var canvas = new SKCanvas(bitmap);

        canvas.Clear(SKColors.White);
        AxisRendering.DrawAxes(canvas, renderInfo);
        AxisRendering.DrawTicksAndLabels(canvas, renderInfo);

        DataRendering.DrawSine(canvas, renderInfo, 1f, 1f, 0f, SKColors.Red);
        DataRendering.DrawSine(canvas, renderInfo, 0.5f, 2f, (float)Math.PI / 4, SKColors.Green);
        DataRendering.DrawCross(canvas, renderInfo, 1f, 0.5f, 5, SKColors.Blue);

        DrawLabel(canvas, Margin.Left / 2, Margin.Top / 2, $"View X:[{renderInfo.ViewPort.XMin:0.00},{renderInfo.ViewPort.XMax:0.00}] Y:[{renderInfo.ViewPort.YMin:0.00},{renderInfo.ViewPort.YMax:0.00}] Width:[{renderInfo.Width}] Height:[{renderInfo.Height}]");

        using var image = SKImage.FromBitmap(bitmap);
        using var data = image.Encode(SKEncodedImageFormat.Png, 100);

        return data.ToArray();
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
