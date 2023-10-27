namespace Switcharoo.Entities;

public sealed class FeatureEnvironment
{
    public Guid Id { get; set; }
    
    public Environment Environment { get; set; } = null!;
    
    public bool IsEnabled { get; set; }
}
