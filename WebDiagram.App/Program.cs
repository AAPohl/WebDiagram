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



var webDiagramBackend = new WebDiagramBackend(app);
webDiagramBackend.AddRenderer("diagram1", renderAction => new SkiaSharpRenderer(24, renderAction));
webDiagramBackend.AddRenderer("diagram2", renderAction => new SkiaSharpRenderer(24, renderAction));
webDiagramBackend.Start();

app.Run();
