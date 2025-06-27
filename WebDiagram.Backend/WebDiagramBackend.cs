using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Renderer.Contract;

public class WebDiagramBackend
{
    private readonly Dictionary<string, IRenderer> renderer = new Dictionary<string, IRenderer>();
    private readonly IHubContext<DiagramHub> hubContext;
    private readonly WebApplication webApplication;
    public WebDiagramBackend(WebApplication webApplication)
    {
        this.webApplication = webApplication;
        hubContext = webApplication.Services.GetRequiredService<IHubContext<DiagramHub>>();
    }

    public void AddRenderer(string instanceId, Func<Action<byte[]>, IRenderer> renderFactory) => renderer.Add(instanceId, renderFactory(image => hubContext.Clients.Group(instanceId).SendAsync("ReceiveImage", Convert.ToBase64String(image)).GetAwaiter().GetResult()));

    public IRenderer GetRenderer(string instanceId) => renderer[instanceId];

    public void Start()
    {
        webApplication.MapHub<DiagramHub>("/diagramhub");
        webApplication.MapGet("/{instanceId}/updateSize", (
            string instanceId,
            [FromQuery] int width,
            [FromQuery] int height) =>
        {
            try
            {
                renderer[instanceId].UpdateSize(width, height);
                return Results.Ok();
            }
            catch
            {
                return Results.Problem();
            }
        });

        webApplication.MapGet("/{instanceId}/updateViewPort", (
            string instanceId,
            [FromQuery] float xMin,
            [FromQuery] float xMax,
            [FromQuery] float yMin,
            [FromQuery] float yMax) =>
        {
            try
            {
                renderer[instanceId].UpdateViewport(xMin, xMax, yMin, yMax);
                return Results.Ok();
            }
            catch
            {
                return Results.Problem();
            }
        });

        webApplication.MapGet("/{instanceId}/updateHover", (
            string instanceId,
            [FromQuery] float x,
            [FromQuery] float y) =>
        {
            try
            {
                renderer[instanceId].UpdateHover(x, y);
                return Results.Ok();
            }
            catch
            {
                return Results.Problem();
            }
        });

        webApplication.MapGet("/{instanceId}/config", (
            string instanceId) =>
        {
            var margin = new
            {
                top = renderer[instanceId].Margin.Top,
                bottom = renderer[instanceId].Margin.Bottom,
                left = renderer[instanceId].Margin.Left,
                right = renderer[instanceId].Margin.Right
            };

            return Results.Json(new { margin });
        });

        foreach (var renderer in renderer)
            renderer.Value.Start();

        webApplication.Lifetime.ApplicationStopping.Register(() =>
        {
            foreach(var renderer in renderer)
	        renderer.Value.Stop();
        });
        
    }

}