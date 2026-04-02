using Nid.Services;

int Port = 5000;
static void HorizontalLine() => Console.WriteLine("-------------------------------------------------------------");

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSingleton<TailscaleService>();
builder.WebHost.ConfigureKestrel(options => 
{
  options.Limits.MaxRequestBodySize = 500 * 1024 * 1024; // 500 MB
});

var app = builder.Build();

var tailscale = app.Services.GetRequiredService<TailscaleService>();
var hostname = await tailscale.GetTailnetHostnameAsync();


HorizontalLine();
if (!string.IsNullOrEmpty(hostname)) {
  Console.WriteLine($"🌐 Tailnet: http://{hostname}:{Port}");
} else {
  Console.WriteLine("📍 Tailscale not found. Use local IP instead.");
}
HorizontalLine();

app.MapGet("/", () => Results.Content(@"
  <!DOCTYPE html>
  <html>
    <head>
      <meta name='viewport' content='width=device-width, initial-scale=1'>
    </head>
    <body>
      <h1>Upload to PC</h1>
      <form action='/upload' method='post' enctype='multipart/form-data'>
        <input type='file' name='file' required />
        <button type='submit'>Upload</button>
      </form>
    </body>
  </html>
", "text/html"));

app.MapPost("/upload", async (IFormFile file) =>
{
  var filePath = Path.Combine(Directory.GetCurrentDirectory(), "uploads", file.FileName);
  using var stream = new FileStream(filePath, FileMode.Create);
  await file.CopyToAsync(stream);
  return Results.Content(@$"
  <!DOCTYPE html>
  <html>
    <head>
      <meta name='viewport' content='width=device-width, initial-scale=1'>
    </head>
    <body>
      <h1>Hurray</h1>
      <p>
        File '{file.FileName}' saved to {filePath}!
      </p>
      <p>
        <a href='/'>Go Back</a>
      </p>
    </body>
  </html>", "text/html");
}).DisableAntiforgery();

app.Run($"http://0.0.0.0:{Port}");
