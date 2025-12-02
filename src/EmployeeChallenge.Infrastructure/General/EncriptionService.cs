namespace EmployeeChallenge.Infrastructure.General;

public interface IEncryptionService
{
    string PasswordEncrypt(string password);

    bool VerifyPassword(string enteredPassword, string storedHash);
}

public class EncryptionService : IEncryptionService
{
    public string PasswordEncrypt(string password) =>
        BCrypt.Net.BCrypt.HashPassword(password, workFactor: 12);

    public bool VerifyPassword(string enteredPassword, string storedHash) =>
        BCrypt.Net.BCrypt.Verify(enteredPassword, storedHash);
}
