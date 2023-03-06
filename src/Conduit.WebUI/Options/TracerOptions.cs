namespace Conduit.WebUI.Options;

public class TracerOptions
{
    public required string Host { get; set; }
    public int Port { get; set; }
    public bool Enabled { get; set; }
}