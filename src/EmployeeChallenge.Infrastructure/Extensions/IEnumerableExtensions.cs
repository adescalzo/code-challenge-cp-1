using System.Data;

namespace EmployeeChallenge.Infrastructure.Extensions;

internal static class IEnumerableExtensions
{
    public static DataTable ToDataTable<T>(this IEnumerable<T> enumerable)
    {
        var dataTable = new DataTable(typeof(T).Name);
        var properties = typeof(T).GetProperties();

        foreach (var property in properties)
        {
            dataTable.Columns.Add(property.Name,
                Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType);
        }

        //Add data to the DataTable row by row
        foreach (var item in enumerable)
        {
            var row = dataTable.NewRow();
            foreach (var property in properties)
            {
                row[property.Name] = property.GetValue(item, null) ?? DBNull.Value;
            }

            dataTable.Rows.Add(row);
        }

        return dataTable;
    }
}
