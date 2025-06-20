using System.Runtime.InteropServices;
using Renderer.Contract;
using System.Threading;

public class StaRenderDispatcher : IDisposable
{
    private readonly Thread staThread;
    private readonly IRenderer renderer;
    private readonly AutoResetEvent newJobEvent = new(false);

    private volatile bool running = true;

    // NEU: Lock für Thread-Sicherheit
    private readonly object locker = new();

    // NEU: aktuell laufender Job
    private RenderJob? currentJob = null;

    // NEU: nächster Job (wird laufendem Job vorgezogen, überschreibt evtl. ältere)
    private RenderJob? nextJob = null;

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

        lock (locker)
        {
            if (currentJob == null)
            {
                currentJob = job;
                newJobEvent.Set();
            }
            else
            {
                // Verwerfe alten nextJob (optional: abbrechen)
                nextJob?.CompletionSource.TrySetCanceled();
                nextJob = job;
            }
        }

        return job.CompletionSource.Task;
    }

    private void renderLoop()
    {
        while (running)
        {
            newJobEvent.WaitOne();

            if (!running) break;

            RenderJob? jobToRender;
            lock (locker)
            {
                jobToRender = currentJob;
            }

            if (jobToRender == null)
            {
                newJobEvent.Reset();
                continue;
            }

            try
            {
                var result = renderer.Render(jobToRender.XMin, jobToRender.XMax, jobToRender.YMin, jobToRender.YMax, jobToRender.Width, jobToRender.Height);
                jobToRender.CompletionSource.TrySetResult(result);
            }
            catch (Exception ex)
            {
                jobToRender.CompletionSource.TrySetException(ex);
            }

            lock (locker)
            {
                currentJob = nextJob;
                nextJob = null;

                if (currentJob == null)
                    newJobEvent.Reset();
                else
                    newJobEvent.Set();
            }
        }
    }

    public void Dispose()
    {
        running = false;
        newJobEvent.Set();
        staThread.Join();
        newJobEvent.Dispose();
    }
}
