using Bogus;
using EmployeeChallenge.Api.Application.Services;
using EmployeeChallenge.Api.Core.Entities;
using EmployeeChallenge.Infrastructure.General;
using EmployeeChallenge.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EmployeeChallenge.Api.Core;

internal sealed class ApplicationDbContextSeeder(
    ApplicationDbContext context,
    IAuthService authService,
    IClock clock,
    ILogger<ApplicationDbContextSeeder> logger) : IDbSeeder
{
    private readonly Faker _faker = new();

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await SeedUsersAsync(cancellationToken).ConfigureAwait(false);
            await SeedEmployeesAsync(cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while seeding the database");
            throw;
        }
    }

    private async Task SeedUsersAsync(CancellationToken cancellationToken)
    {
        // Check if users already exist
        if (await context.Users.AnyAsync(cancellationToken).ConfigureAwait(false))
        {
            logger.LogInformation("Users already exist, skipping seed");
            return;
        }

        logger.LogInformation("Seeding users...");

        var users = new[]
        {
            new User(
                id: Guid.CreateVersion7(),
                username: "admin",
                email: _faker.Internet.Email(),
                password: authService.HashPassword("Admin@123"),
                firstName: _faker.Name.FirstName(),
                lastName: _faker.Name.LastName()
            ),
            new User(
                id: Guid.CreateVersion7(),
                username: "manager",
                email: _faker.Internet.Email(),
                password: authService.HashPassword("Manager@123"),
                firstName: _faker.Name.FirstName(),
                lastName: _faker.Name.LastName()
            ),
            new User(
                id: Guid.CreateVersion7(),
                username: "employee",
                email: _faker.Internet.Email(),
                password: authService.HashPassword("Employee@123"),
                firstName: _faker.Name.FirstName(),
                lastName: _faker.Name.LastName()
            )
        };

        var when = clock.UtcNow;
        foreach (var user in users)
        {
            user.SetAuditableAdd(when);
        }

        await context.Users.AddRangeAsync(users, cancellationToken).ConfigureAwait(false);
        await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        logger.LogInformation("Seeded {Count} users successfully", users.Length);
    }

    private async Task SeedEmployeesAsync(CancellationToken cancellationToken)
    {
        if (await context.Employees.AnyAsync(cancellationToken).ConfigureAwait(false))
        {
            return;
        }

        logger.LogInformation("Seeding sample employees...");

        var when = clock.UtcNow;

        // Create CEO (no supervisor)
        var ceo = new Employee(Guid.CreateVersion7(), _faker.Name.FirstName(), _faker.Name.LastName(), _faker.Internet.Email(), true, null);
        ceo.SetAuditableAdd(when);
        await context.Employees.AddAsync(ceo, cancellationToken).ConfigureAwait(false);
        await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        // Create managers reporting to CEO
        var manager1 = new Employee(Guid.CreateVersion7(), _faker.Name.FirstName(), _faker.Name.LastName(), _faker.Internet.Email(), true, ceo.Id);
        var manager2 = new Employee(Guid.CreateVersion7(), _faker.Name.FirstName(), _faker.Name.LastName(), _faker.Internet.Email(), true, ceo.Id);
        manager1.SetAuditableAdd(when);
        manager2.SetAuditableAdd(when);

        await context.Employees.AddRangeAsync([manager1, manager2], cancellationToken).ConfigureAwait(false);
        await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        // Create employees reporting to managers
        var employees = new[]
        {
            new Employee(Guid.CreateVersion7(), _faker.Name.FirstName(), _faker.Name.LastName(), _faker.Internet.Email(), false, manager1.Id),
            new Employee(Guid.CreateVersion7(), _faker.Name.FirstName(), _faker.Name.LastName(), _faker.Internet.Email(), false, manager1.Id),
            new Employee(Guid.CreateVersion7(), _faker.Name.FirstName(), _faker.Name.LastName(), _faker.Internet.Email(), false, manager2.Id),
            new Employee(Guid.CreateVersion7(), _faker.Name.FirstName(), _faker.Name.LastName(), _faker.Internet.Email(), false, manager2.Id),
        };

        foreach (var employee in employees)
        {
            employee.SetAuditableAdd(when);
        }

        await context.Employees.AddRangeAsync(employees, cancellationToken).ConfigureAwait(false);
        await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        logger.LogInformation("Seeded {Count} employees successfully", 7);
    }
}
