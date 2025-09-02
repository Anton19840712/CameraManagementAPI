using System.Data;
using Microsoft.Data.SqlClient;
using Npgsql;

namespace WebhookApp2.Services;

public interface ILeaderElectionService
{
    Task<bool> TryBecomeLeaderAsync();
    Task<bool> IsLeaderAsync();
    Task UpdateHeartbeatAsync();
    Task<string> GetInstanceIdAsync();
}

public class LeaderElectionService : ILeaderElectionService, IDisposable
{
    private readonly ILogger<LeaderElectionService> _logger;
    private readonly IConfiguration _configuration;
    private readonly string _instanceId;
    private readonly string _appName;
    private readonly string _connectionString;
    private bool _isLeader = false;

    public LeaderElectionService(ILogger<LeaderElectionService> logger, IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
        _instanceId = Guid.NewGuid().ToString();
        _appName = "WebhookApp2";
        _connectionString = _configuration.GetConnectionString("DefaultConnection") ?? 
            "Host=localhost;Port=5432;Database=webhook_db;Username=postgres;Password=mysecret";
    }

    public async Task<bool> TryBecomeLeaderAsync()
    {
        try
        {
            await using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();

            // Начинаем транзакцию для атомарности операций
            await using var transaction = await connection.BeginTransactionAsync();

            try
            {
                // Регистрируем текущий экземпляр
                await using var insertCmd = new NpgsqlCommand(
                    @"INSERT INTO webhook_instances (id, app_name, is_leader, heartbeat_at, created_at) 
                      VALUES (@id, @app_name, FALSE, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP)
                      ON CONFLICT (id) DO UPDATE SET heartbeat_at = CURRENT_TIMESTAMP", connection, transaction);
                
                insertCmd.Parameters.AddWithValue("id", _instanceId);
                insertCmd.Parameters.AddWithValue("app_name", _appName);
                await insertCmd.ExecuteNonQueryAsync();

                // Проверяем, есть ли уже лидер для этого приложения
                await using var checkCmd = new NpgsqlCommand(
                    @"SELECT COUNT(*) FROM webhook_instances 
                      WHERE is_leader = TRUE 
                      AND heartbeat_at > (CURRENT_TIMESTAMP - INTERVAL '30 seconds')", connection, transaction);
                
                checkCmd.Parameters.AddWithValue("app_name", _appName);
                var existingLeaderCount = Convert.ToInt32(await checkCmd.ExecuteScalarAsync());

                if (existingLeaderCount == 0)
                {
                    // Нет активного лидера - становимся лидером
                    await using var becomeLeaderCmd = new NpgsqlCommand(
                        @"UPDATE webhook_instances SET is_leader = TRUE 
                          WHERE id = @id", connection, transaction);
                    
                    becomeLeaderCmd.Parameters.AddWithValue("id", _instanceId);
                    await becomeLeaderCmd.ExecuteNonQueryAsync();

                    _isLeader = true;
                    _logger.LogInformation("Instance {InstanceId} became leader for {AppName}", _instanceId, _appName);
                }
                else
                {
                    _isLeader = false;
                    _logger.LogInformation("Instance {InstanceId} became follower for {AppName} (existing leader found)", _instanceId, _appName);
                }

                await transaction.CommitAsync();
                return _isLeader;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to determine leader status for instance {InstanceId}", _instanceId);
            return false;
        }
    }

    public async Task<bool> IsLeaderAsync()
    {
        try
        {
            await using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();

            await using var cmd = new NpgsqlCommand(
                @"SELECT is_leader FROM webhook_instances 
                  WHERE id = @id", connection);
            
            cmd.Parameters.AddWithValue("id", _instanceId);
            var result = await cmd.ExecuteScalarAsync();
            
            return result != null && (bool)result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to check leader status for instance {InstanceId}", _instanceId);
            return false;
        }
    }

    public async Task UpdateHeartbeatAsync()
    {
        try
        {
            await using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();

            await using var cmd = new NpgsqlCommand(
                @"UPDATE webhook_instances 
                  SET heartbeat_at = CURRENT_TIMESTAMP 
                  WHERE id = @id", connection);
            
            cmd.Parameters.AddWithValue("id", _instanceId);
            await cmd.ExecuteNonQueryAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update heartbeat for instance {InstanceId}", _instanceId);
        }
    }

    public Task<string> GetInstanceIdAsync()
    {
        return Task.FromResult(_instanceId);
    }

    public void Dispose()
    {
        // Cleanup when service is disposed
        Task.Run(async () =>
        {
            try
            {
                await using var connection = new NpgsqlConnection(_connectionString);
                await connection.OpenAsync();

                await using var cmd = new NpgsqlCommand(
                    @"DELETE FROM webhook_instances WHERE id = @id", connection);
                
                cmd.Parameters.AddWithValue("id", _instanceId);
                await cmd.ExecuteNonQueryAsync();

                _logger.LogInformation("Cleaned up instance {InstanceId} from database", _instanceId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to cleanup instance {InstanceId}", _instanceId);
            }
        });
    }
}