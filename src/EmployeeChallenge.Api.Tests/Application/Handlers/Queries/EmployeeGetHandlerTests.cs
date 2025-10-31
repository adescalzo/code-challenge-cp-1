using EmployeeChallenge.Api.Application.Handlers.Queries;
using EmployeeChallenge.Api.Core.Entities;
using EmployeeChallenge.Api.Core.Repositories;
using EmployeeChallenge.Api.Presentation.Employees.Queries;
using EmployeeChallenge.Infrastructure;
using FluentAssertions;
using NSubstitute;

namespace EmployeeChallenge.Api.Tests.Application.Handlers.Queries;

public class EmployeeGetHandlerTests
{
    private readonly IEmployeeRepository _repository;
    private readonly EmployeeGetHandler _sut;

    public EmployeeGetHandlerTests()
    {
        _repository = Substitute.For<IEmployeeRepository>();
        _sut = new EmployeeGetHandler(_repository);
    }

    [Fact]
    public async Task Handle_WhenEmployeeExists_ShouldReturnSuccessWithEmployee()
    {
        // Arrange
        var employeeId = Guid.NewGuid();
        var supervisorId = Guid.NewGuid();
        var supervisor = new Employee(supervisorId, "John", "Manager", "john@test.com", true, null);
        var employee = new Employee(employeeId, "Jane", "Doe", "jane@test.com", false, supervisorId)
        {
            Supervisor = supervisor
        };

        _repository.GetByIdWithSupervisorAsync(employeeId, Arg.Any<CancellationToken>())
            .Returns(employee);
        _repository.GetTotalReportsCountAsync(employeeId, Arg.Any<CancellationToken>())
            .Returns(0);

        var query = new EmployeeGetQuery(employeeId);

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Id.Should().Be(employeeId);
        result.Value.FirstName.Should().Be("Jane");
        result.Value.LastName.Should().Be("Doe");
        result.Value.Email.Should().Be("jane@test.com");
        result.Value.SupervisorId.Should().Be(supervisorId);
        result.Value.SupervisorName.Should().Be("John Manager");
        result.Value.TotalReportsCount.Should().Be(0);
    }

    [Fact]
    public async Task Handle_WhenEmployeeIsSupervisor_ShouldIncludeTotalReportsCount()
    {
        // Arrange
        var employeeId = Guid.NewGuid();
        var employee = new Employee(employeeId, "John", "Manager", "john@test.com", true, null);

        _repository.GetByIdWithSupervisorAsync(employeeId, Arg.Any<CancellationToken>())
            .Returns(employee);
        _repository.GetTotalReportsCountAsync(employeeId, Arg.Any<CancellationToken>())
            .Returns(5);

        var query = new EmployeeGetQuery(employeeId);

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.TotalReportsCount.Should().Be(5);
        await _repository.Received(1).GetTotalReportsCountAsync(employeeId, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenEmployeeIsNotSupervisor_ShouldNotCallGetTotalReportsCount()
    {
        // Arrange
        var employeeId = Guid.NewGuid();
        var employee = new Employee(employeeId, "Jane", "Doe", "jane@test.com", false, null);

        _repository.GetByIdWithSupervisorAsync(employeeId, Arg.Any<CancellationToken>())
            .Returns(employee);

        var query = new EmployeeGetQuery(employeeId);

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.TotalReportsCount.Should().Be(0);
        await _repository.DidNotReceive().GetTotalReportsCountAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenEmployeeHasNoSupervisor_ShouldReturnNullSupervisorInfo()
    {
        // Arrange
        var employeeId = Guid.NewGuid();
        var employee = new Employee(employeeId, "Jane", "Doe", "jane@test.com", false, null);

        _repository.GetByIdWithSupervisorAsync(employeeId, Arg.Any<CancellationToken>()).Returns(employee);

        var query = new EmployeeGetQuery(employeeId);

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.SupervisorId.Should().BeNull();
        result.Value.SupervisorName.Should().BeNull();
    }

    [Fact]
    public async Task Handle_WhenEmployeeDoesNotExist_ShouldReturnNotFoundFailure()
    {
        // Arrange
        var employeeId = Guid.NewGuid();
        _repository.GetByIdWithSupervisorAsync(employeeId, Arg.Any<CancellationToken>()).Returns((Employee?)null);

        var query = new EmployeeGetQuery(employeeId);

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be($"{ErrorDefinition.NotFound}");
        result.Error.Description.Should().Contain(employeeId.ToString());
    }
}
