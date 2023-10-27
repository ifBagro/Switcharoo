using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Switcharoo.Entities;
using Environment = Switcharoo.Entities.Environment;

namespace Switcharoo.Database;

public class SwitcharooContext : IdentityDbContext<User>
{
    public DbSet<Environment> Environments { get; set; } = null!;
    public DbSet<Feature> Features { get; set; } = null!;
    
    public SwitcharooContext(DbContextOptions<SwitcharooContext> options): base(options) { }
}
