using Microsoft.AspNetCore.Mvc;
using Npgsql;
using Dapper;
using System.Threading.Tasks;
using System.Data;

namespace EventOfficeApi.Controllers
{
    public abstract class BaseController : ControllerBase
    {
        private readonly NpgsqlConnection _dbConnection;

        protected BaseController(NpgsqlConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }

        // Executes a query that returns results
        protected async Task<T> QuerySingleAsync<T>(string sql, object parameters = null)
        {
            return await _dbConnection.QueryFirstOrDefaultAsync<T>(sql, parameters);
        }

        // Executes a command that doesn't return results
        protected async Task<int> ExecuteAsync(string sql, object parameters = null)
        {
            return await _dbConnection.ExecuteAsync(sql, parameters);
        }

        // Executes a command in a transaction
        protected async Task<int> ExecuteInTransactionAsync(string[] sqlStatements, object[] parameters)
        {
            using var transaction = _dbConnection.BeginTransaction();
            try
            {
                int rowsAffected = 0;
                for (int i = 0; i < sqlStatements.Length; i++)
                {
                    rowsAffected += await _dbConnection.ExecuteAsync(sqlStatements[i], parameters[i], transaction);
                }

                await transaction.CommitAsync();
                return rowsAffected;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw; // Rethrow the exception after rollback
            }
        }
    }
}
