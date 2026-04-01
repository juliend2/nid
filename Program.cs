using Microsoft.AspNetCore.Http.Features;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Diagnostics;
using System.Text.Json;

int Port = 5000;

var builder = WebApplication.CreateBuilder(args);
builder.WebHost.ConfigureKestrel(options => 
{
  options.Limits.MaxRequestBodySize = 500 * 1024 * 1024; // 500 MB
});

var app = builder.Build();

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
  return Results.Ok($"File '{file.FileName}' saved to {filePath}!");
}).DisableAntiforgery();


static string GetTailscaleHostname()
{
    try
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = "tailscale",
            Arguments = "status --json",
            RedirectStandardOutput = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var process = Process.Start(startInfo);
        using var reader = process.StandardOutput;
        string json = reader.ReadToEnd();
        
        // Parse the "Self" node which contains your machine's info
        using var doc = JsonDocument.Parse(json);
        var dnsName = doc.RootElement.GetProperty("Self").GetProperty("DNSName").GetString();

        // Tailscale returns names with a trailing dot (e.g., machine.net.), so we trim it
        return dnsName?.TrimEnd('.') ?? "localhost";
    }
    catch
    {
        return "localhost"; // Fallback if Tailscale isn't running
    }
}

var tailscaleName = GetTailscaleHostname();
if (tailscaleName == "localhost") 
{
  Console.WriteLine("Could not find a tailscale name");
}

Console.WriteLine($"Try connecting on: http://{tailscaleName}:{Port}");
app.Run($"http://0.0.0.0:{Port}");
