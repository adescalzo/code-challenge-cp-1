using Bogus;
using EmployeeChallenge.Api.Core.Entities;
using EmployeeChallenge.Infrastructure.Testing;

namespace EmployeeChallenge.Api.Tests.Builders;

internal class EmployeeBuilder : IBuilder<Employee>
{
    private Guid? _id;
    private string? _firstName;
    private string? _lastName;
    private string? _email;
    private bool? _isSupervisor;
    private Guid? _supervisorId;
    private bool _supervisorIdSet;

    public EmployeeBuilder WithId(Guid id)
    {
        _id = id;
        return this;
    }

    public EmployeeBuilder WithName(string firstName, string lastName)
    {
        _firstName = firstName;
        _lastName = lastName;
        return this;
    }

    public EmployeeBuilder WithEmail(string email)
    {
        _email = email;
        return this;
    }

    public EmployeeBuilder WithSupervisor(Guid? supervisorId)
    {
        _supervisorId = supervisorId;
        _supervisorIdSet = true;
        return this;
    }

    public EmployeeBuilder AsSupervisor()
    {
        _isSupervisor = true;
        if (!_supervisorIdSet)
        {
            _supervisorId = null;
            _supervisorIdSet = true;
        }
        return this;
    }

    public EmployeeBuilder AsEmployee()
    {
        _isSupervisor = false;
        return this;
    }

    public Employee Build()
    {
        var faker = new Faker();
        var supervisorId = GetSupervisorId(faker);
        return new Employee(
            _id ?? Guid.NewGuid(),
            _firstName ?? faker.Name.FirstName(),
            _lastName ?? faker.Name.LastName(),
            _email ?? faker.Internet.Email(),
            _isSupervisor ?? faker.Random.Bool(),
            supervisorId
        );
    }

    public IEnumerable<Employee> BuildList(int count)
    {
        var employees = new List<Employee>();
        for (var i = 0; i < count; i++)
        {
            var faker = new Faker();
            var supervisorId = GetSupervisorId(faker);
            employees.Add(new Employee(
                _id ?? Guid.NewGuid(),
                _firstName ?? faker.Name.FirstName(),
                _lastName ?? faker.Name.LastName(),
                _email ?? faker.Internet.Email(),
                _isSupervisor ?? faker.Random.Bool(),
                supervisorId
            ));
        }
        return employees;
    }

    private Guid? GetSupervisorId(Faker faker)
    {
        if (_supervisorIdSet)
        {
            return _supervisorId;
        }

        return faker.Random.Bool(0.3f) ? Guid.NewGuid() : null;
    }
}
