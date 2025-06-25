using Microsoft.AspNetCore.SignalR;
using Renderer.Contract;

public class DiagramManager
{
    private readonly Dictionary<string, IRenderer> renderer;
    private readonly IHubContext<DiagramHub> hubContext;
    public DiagramManager(IHubContext<DiagramHub> hubContext)
    {
        this.hubContext = hubContext;
        renderer = new Dictionary<string, IRenderer>();
    }

    public void AddRenderer(string instanceId, Func<Action<byte[]>, IRenderer> renderFactory) => renderer.Add(instanceId, renderFactory(image => hubContext.Clients.Group(instanceId).SendAsync("ReceiveImage", Convert.ToBase64String(image)).GetAwaiter().GetResult()));

    public IRenderer GetRenderer(string instanceId) => renderer[instanceId];

    public void Start()
    {
        foreach (var renderer in renderer)
            renderer.Value.Start();
    }
    
    public void Stop()
    {
        foreach(var renderer in renderer)
	        renderer.Value.Stop();
    }

}