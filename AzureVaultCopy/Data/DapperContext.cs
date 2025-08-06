using Microsoft.Data.SqlClient;
using System.Data;

namespace AzureVaultCopy.Data
{
    public class DapperContext
    {
        private readonly string _connectionString;

        public DapperContext( )
        {
            _connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection");
            if (string.IsNullOrWhiteSpace(_connectionString))
                throw new InvalidOperationException("Database connection string not found in environment variables.");
        }

        public IDbConnection CreateConnection() => new SqlConnection(_connectionString);
    }
}
