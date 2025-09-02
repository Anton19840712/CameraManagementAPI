using Npgsql;

namespace WebhookApp2.Services;

public interface IDatabaseInitializationService
{
    Task InitializeDatabaseAsync();
}

public class DatabaseInitializationService : IDatabaseInitializationService
{
    private readonly ILogger<DatabaseInitializationService> _logger;
    private readonly IConfiguration _configuration;
    private readonly string _connectionString;

    public DatabaseInitializationService(ILogger<DatabaseInitializationService> logger, IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
        _connectionString = _configuration.GetConnectionString("DefaultConnection") ??
			"Host=localhost;Port=5432;Database=webhook_db;Username=postgres;Password=mysecret";
    }

    public async Task InitializeDatabaseAsync()
    {
        try
        {
            // Сначала подключаемся к базе postgres для создания нашей базы
            var builder = new NpgsqlConnectionStringBuilder(_connectionString);
            var databaseName = builder.Database;
            builder.Database = "postgres"; // Подключаемся к системной базе
            var systemConnectionString = builder.ToString();

            await using var systemConnection = new NpgsqlConnection(systemConnectionString);
            await systemConnection.OpenAsync();

            // Проверяем, существует ли база данных
            await using var checkDbCmd = new NpgsqlCommand(
                "SELECT 1 FROM pg_database WHERE datname = @dbname", systemConnection);
            checkDbCmd.Parameters.AddWithValue("dbname", databaseName);
            
            var dbExists = await checkDbCmd.ExecuteScalarAsync();
            
            if (dbExists == null)
            {
                // Создаем базу данных
                await using var createDbCmd = new NpgsqlCommand(
                    $"CREATE DATABASE \"{databaseName}\"", systemConnection);
                await createDbCmd.ExecuteNonQueryAsync();
                
                _logger.LogInformation("Created database: {DatabaseName}", databaseName);
            }
            else
            {
                _logger.LogInformation("Database {DatabaseName} already exists", databaseName);
            }

            // Теперь подключаемся к нашей базе для создания таблицы
            await using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();

            // Создаем таблицу если она не существует
            await using var createTableCmd = new NpgsqlCommand(@"
                CREATE TABLE IF NOT EXISTS webhook_instances (
                    id VARCHAR(50) PRIMARY KEY,                    -- Уникальный ID экземпляра (GUID)
                    app_name VARCHAR(100) NOT NULL,               -- Имя приложения (WebhookApp1, WebhookApp2)
                    is_leader BOOLEAN DEFAULT FALSE,              -- Является ли экземпляр лидером
                    heartbeat_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP, -- Последний heartbeat
                    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP    -- Время создания записи
                );

                -- Создаем индексы если они не существуют
                CREATE INDEX IF NOT EXISTS idx_webhook_instances_app_leader 
                    ON webhook_instances(app_name, is_leader);

                CREATE INDEX IF NOT EXISTS idx_webhook_instances_heartbeat 
                    ON webhook_instances(heartbeat_at);
            ", connection);

            await createTableCmd.ExecuteNonQueryAsync();
            _logger.LogInformation("Database schema initialized successfully");

            // Очищаем старые записи (старше 5 минут)
            await using var cleanupCmd = new NpgsqlCommand(@"
                DELETE FROM webhook_instances 
                WHERE heartbeat_at < (CURRENT_TIMESTAMP - INTERVAL '5 minutes')
            ", connection);
            
            var deletedRows = await cleanupCmd.ExecuteNonQueryAsync();
            if (deletedRows > 0)
            {
                _logger.LogInformation("Cleaned up {DeletedRows} stale instances", deletedRows);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize database");
            throw;
        }
    }
}