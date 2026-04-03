using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace TaskBoard.Infrastructure;

public interface IDbConnectionFactory
{
    SqlConnection CreateConnection();
}

public class SqlConnectionFactory : IDbConnectionFactory
{
    private readonly string _connectionString;

    public SqlConnectionFactory(IConfiguration config)
    {
        _connectionString = config.GetConnectionString("TaskBoard")
            ?? throw new InvalidOperationException("TaskBoard connection string is required.");
    }

    public SqlConnection CreateConnection() => new(_connectionString);
}
