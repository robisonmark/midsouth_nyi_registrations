using System.Data;
using Microsoft.Data.SqlClient;
using Dapper;

namespace RoBrosRegistrantsService.Services
{
    public class DatabaseService : IDisposable
    {
        private readonly string _connectionString;
        private readonly IDbConnection _connection;

        public DatabaseService(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection") 
                ?? throw new InvalidOperationException("DefaultConnection string not found");
            _connection = new SqlConnection(_connectionString);
            _connection.Open();
        }

        // public async Task<SqlCommand> InitializeCommandAsync(string sql, object? parameters = null)
        // {
        //     await using var connection = new SqlConnection(_connectionString);
        //     await connection.OpenAsync();
        //     return new SqlCommand(sql, connection);
        // }

        public async Task<T?> QuerySingleAsync<T>(string sql, object? parameters = null)
        {
            return await _connection.QuerySingleOrDefaultAsync<T>(sql, parameters);
        }

        public async Task<IEnumerable<T>> QueryAsync<T>(string sql, object? parameters = null)
        {
            return await _connection.QueryAsync<T>(sql, parameters);
        }

        public async Task<int> ExecuteAsync(string sql, object? parameters = null)
        {
            Console.WriteLine($"Executing SQL: {sql} with parameters: {parameters}");
            return await _connection.ExecuteAsync(sql, parameters);
        }

        public void Dispose()
        {
            _connection?.Dispose();
        }
    }
}