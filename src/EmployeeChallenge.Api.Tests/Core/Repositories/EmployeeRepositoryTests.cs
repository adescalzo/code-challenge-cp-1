using EmployeeChallenge.Api.Core.Repositories;
using EmployeeChallenge.Api.Tests.Builders;
using EmployeeChallenge.Api.Tests.IntegrationTests;
using FluentAssertions;
using Xunit.Abstractions;

namespace EmployeeChallenge.Api.Tests.Core.Repositories;

public class EmployeeRepositoryTests(ITestOutputHelper output) : AsyncLifetimeBase(output)
{
    private EmployeeRepository _repository = null!;

    protected override async Task OnInitializeAsync()
    {
        await Context.Database.EnsureCreatedAsync().ConfigureAwait(false);
        _repository = new EmployeeRepository(CreateUnitOfWork());
    }

    protected override async Task OnDisposeAsync()
    {
        await Context.Database.EnsureDeletedAsync().ConfigureAwait(false);
    }

    [Fact]
    public async Task GetByIdWithSupervisorAsync_WhenEmployeeHasSupervisor_ShouldReturnEmployeeWithSupervisor()
    {
        // Arrange
        var supervisor = new EmployeeBuilder()
            .AsSupervisor()
            .WithSupervisor(null)
            .Build();

        var employee = new EmployeeBuilder()
            .AsEmployee()
            .WithSupervisor(supervisor.Id)
            .Build();

        await _repository.Add(supervisor);
        await _repository.Add(employee);
        await SaveChangesAsync();

        // Act
        var result = await _repository.GetByIdWithSupervisorAsync(employee.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(employee.Id);
        result.Supervisor.Should().NotBeNull();
        result.Supervisor!.Id.Should().Be(supervisor.Id);
    }

    [Fact]
    public async Task GetByIdWithSupervisorAsync_WhenEmployeeHasNoSupervisor_ShouldReturnEmployeeWithNullSupervisor()
    {
        // Arrange
        var employee = new EmployeeBuilder()
            .WithSupervisor(null)
            .Build();

        await _repository.Add(employee);
        await SaveChangesAsync();

        // Act
        var result = await _repository.GetByIdWithSupervisorAsync(employee.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(employee.Id);
        result.Supervisor.Should().BeNull();
    }

    [Fact]
    public async Task GetByIdWithSupervisorAsync_WhenEmployeeDoesNotExist_ShouldReturnNull()
    {
        // Act
        var result = await _repository.GetByIdWithSupervisorAsync(Guid.NewGuid());

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetDirectReportsAsync_WhenSupervisorHasDirectReports_ShouldReturnAllDirectReports()
    {
        // Arrange
        var supervisor = new EmployeeBuilder()
            .AsSupervisor()
            .WithSupervisor(null)
            .Build();

        var directReports = new EmployeeBuilder()
            .WithSupervisor(supervisor.Id)
            .AsEmployee()
            .BuildList(3)
            .ToList();

        await _repository.Add(supervisor);
        foreach (var report in directReports)
        {
            await _repository.Add(report);
        }
        await SaveChangesAsync();

        // Act
        var result = await _repository.GetDirectReportsAsync(supervisor.Id);

        // Assert
        result.Should().HaveCount(3);
        result.Should().OnlyContain(e => e.SupervisorId == supervisor.Id);
    }

    [Fact]
    public async Task GetDirectReportsAsync_WhenSupervisorHasNoReports_ShouldReturnEmptyList()
    {
        // Arrange
        var supervisor = new EmployeeBuilder()
            .AsSupervisor()
            .WithSupervisor(null)
            .Build();

        await _repository.Add(supervisor);
        await SaveChangesAsync();

        // Act
        var result = await _repository.GetDirectReportsAsync(supervisor.Id);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetTotalReportsCountAsync_WithNestedHierarchy_ShouldReturnTotalCount()
    {
        // Arrange
        var ceo = new EmployeeBuilder().AsSupervisor().WithSupervisor(null).Build();
        var manager1 = new EmployeeBuilder().AsSupervisor().WithSupervisor(ceo.Id).Build();
        var manager2 = new EmployeeBuilder().AsSupervisor().WithSupervisor(ceo.Id).Build();
        var employee1 = new EmployeeBuilder().AsEmployee().WithSupervisor(manager1.Id).Build();
        var employee2 = new EmployeeBuilder().AsEmployee().WithSupervisor(manager1.Id).Build();
        var employee3 = new EmployeeBuilder().AsEmployee().WithSupervisor(manager2.Id).Build();

        await _repository.Add(ceo);
        await _repository.Add(manager1);
        await _repository.Add(manager2);
        await _repository.Add(employee1);
        await _repository.Add(employee2);
        await _repository.Add(employee3);
        await SaveChangesAsync();

        // Act
        var result = await _repository.GetTotalReportsCountAsync(ceo.Id);

        // Assert
        result.Should().Be(5); // 2 managers + 3 employees
    }

    [Fact]
    public async Task GetPaginatedWithSupervisorAsync_ShouldReturnCorrectPage()
    {
        // Arrange
        var supervisor = new EmployeeBuilder().AsSupervisor().WithSupervisor(null).Build();
        var employees = new EmployeeBuilder()
            .WithSupervisor(supervisor.Id)
            .BuildList(10)
            .ToList();

        await _repository.Add(supervisor);
        foreach (var emp in employees)
        {
            await _repository.Add(emp);
        }
        await SaveChangesAsync();

        // Act
        var result = await _repository.GetPaginatedWithSupervisorAsync(page: 1, pageSize: 5);

        // Assert
        result.Should().HaveCount(5);
    }
}
