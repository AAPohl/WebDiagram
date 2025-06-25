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

var renderer = new SkiaSharpRenderer(24, image => hubContext.Clients.All.SendAsync("ReceiveImage", Convert.ToBase64String(image)).GetAwaiter().GetResult());
app.MapGet("/updateSize", (
	[FromQuery] int width,
	[FromQuery] int height) =>
{
	try
	{
        renderer.UpdateSize(width, height);
		return Results.Ok();
	}
	catch
	{
        return Results.Problem();
	}
});

app.MapGet("/updateViewPort", (
	[FromQuery] float xMin,
	[FromQuery] float xMax,
	[FromQuery] float yMin,
	[FromQuery] float yMax) =>
{
	try
	{
		renderer.UpdateViewport(xMin, xMax, yMin, yMax);
		return Results.Ok();
	}
	catch
	{
		return Results.Problem();
	}
});

app.MapGet("/config", () =>
{
    var margin = new
    {
        top = renderer.Margin.Top,
        bottom = renderer.Margin.Bottom,
        left = renderer.Margin.Left,
        right = renderer.Margin.Right
    };

    return Results.Json(new { margin });
});

renderer.Start();

app.Lifetime.ApplicationStopping.Register(() =>
{
	renderer.Stop();
});

app.Run();
