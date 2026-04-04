using Nid.Services;
using RazorLight;

int Port = 5000;
static void HorizontalLine() => Console.WriteLine("-------------------------------------------------------------");

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSingleton<TailscaleService>();
builder.Services.AddSingleton<QrCodeService>();

builder.WebHost.ConfigureKestrel(options => 
{
  options.Limits.MaxRequestBodySize = 500 * 1024 * 1024; // 500 MB
});

var app = builder.Build();

var tailscale = app.Services.GetRequiredService<TailscaleService>();
var qrService = app.Services.GetRequiredService<QrCodeService>();

var hostname = await tailscale.GetTailnetHostnameAsync();
string accessUrl = $"http://{hostname}:{Port}";

HorizontalLine();
if (!string.IsNullOrEmpty(hostname)) {
  Console.WriteLine($"🌐 Tailnet: {accessUrl}");
} else {
  Console.WriteLine("📍 Tailscale not found. Use local IP instead.");
}

qrService.PrintQrToConsole(accessUrl);
HorizontalLine();

app.MapGet("/", async () => {
  string rendered = await Templates.GetRenderedPage("./templates/upload.cshtml", "");
  return Results.Content(rendered, "text/html");
});

app.MapPost("/upload", async (HttpRequest request) =>
{
  var form = await request.ReadFormAsync();
  string? text = form["text"];
  var message = "";
  var file = form.Files.GetFile("file");
  if (file == null)
  {
    File.WriteAllText(Path.Combine(Directory.GetCurrentDirectory(), "uploads", "text-file.txt"), text);
    message = "Text saved";
  }
  else
  {
    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "uploads", file.FileName);
    using var stream = new FileStream(filePath, FileMode.Create);
    await file.CopyToAsync(stream);
    message = $"File '{file.FileName}' saved to {filePath}!";
  }
  string rendered = await Templates.GetRenderedPage("./templates/uploaded.cshtml", message);
  return Results.Content(rendered, "text/html");
}).DisableAntiforgery();

app.Run($"http://0.0.0.0:{Port}");

public static class Templates {
  public static async Task<string> GetRenderedPage(string templatePath, string message)
  {
    var engine = new RazorLightEngineBuilder()
      .UseEmbeddedResourcesProject(typeof(Program))
      .UseMemoryCachingProvider()
      .Build();
    string template = File.ReadAllText(templatePath);
    var model = new {
      Message = message
    };
    string result = await engine.CompileRenderStringAsync(templatePath, template, model);
    return result;
  }
}
