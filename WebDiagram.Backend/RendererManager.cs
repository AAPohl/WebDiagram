using System.Collections.Concurrent;
using Microsoft.AspNetCore.SignalR;
using Renderer.Contract;

public class RendererManager
{
    private readonly IHubContext<DiagramHub> hubContext;
    private readonly Func<Action<byte[]>, IRenderer> rendererFactory;
    private readonly ConcurrentDictionary<string, IRenderer> instances = new();

    public RendererManager(IHubContext<DiagramHub> hubContext, Func<Action<byte[]>, IRenderer> rendererFactory)
    {
        this.hubContext = hubContext;
        this.rendererFactory = rendererFactory;
    }

    public IRenderer GetOrCreateInstance(string instanceId)
    {
        return instances.GetOrAdd(instanceId, id =>
        {
            return rendererFactory.Invoke(image =>
            {
                hubContext.Clients.Group(id).SendAsync("ReceiveImage", Convert.ToBase64String(image)).GetAwaiter().GetResult();
            });
        });
    }

    public bool RemoveInstance(string instanceId)
    {
        if (instances.TryRemove(instanceId, out var instance))
        {
            instance.Stop();
            return true;
        }
        return false;
    }
}