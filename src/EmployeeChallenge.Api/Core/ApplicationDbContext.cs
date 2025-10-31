using EmployeeChallenge.Api.Core.Entities;
using EmployeeChallenge.Infrastructure;
using EmployeeChallenge.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EmployeeChallenge.Api.Core;

internal class ApplicationDbContext(
    IClock clock,
    DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{
    public DbSet<User> Users { get; set; }
    public DbSet<Employee> Employees { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }

    public override int SaveChanges()
    {
        UpdateTimestamps();
        return base.SaveChanges();
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        UpdateTimestamps();
        return await base.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    private void UpdateTimestamps()
    {
        var entries = ChangeTracker.Entries<Entity>();

        foreach (var entry in entries)
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.SetAuditableAdd(clock.UtcNow);
                    break;
                case EntityState.Modified:
                    entry.Entity.SetAuditableModified(clock.UtcNow);
                    break;
                case EntityState.Detached:
                case EntityState.Unchanged:
                case EntityState.Deleted:
                default:
                    break;
            }
        }
    }
}
