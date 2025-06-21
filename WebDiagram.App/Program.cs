using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(new WebApplicationOptions
{
    ContentRootPath = AppContext.BaseDirectory,
    WebRootPath = Path.Combine(AppContext.BaseDirectory, "wwwroot")
});

var app = builder.Build();

app.UseStaticFiles();

app.MapGet("/", context =>
{
    context.Response.Redirect("/index.html");
    return Task.CompletedTask;
});

var renderer = new SkiaSharpRenderer();
var wrappedRenderer = new StaRenderDispatcher(renderer);
app.MapGet("/render", async (
    [FromQuery] float xMin,
    [FromQuery] float xMax,
    [FromQuery] float yMin,
    [FromQuery] float yMax,
    [FromQuery] int width,
    [FromQuery] int height) =>
{
    try
    {
        var imgBytes = await wrappedRenderer.RenderAsync(xMin, xMax, yMin, yMax, width, height);
        if (imgBytes == null || imgBytes.Length == 0)
            return Results.NoContent();  // 204 No Content bei leerem Ergebnis

        return Results.File(imgBytes, "image/png");
    }
    catch
    {
        return Results.NoContent();  // 204 No Content bei Fehler statt 500
    }
});

// app.MapGet("/render", (
//     [FromQuery] float xMin,
//     [FromQuery] float xMax,
//     [FromQuery] float yMin,
//     [FromQuery] float yMax,
//     [FromQuery] int width,
//     [FromQuery] int height) =>
// {
//     var imgBytes = renderer.Render(xMin, xMax, yMin, yMax, width, height);
//     return Results.File(imgBytes, "image/png");
// });

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

app.Run();
