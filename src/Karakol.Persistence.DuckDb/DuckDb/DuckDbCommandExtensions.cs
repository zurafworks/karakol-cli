using System.Data.Common;

namespace Karakol.Persistence.DuckDb.DuckDb;

internal static class DuckDbCommandExtensions
{
    public static int ExecuteNonQuery(this DbConnection connection, string sql, params object?[] parameters)
    {
        using var command = connection.CreateCommand();
        command.CommandText = sql;
        foreach (var parameterValue in parameters)
        {
            var parameter = command.CreateParameter();
            parameter.Value = parameterValue ?? DBNull.Value;
            command.Parameters.Add(parameter);
        }

        return command.ExecuteNonQuery();
    }

    public static object? ExecuteScalar(this DbConnection connection, string sql, params object?[] parameters)
    {
        using var command = connection.CreateCommand();
        command.CommandText = sql;
        foreach (var parameterValue in parameters)
        {
            var parameter = command.CreateParameter();
            parameter.Value = parameterValue ?? DBNull.Value;
            command.Parameters.Add(parameter);
        }

        return command.ExecuteScalar();
    }
}
