namespace ZapretDesktop.Core.Models;

public class Preset
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = "General";
    public string Arguments { get; set; } = string.Empty;
    public bool IsBuiltIn { get; set; }
}