namespace EmployeeChallenge.Infrastructure.Data;

/// <summary>
/// Interface for database seeding operations
/// </summary>
public interface IDbSeeder
{
    /// <summary>
    /// Seeds initial data into the database
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    Task SeedAsync(CancellationToken cancellationToken = default);
}
