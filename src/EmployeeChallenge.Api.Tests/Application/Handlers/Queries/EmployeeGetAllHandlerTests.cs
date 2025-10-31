using EmployeeChallenge.Api.Application.Handlers.Queries;
using EmployeeChallenge.Api.Core.Entities;
using EmployeeChallenge.Api.Core.Repositories;
using EmployeeChallenge.Api.Presentation;
using EmployeeChallenge.Api.Presentation.Employees.Queries;
using FluentAssertions;
using NSubstitute;

namespace EmployeeChallenge.Api.Tests.Application.Handlers.Queries;

public class EmployeeGetAllHandlerTests
{
    private readonly IEmployeeRepository _repository;
    private readonly EmployeeGetAllHandler _sut;

    public EmployeeGetAllHandlerTests()
    {
        _repository = Substitute.For<IEmployeeRepository>();
        _sut = new EmployeeGetAllHandler(_repository);
    }

    [Fact]
    public async Task Handle_ShouldReturnPaginatedEmployees()
    {
        // Arrange
        var supervisor = new Employee(Guid.NewGuid(), "John", "Manager", "john@test.com", true, null);
        var employees = new List<Employee>
        {
            new(Guid.NewGuid(), "Jane", "Doe", "jane@test.com", false, supervisor.Id) { Supervisor = supervisor },
            new(Guid.NewGuid(), "Bob", "Smith", "bob@test.com", false, supervisor.Id) { Supervisor = supervisor },
            new(Guid.NewGuid(), "Alice", "Johnson", "alice@test.com", false, null)
        };

        var payload = new PaginationPayload(Page: 1, PageSize: 10);
        _repository.GetPaginatedWithSupervisorAsync(1, 10, Arg.Any<CancellationToken>()).Returns(employees);

        var query = new EmployeeGetAllQuery(payload);

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(3);
    }

    [Fact]
    public async Task Handle_ShouldMapEmployeesCorrectly()
    {
        // Arrange
        var supervisorId = Guid.NewGuid();
        var supervisor = new Employee(supervisorId, "John", "Manager", "john@test.com", true, null);
        var employeeId = Guid.NewGuid();
        var employee = new Employee(employeeId, "Jane", "Doe", "jane@test.com", false, supervisorId)
        {
            Supervisor = supervisor
        };

        var employees = new List<Employee> { employee };
        var payload = new PaginationPayload(Page: 1, PageSize: 10);

        _repository.GetPaginatedWithSupervisorAsync(1, 10, Arg.Any<CancellationToken>()).Returns(employees);

        var query = new EmployeeGetAllQuery(payload);

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Should().HaveCount(employees.Count);
        var response = result.Value!.First();
        response.Id.Should().Be(employeeId);
        response.FirstName.Should().Be("Jane");
        response.LastName.Should().Be("Doe");
        response.Email.Should().Be("jane@test.com");
        response.SupervisorId.Should().Be(supervisorId);
        response.SupervisorName.Should().Be("John Manager");
        response.TotalReportsCount.Should().Be(0);
    }

    [Fact]
    public async Task Handle_WhenEmployeeHasNoSupervisor_ShouldMapNullSupervisorFields()
    {
        // Arrange
        var employee = new Employee(Guid.NewGuid(), "Jane", "Doe", "jane@test.com", false, null);
        var employees = new List<Employee> { employee };
        var payload = new PaginationPayload(Page: 1, PageSize: 10);

        _repository.GetPaginatedWithSupervisorAsync(1, 10, Arg.Any<CancellationToken>()).Returns(employees);

        var query = new EmployeeGetAllQuery(payload);

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Should().HaveCount(employees.Count);
        var response = result.Value!.First();
        response.SupervisorId.Should().BeNull();
        response.SupervisorName.Should().BeNull();
    }

    [Fact]
    public async Task Handle_WhenNoEmployeesExist_ShouldReturnEmptyList()
    {
        // Arrange
        var payload = new PaginationPayload(Page: 1, PageSize: 10);
        _repository.GetPaginatedWithSupervisorAsync(1, 10, Arg.Any<CancellationToken>()).Returns([]);

        var query = new EmployeeGetAllQuery(payload);

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_ShouldUseCorrectPaginationParameters()
    {
        // Arrange
        var payload = new PaginationPayload(Page: 2, PageSize: 25);
        _repository.GetPaginatedWithSupervisorAsync(2, 25, Arg.Any<CancellationToken>()).Returns([]);

        var query = new EmployeeGetAllQuery(payload);

        // Act
        await _sut.Handle(query, CancellationToken.None);

        // Assert
        await _repository.Received(1).GetPaginatedWithSupervisorAsync(2, 25, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldPassCancellationTokenToRepository()
    {
        // Arrange
        var payload = new PaginationPayload(Page: 1, PageSize: 10);
        var cancellationToken = new CancellationToken(true);
        _repository.GetPaginatedWithSupervisorAsync(1, 10, cancellationToken).Returns([]);

        var query = new EmployeeGetAllQuery(payload);

        // Act
        await _sut.Handle(query, cancellationToken);

        // Assert
        await _repository.Received(1).GetPaginatedWithSupervisorAsync(1, 10, cancellationToken);
    }
}
