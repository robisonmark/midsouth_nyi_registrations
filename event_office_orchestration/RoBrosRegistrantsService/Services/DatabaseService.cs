using System.Data;
using Npgsql;
using Dapper;

namespace EventOfficeApi.Services
{
    public class DatabaseService : IDisposable
    {
        private readonly string _connectionString;
        private readonly IDbConnection _connection;

        public DatabaseService(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
            _connection = new NpgsqlConnection(_connectionString);
            _connection.Open();
        }

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
            _connection.Dispose();
        }
    }
}
