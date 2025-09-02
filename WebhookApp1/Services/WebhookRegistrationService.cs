using System.Text;
using System.Text.Json;

namespace WebhookApp1.Services;

public class WebhookRegistrationService : IHostedService
{
    private readonly ILogger<WebhookRegistrationService> _logger;
    private readonly IConfiguration _configuration;
    private readonly HttpClient _httpClient;
    private readonly ISubscriptionManagerService _subscriptionManagerService;

    public WebhookRegistrationService(ILogger<WebhookRegistrationService> logger, IConfiguration configuration, ISubscriptionManagerService subscriptionManagerService)
    {
        _logger = logger;
        _configuration = configuration;
        _subscriptionManagerService = subscriptionManagerService;
        _httpClient = new HttpClient();
    }

	/// <summary>
	/// Отправляем запрос на регистрацию вебхука после запуска сервиса
	/// </summary>
	/// <param name="cancellationToken"></param>
	/// <returns></returns>
	public async Task StartAsync(CancellationToken cancellationToken)
    {
        await Task.Delay(3000, cancellationToken); // Ждем запуска сервиса

        // Инициализируем базу данных и таблицы

        _logger.LogInformation("WebhookApp1: Managing webhook subscription");
        
        // Управляем подпиской через SubscriptionManagerService
        var subscriptionId = await _subscriptionManagerService.GetOrCreateSubscriptionAsync();
        if (!string.IsNullOrEmpty(subscriptionId))
        {
            _logger.LogInformation("WebhookApp1: Successfully managed subscription: {SubscriptionId}", subscriptionId);
        }
        else
        {
            _logger.LogError("WebhookApp1: Failed to manage subscription");
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _httpClient?.Dispose();
        return Task.CompletedTask;
    }
}