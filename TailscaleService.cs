using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Nid.Services;

public class TailscaleService
{
  private readonly ILogger<TailscaleService> _logger;

  public TailscaleService(ILogger<TailscaleService> logger)
  {
    _logger = logger;
  }

  public async Task<string?> GetTailnetHostnameAsync()
  {
    try {
      var startInfo = new ProcessStartInfo
      {
          FileName = "tailscale",
          Arguments = "status --json",
          RedirectStandardOutput = true,
          UseShellExecute = false,
          CreateNoWindow = true
      };

      using var process = Process.Start(startInfo);
      if (process == null) return null;

      var status = await JsonSerializer.DeserializeAsync<TailscaleStatus>(
        process.StandardOutput.BaseStream
      );

      return status?.Self?.DNSName?.TrimEnd('.');
    }
    catch (Exception ex)
    {
      _logger.LogWarning("Tailscale not detected or not running: {Message}", ex.Message);
      return null;
    }
  }

  private class TailscaleStatus
  {
    [JsonPropertyName("Self")]
    public TailscalePeer? Self { get; set; }
  }

  private class TailscalePeer
  {
    [JsonPropertyName("DNSName")]
    public string? DNSName { get; set; }
  }
}
