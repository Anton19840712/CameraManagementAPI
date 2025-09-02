using System.Text;
using System.Text.Json;

namespace WebhookApp2.Services;

public class WebhookRegistrationService : IHostedService
{
    private readonly ILogger<WebhookRegistrationService> _logger;
    private readonly IConfiguration _configuration;
    private readonly HttpClient _httpClient;
    private readonly ILeaderElectionService _leaderElectionService;
    private readonly IDatabaseInitializationService _databaseInitializationService;

    public WebhookRegistrationService(ILogger<WebhookRegistrationService> logger, IConfiguration configuration, ILeaderElectionService leaderElectionService, IDatabaseInitializationService databaseInitializationService)
    {
        _logger = logger;
        _configuration = configuration;
        _leaderElectionService = leaderElectionService;
        _databaseInitializationService = databaseInitializationService;
        _httpClient = new HttpClient();
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await Task.Delay(4000, cancellationToken); // Ждем запуска сервиса (немного дольше чем App1)

        // Инициализируем базу данных и таблицы
        await _databaseInitializationService.InitializeDatabaseAsync();

        // Определяем роль в кластере
        var isLeader = await _leaderElectionService.TryBecomeLeaderAsync();
        
        if (isLeader)
        {
            _logger.LogInformation("WebhookApp2: This instance is LEADER - will register webhook");
            
            var cameraManagementUrl = _configuration["CameraManagementUrl"] ?? "http://localhost:7080";
            var webhookUrl = _configuration["WebhookUrl"] ?? "http://localhost:7082/api/webhook/events";

            await RegisterWebhook(cameraManagementUrl, webhookUrl);
            
            // Запускаем heartbeat для поддержания лидерства
            _ = Task.Run(async () => await HeartbeatLoop(cancellationToken), cancellationToken);
        }
        else
        {
            _logger.LogInformation("WebhookApp2: This instance is FOLLOWER - will not register webhook");
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _httpClient?.Dispose();
        return Task.CompletedTask;
    }

    private async Task RegisterWebhook(string cameraManagementUrl, string webhookUrl)
    {
        try
        {
            var subscription = new
            {
                Callback = webhookUrl,
                Id = "*"
            };

            var json = JsonSerializer.Serialize(subscription);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync($"{cameraManagementUrl}/api/events/subscriptions", content);
            
            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                _logger.LogInformation("WebhookApp2: Successfully registered webhook subscription: {Response}", responseContent);
            }
            else
            {
                _logger.LogError("WebhookApp2: Failed to register webhook. Status: {Status}, Content: {Content}", 
                    response.StatusCode, await response.Content.ReadAsStringAsync());
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "WebhookApp2: Error registering webhook");
        }
    }

    private async Task HeartbeatLoop(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                await _leaderElectionService.UpdateHeartbeatAsync();
                await Task.Delay(10000, cancellationToken); // Heartbeat каждые 10 секунд
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "WebhookApp2: Error during heartbeat");
                await Task.Delay(5000, cancellationToken);
            }
        }
    }
}