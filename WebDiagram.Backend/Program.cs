using Microsoft.AspNetCore.Mvc;
using SkiaSharp;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddCors();
var app = builder.Build();

app.UseCors(p => p.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());

app.MapGet("/render", ([FromQuery] float x, [FromQuery] float y, [FromQuery] float z) =>
{
    int width = 400;
    int height = 300;

    using var bitmap = new SKBitmap(width, height);
    using var canvas = new SKCanvas(bitmap);
    canvas.Clear(SKColors.Black);

    using var paint = new SKPaint { Color = SKColors.White, IsAntialias = true };
    using var font = new SKFont
    {
        Size = 24,
        Edging = SKFontEdging.Antialias,
        Subpixel = true
    };

    var text = $"Camera @ X:{x:0.0} Y:{y:0.0} Z:{z:0.0}";
    canvas.DrawText(text, 20, 50, SKTextAlign.Left, font, paint);

    using var image = SKImage.FromBitmap(bitmap);
    using var data = image.Encode(SKEncodedImageFormat.Png, 100);

    return Results.File(data.AsSpan().ToArray(), "image/png");
});

app.Run();
