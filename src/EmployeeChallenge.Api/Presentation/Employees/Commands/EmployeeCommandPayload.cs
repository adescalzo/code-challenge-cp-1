namespace EmployeeChallenge.Api.Presentation.Employees.Commands;

internal record EmployeeCommandPayload(
    string FirstName,
    string LastName,
    string Email,
    Guid? SupervisorId,
    bool IsSupervisor
);
