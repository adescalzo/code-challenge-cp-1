using System.Globalization;

namespace EmployeeChallenge.Infrastructure.Extensions;

public static class DateTimeExtensions
{
    private const string MinDate = "0001-01-01";

    public static bool IsDefault(this DateTime dateTime)
    {
        return dateTime == DateTime.MinValue ||
               dateTime.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) == MinDate ||
               dateTime == default;
    }
}
