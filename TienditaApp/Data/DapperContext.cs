using System.Data;
using Microsoft.Data.Sqlite;

namespace TienditaApp.Data;

public class DapperContext
{
    private readonly string _connectionString;

    public DapperContext(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new Exception("Connection string not found");
    }

    public IDbConnection CreateConnection()
        => new SqliteConnection(_connectionString);
}