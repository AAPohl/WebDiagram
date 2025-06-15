using Microsoft.AspNetCore.Mvc;
using SkiaSharp;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddCors();
var app = builder.Build();

app.UseCors(p => p.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());

app.MapGet("/render", ([FromQuery] float xMin, [FromQuery] float xMax, [FromQuery] float yMin, [FromQuery] float yMax, [FromQuery] int width, [FromQuery] int height) =>
{
    var imgBytes = Renderer.RenderCameraView(xMin, xMax, yMin, yMax, width, height);
    return Results.File(imgBytes, "image/png");
});
app.MapGet("/config", () =>
{
    var margin = new {
        top = 40,
        bottom = 40,
        left = 40,
        right = 40
    };

    return Results.Json(new { margin });
});

app.Run();
