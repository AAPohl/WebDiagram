using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

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

var diagramManager = new DiagramManager(hubContext);
diagramManager.AddRenderer("diagram1", renderAction => new SkiaSharpRenderer(24, renderAction));
diagramManager.AddRenderer("diagram2", renderAction => new SkiaSharpRenderer(24, renderAction));
app.MapGet("/{instanceId}/updateSize", (
	string instanceId,
	[FromQuery] int width,
	[FromQuery] int height) =>
{
	try
	{
        diagramManager.GetRenderer(instanceId).UpdateSize(width, height);
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
		diagramManager.GetRenderer(instanceId).UpdateViewport(xMin, xMax, yMin, yMax);
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
        top = diagramManager.GetRenderer(instanceId).Margin.Top,
        bottom = diagramManager.GetRenderer(instanceId).Margin.Bottom,
        left = diagramManager.GetRenderer(instanceId).Margin.Left,
        right = diagramManager.GetRenderer(instanceId).Margin.Right
    };

    return Results.Json(new { margin });
});

diagramManager.Start();

app.Lifetime.ApplicationStopping.Register(() =>
{
	diagramManager.Stop();
});

app.Run();
