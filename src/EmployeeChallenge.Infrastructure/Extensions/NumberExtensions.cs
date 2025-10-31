namespace EmployeeChallenge.Infrastructure.Extensions;

public static class DoubleExtensions
{
    public static bool IsCloseTo(this double value, double other, double epsilon = 0.0001)
        => Math.Abs(value - other) < epsilon;

    public static int ToInt(this decimal value)
        => (int)Math.Round(value, MidpointRounding.AwayFromZero);
}
