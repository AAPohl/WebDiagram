using Microsoft.AspNetCore.Mvc;
using SkiaSharp;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddCors();
var app = builder.Build();

app.UseCors(p => p.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());

app.MapGet("/render", ([FromQuery] float x, [FromQuery] float y, [FromQuery] float z) =>
{
    var pngBytes = Renderer.RenderCameraView(x, y, z);
    return Results.File(pngBytes, "image/png");
});

app.Run();
