

using System.Runtime.InteropServices;
using Renderer.Contract;

public class StaRenderDispatcher : IDisposable
{
    private readonly Thread staThread;
    private readonly IRenderer renderer;
    private readonly AutoResetEvent newJobEvent = new(false);

    private volatile bool running = true;
    private RenderJob? latestJob;

    private class RenderJob
    {
        public float XMin, XMax, YMin, YMax;
        public int Width, Height;
        public TaskCompletionSource<byte[]> CompletionSource = new(TaskCreationOptions.RunContinuationsAsynchronously);
    }

    public StaRenderDispatcher(IRenderer renderer)
    {
        this.renderer = renderer;

        staThread = new Thread(renderLoop)
        {
            IsBackground = true,
            Name = "STA Render Thread"
        };
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))    
            staThread.SetApartmentState(ApartmentState.STA);
        staThread.Start();
    }

    public Task<byte[]> RenderAsync(float xMin, float xMax, float yMin, float yMax, int width, int height)
    {
        var job = new RenderJob
        {
            XMin = xMin,
            XMax = xMax,
            YMin = yMin,
            YMax = yMax,
            Width = width,
            Height = height
        };

        latestJob = job;
        newJobEvent.Set();

        return job.CompletionSource.Task;
    }

    private void renderLoop()
    {
        while (running)
        {
            newJobEvent.WaitOne();

            if (!running) break;

            var job = Interlocked.Exchange(ref latestJob, null);
            if (job is null) continue;

            try
            {
                var result = renderer.Render(job.XMin, job.XMax, job.YMin, job.YMax, job.Width, job.Height);
                job.CompletionSource.TrySetResult(result);
            }
            catch (Exception ex)
            {
                job.CompletionSource.TrySetException(ex);
            }
        }
    }

    public void Dispose()
    {
        running = false;
        newJobEvent.Set();
        staThread.Join();
    }
}
