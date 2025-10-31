namespace EmployeeChallenge.Infrastructure.Extensions;

public static class ExtensionMethods
{
    public static int ToInt32(this object? o, int defaultValue = 0)
    {
        if (o is null)
        {
            return defaultValue;
        }

        return int.TryParse(o.ToString(), out var intValue) ? intValue : defaultValue;
    }


    public static T? As<T>(this object o) where T : class => o as T;

    public static string ToString(this object? o, string defaultValue)
    {
        return o?.ToString() ?? defaultValue;
    }
}
