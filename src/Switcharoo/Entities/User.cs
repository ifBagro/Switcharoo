using Microsoft.AspNetCore.Identity;

namespace Switcharoo.Entities;

public sealed class User : IdentityUser
{
    public List<Environment> Environments { get; set; } = new();
    
    public List<Feature> Features { get; set; } = new();
}