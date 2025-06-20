using Microsoft.AspNetCore.Mvc;
using Renderer.Contract;
namespace WebDiagram.Backend;

public static class EndpointRegistrar
{
    public static void MapWebDiagramApi(this WebApplication app, IRenderer render)
    {
        app.MapGet("/render", (
            [FromQuery] float xMin,
            [FromQuery] float xMax,
            [FromQuery] float yMin,
            [FromQuery] float yMax,
            [FromQuery] int width,
            [FromQuery] int height) =>
        {
            var imgBytes = render.Render(xMin, xMax, yMin, yMax, width, height);
            return Results.File(imgBytes, "image/png");
        });

        app.MapGet("/config", () =>
        {
            var margin = new
            {
                top = render.Margin.Top,
                bottom = render.Margin.Bottom,
                left = render.Margin.Left,
                right = render.Margin.Right
            };

            return Results.Json(new { margin });
        });
    }
}
