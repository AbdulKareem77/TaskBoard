using System.Reflection;
using Microsoft.Data.SqlClient;

namespace TaskBoard.Infrastructure.Repositories;

public abstract class SqlRepositoryBase
{
    private readonly string _connectionString;

    protected SqlRepositoryBase(string connectionString)
    {
        _connectionString = connectionString;
    }

    protected SqlConnection CreateConnection() => new(_connectionString);

    protected string LoadSql(string fileName)
    {
        var assembly = Assembly.GetExecutingAssembly();
        var resourceName = $"TaskBoard.Infrastructure.Sql.{fileName}";
        using var stream = assembly.GetManifestResourceStream(resourceName)
            ?? throw new InvalidOperationException($"Embedded SQL resource '{resourceName}' not found.");
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }
}
