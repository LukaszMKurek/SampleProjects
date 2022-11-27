using Dapper;
using Microsoft.Data.SqlClient;

namespace LockingInSqlServer.Utils;

public static class ConnectionHelper
{
    private const string ConnectionString = "Data Source=.;Initial Catalog=LockingInSqlServerDatabase;Integrated Security=True;Encrypt=false;max pool size=2000;Connection Timeout=60";

    public static async Task<SqlConnection> GetSqlConnection()
    {
        var connection = new SqlConnection(ConnectionString);
        await connection.OpenAsync();
        return connection;
    }

    public static async Task Execute(string sql, object parameters, int commandTimeout)
    {
        await using var connection = await GetSqlConnection();
        await connection.ExecuteAsync(sql, parameters, commandTimeout: commandTimeout);
    }
}