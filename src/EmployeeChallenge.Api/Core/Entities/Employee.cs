using EmployeeChallenge.Infrastructure.Data;

namespace EmployeeChallenge.Api.Core.Entities;

internal class Employee : Entity
{
    private Employee()
    {
    }

    public Employee(Guid id, string firstName, string lastName, string email, bool isSupervisor, Guid? supervisorId)
    {
        Id = id;
        FirstName = firstName;
        LastName = lastName;
        Email = email;
        IsSupervisor = isSupervisor;
        SupervisorId = supervisorId;
    }

    public string FirstName { get; private set; } = null!;
    public string LastName { get; private set; } = null!;
    public string Email { get; private set; } = null!;
    public bool IsSupervisor { get; private set; }
    public Guid? SupervisorId { get; private set; }
    public Employee? Supervisor { get; set; }
    public ICollection<Employee> DirectReports { get; private set; } = [];

    public void Update(string firstName, string lastName, string email, bool isSupervisor, Guid? supervisorId)
    {
        FirstName = firstName;
        LastName = lastName;
        Email = email;
        IsSupervisor = isSupervisor;
        SupervisorId = supervisorId;
    }
}

