namespace EmployeeChallenge.Infrastructure;

public interface IClock
{
    DateTime UtcNow { get; }
    DateTime Today { get; }
}

public class Clock : IClock
{
    public DateTime UtcNow => DateTime.UtcNow;

    public DateTime Today => DateTime.Today;
}
