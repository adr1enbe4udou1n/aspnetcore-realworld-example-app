namespace Conduit.WebUI.Options;

public class TracerOptions
{
    public string Host { get; set; } = default!;
    public int Port { get; set; }
    public bool Enabled { get; set; }
}