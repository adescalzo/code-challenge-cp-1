namespace EmployeeChallenge.Api.Presentation.Employees;

internal record EmployeeResponse(
    Guid Id,
    string FirstName,
    string LastName,
    string Email,
    Guid? SupervisorId,
    string? SupervisorName,
    int TotalReportsCount,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);
