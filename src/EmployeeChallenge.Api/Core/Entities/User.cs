using EmployeeChallenge.Infrastructure.Data;

namespace EmployeeChallenge.Api.Core.Entities;

internal class User : Entity
{
    private User()
    {
    }

    public User(Guid id, string username, string email, string password, string firstName, string lastName)
    {
        Id = id;
        Username = username;
        Email = email;
        Password = password;
        FirstName = firstName;
        LastName = lastName;
    }

    public string Username { get; private set; } = null!;
    public string Email { get; private set; } = null!;
    public string Password { get; private set; } = null!;
    public string FirstName { get; private set; } = null!;
    public string LastName { get; private set; } = null!;
}
