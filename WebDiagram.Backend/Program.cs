using Microsoft.AspNetCore.Mvc;
using SkiaSharp;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddCors();
var app = builder.Build();

app.UseCors(p => p.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());

app.MapGet("/render", ([FromQuery] float xMin, [FromQuery] float xMax, [FromQuery] float yMin, [FromQuery] float yMax) =>
{
    var imgBytes = Renderer.RenderCameraView(xMin, xMax, yMin, yMax);
    return Results.File(imgBytes, "image/png");
});

app.Run();
