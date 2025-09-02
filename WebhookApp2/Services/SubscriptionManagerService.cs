using Npgsql;
using System.Text;
using System.Text.Json;

namespace WebhookApp2.Services;

public interface ISubscriptionManagerService
{
	Task<string?> GetOrCreateSubscriptionAsync();
	Task CleanupSubscriptionAsync();
}

public class SubscriptionManagerService : ISubscriptionManagerService
{
	private readonly ILogger<SubscriptionManagerService> _logger;
	private readonly IConfiguration _configuration;
	private readonly HttpClient _httpClient;
	private readonly string _connectionString;
	private readonly string _appName;
	private readonly string _webhookUrl;
	private readonly string _cameraManagementUrl;

	public SubscriptionManagerService(ILogger<SubscriptionManagerService> logger, IConfiguration configuration)
	{
		_logger = logger;
		_configuration = configuration;
		_httpClient = new HttpClient();
		_connectionString = _configuration.GetConnectionString("DefaultConnection") ?? "Host=localhost;Port=5432;Database=webhook_db;Username=postgres;Password=mysecret";
		_appName = "WebhookApp2";
		_webhookUrl = _configuration["WebhookUrl"] ?? "http://localhost:7081/api/webhook/events";
		_cameraManagementUrl = _configuration["CameraManagementUrl"] ?? "http://localhost:7080";
	}

	public async Task<string?> GetOrCreateSubscriptionAsync()
	{
		try
		{
			// 1. Проверяем сначала КИИС на наличие существующей подписки:
			var existingKiisSubscriptions = await GetKiisSubscriptionsAsync();

			if (existingKiisSubscriptions!.Count() == 0)
			{
				_logger.LogInformation("Подписок в КИИС не было найдено.");
				_logger.LogInformation("Создаем новую подписку в базе.");

				// 2. Если нет подписки в КИИС - создаем новую и сначала отправляем в КИИС:
				var newSubscriptionId = await CreateKiisSubscriptionAsync();
				_logger.LogInformation("{AppName}: Pushed new subscription to KIIS: {SubscriptionId}", _appName, newSubscriptionId);

				// 3. Далее сохраняем в нашу локальную БД:
				await SaveLocalSubscriptionAsync(newSubscriptionId!, _webhookUrl);
				_logger.LogInformation("{AppName}: Created new subscription in database locally: {SubscriptionId}", _appName, newSubscriptionId);

				return newSubscriptionId;
			}

			// если хотя бы одна подписка в КИИС есть:
			else
			{
				var kiisSubscription = existingKiisSubscriptions?.FirstOrDefault(s => s.Callback == _webhookUrl);
				var kiisId = kiisSubscription?.Id;

				var localSubscriptionId = await GetLocalSubscriptionAsync();

				// если подписки эквивалентны, ничего не делаем:
				if (kiisId == localSubscriptionId)
				{
					_logger.LogInformation("Подписки эквивалентны по их id.");
					return kiisId;
				}

				// если подписки НЕ эквивалентны, обновляем подписку у нас так, как она существует в КИИС:
				else
				{
					_logger.LogInformation("Подписки НЕ эквивалентны по их id.");

					if (localSubscriptionId == null)
					{
						await SaveLocalSubscriptionAsync(kiisSubscription!.Id, _webhookUrl);
						return kiisSubscription!.Id;
					}
					else
					{
						// придерживаемся того, что в КИИС актуальнее, чем в локальной БД, поэтому предоставляем приоритет КИИС над локальной подпиской:
						await DeleteLocalSubscriptionAsync();
						await SaveLocalSubscriptionAsync(kiisSubscription!.Id, _webhookUrl);
						return kiisId;
					}
				}
			}
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "{AppName}: Error managing subscription", _appName);
			return null;
		}
	}

	/// <summary>
	/// Идет в КИИС по api и собирает существующие подписки
	/// </summary>
	/// <returns></returns>
	private async Task<List<KiisSubscription>?> GetKiisSubscriptionsAsync()
	{
		try
		{
			var response = await _httpClient.GetAsync($"{_cameraManagementUrl}/api/events/subscriptions");
			if (response.IsSuccessStatusCode)
			{
				var json = await response.Content.ReadAsStringAsync();
				var result = JsonSerializer.Deserialize<KiisSubscriptionResponse>(json, new JsonSerializerOptions
				{
					PropertyNameCaseInsensitive = true
				});
				return result?.Data?.ToList();
			}
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "{AppName}: Error getting KIIS subscriptions", _appName);
		}
		return null;
	}

	private async Task<string?> CreateKiisSubscriptionAsync()
	{
		try
		{
			var subscription = new
			{
				Callback = _webhookUrl,
				Filter = new
				{
					Action = "RUN",
					Type = "MACRO"
				},
				Id = "*"
			};

			var json = JsonSerializer.Serialize(subscription);
			var content = new StringContent(json, Encoding.UTF8, "application/json");

			var response = await _httpClient.PostAsync($"{_cameraManagementUrl}/api/events/subscriptions", content);

			if (response.IsSuccessStatusCode)
			{
				var responseJson = await response.Content.ReadAsStringAsync();
				var result = JsonSerializer.Deserialize<KiisCreateSubscriptionResponse>(responseJson, new JsonSerializerOptions
				{
					PropertyNameCaseInsensitive = true
				});
				return result?.Data?.Id;
			}
			else
			{
				_logger.LogError("{AppName}: Failed to create KIIS subscription. Status: {Status}, Content: {Content}",
					_appName, response.StatusCode, await response.Content.ReadAsStringAsync());
			}
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "{AppName}: Error creating KIIS subscription", _appName);
		}
		return null;
	}

	private async Task<string?> GetLocalSubscriptionAsync()
	{
		try
		{
			await using var connection = new NpgsqlConnection(_connectionString);
			await connection.OpenAsync();

			// Создаем таблицу для локальных подписок если не существует
			await using var createTableCmd = new NpgsqlCommand(@"
                CREATE TABLE IF NOT EXISTS webhook_subscriptions (
                    id SERIAL PRIMARY KEY,
                    app_name VARCHAR(100) NOT NULL,
                    kiis_subscription_id VARCHAR(100) NOT NULL,
                    webhook_url VARCHAR(500) NOT NULL,
                    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
                    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
                );
                
                CREATE UNIQUE INDEX IF NOT EXISTS idx_webhook_subscriptions_app 
                    ON webhook_subscriptions(app_name);
            ", connection);
			await createTableCmd.ExecuteNonQueryAsync();

			await using var cmd = new NpgsqlCommand(@"
                SELECT kiis_subscription_id FROM webhook_subscriptions 
                WHERE app_name = @app_name", connection);

			cmd.Parameters.AddWithValue("app_name", _appName);
			var result = await cmd.ExecuteScalarAsync();
			return result?.ToString();
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "{AppName}: Error getting local subscription", _appName);
			return null;
		}
	}

	private async Task SaveLocalSubscriptionAsync(string kiisSubscriptionId, string webhookUrl)
	{
		try
		{
			await using var connection = new NpgsqlConnection(_connectionString);
			await connection.OpenAsync();

			await using var cmd = new NpgsqlCommand(@"
                INSERT INTO webhook_subscriptions (app_name, kiis_subscription_id, webhook_url, created_at, updated_at)
                VALUES (@app_name, @kiis_subscription_id, @webhook_url, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP)
                ON CONFLICT (app_name) 
                DO UPDATE SET 
                    kiis_subscription_id = EXCLUDED.kiis_subscription_id,
                    webhook_url = EXCLUDED.webhook_url,
                    updated_at = CURRENT_TIMESTAMP", connection);

			cmd.Parameters.AddWithValue("app_name", _appName);
			cmd.Parameters.AddWithValue("kiis_subscription_id", kiisSubscriptionId);
			cmd.Parameters.AddWithValue("webhook_url", webhookUrl);

			await cmd.ExecuteNonQueryAsync();
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "{AppName}: Error saving local subscription", _appName);
		}
	}

	private async Task DeleteLocalSubscriptionAsync()
	{
		try
		{
			await using var connection = new NpgsqlConnection(_connectionString);
			await connection.OpenAsync();

			await using var cmd = new NpgsqlCommand(@"
                DELETE FROM webhook_subscriptions WHERE app_name = @app_name", connection);

			cmd.Parameters.AddWithValue("app_name", _appName);
			await cmd.ExecuteNonQueryAsync();
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "{AppName}: Error deleting local subscription", _appName);
		}
	}

	public async Task CleanupSubscriptionAsync()
	{
		// При остановке приложения можно удалить подписку из КИИС если нужно
		// Пока оставляем подписку активной для других экземпляров
	}
}

// Модели для десериализации ответов от КИИС
public class KiisSubscription
{
	public string Id { get; set; } = "";
	public string Callback { get; set; } = "";
	public KiisFilter Filter { get; set; } = new();
}

public class KiisFilter
{
	public string? Action { get; set; }
	public string? Type { get; set; }
	public string Id { get; set; } = "*";
}

public class KiisSubscriptionResponse
{
	public string Status { get; set; } = "";
	public KiisSubscription[]? Data { get; set; }
}

public class KiisCreateSubscriptionResponse
{
	public string Status { get; set; } = "";
	public KiisSubscription? Data { get; set; }
}
