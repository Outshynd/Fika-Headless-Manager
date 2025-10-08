namespace FikaHeadlessManager.Models;

public record Settings
{
    public string? ProfileId { get; set; }
    public Uri? BackendUrl { get; set; }
}
