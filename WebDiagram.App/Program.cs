using System.Collections.Concurrent;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Renderer.Contract;

var builder = WebApplication.CreateBuilder(new WebApplicationOptions
{
    ContentRootPath = AppContext.BaseDirectory,
    WebRootPath = Path.Combine(AppContext.BaseDirectory, "wwwroot")
});
builder.Services.AddSignalR();

var app = builder.Build();

app.UseStaticFiles();

app.MapGet("/", context =>
{
    context.Response.Redirect("/index.html");
    return Task.CompletedTask;
});

app.MapHub<DiagramHub>("/diagramhub");
var hubContext = app.Services.GetRequiredService<IHubContext<DiagramHub>>();

var renderers = new Dictionary<string, IRenderer>();
renderers.Add("diagram1", new SkiaSharpRenderer(24, image => hubContext.Clients.Group("diagram1").SendAsync("ReceiveImage", Convert.ToBase64String(image)).GetAwaiter().GetResult()));
renderers.Add("diagram2", new SkiaSharpRenderer(24, image => hubContext.Clients.Group("diagram2").SendAsync("ReceiveImage", Convert.ToBase64String(image)).GetAwaiter().GetResult()));

app.MapGet("/{instanceId}/updateSize", (
	string instanceId,
	[FromQuery] int width,
	[FromQuery] int height) =>
{
	try
	{
        renderers[instanceId].UpdateSize(width, height);
		return Results.Ok();
	}
	catch
	{
        return Results.Problem();
	}
});

app.MapGet("/{instanceId}/updateViewPort", (
	string instanceId,
    [FromQuery] float xMin,
	[FromQuery] float xMax,
	[FromQuery] float yMin,
	[FromQuery] float yMax) =>
{
	try
	{
		renderers[instanceId].UpdateViewport(xMin, xMax, yMin, yMax);
		return Results.Ok();
	}
	catch
	{
		return Results.Problem();
	}
});

app.MapGet("/{instanceId}/config", (
    string instanceId) =>
{
    var margin = new
    {
        top = renderers[instanceId].Margin.Top,
        bottom = renderers[instanceId].Margin.Bottom,
        left = renderers[instanceId].Margin.Left,
        right = renderers[instanceId].Margin.Right
    };

    return Results.Json(new { margin });
});

foreach(var renderer in renderers)
	renderer.Value.Start();

app.Lifetime.ApplicationStopping.Register(() =>
{
	foreach(var renderer in renderers)
	renderer.Value.Stop();
});

app.Run();
