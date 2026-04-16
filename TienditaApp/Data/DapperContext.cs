using Microsoft.Data.Sqlite;
using System.Data;

namespace TienditaApp.Data
{
    public class DapperContext
    {
        private readonly string _connectionString;

        public DapperContext()
        {
            _connectionString = "Data Source=tienda.db";
        }

        public IDbConnection CreateConnection()
            => new SqliteConnection(_connectionString);
    }
}