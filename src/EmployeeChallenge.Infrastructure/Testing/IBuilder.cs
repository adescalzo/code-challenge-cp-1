namespace EmployeeChallenge.Infrastructure.Testing;

public interface IBuilder<out T> where T : class
{
    T Build();
    IEnumerable<T> BuildList(int count);
}
