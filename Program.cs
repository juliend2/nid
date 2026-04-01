using Microsoft.AspNetCore.Http.Features;

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

app.Run("http://0.0.0.0:5000");
