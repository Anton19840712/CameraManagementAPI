using System.Text;
using System.Text.Json;

namespace WebhookApp2.Services;

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

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await Task.Delay(4000, cancellationToken); // Ждем запуска сервиса (немного дольше чем App1)

        _logger.LogInformation("WebhookApp2: Managing webhook subscription");
        
        // Управляем подпиской через SubscriptionManagerService
        var subscriptionId = await _subscriptionManagerService.GetOrCreateSubscriptionAsync();
        if (!string.IsNullOrEmpty(subscriptionId))
        {
            _logger.LogInformation("WebhookApp2: Successfully managed subscription: {SubscriptionId}", subscriptionId);
        }
        else
        {
            _logger.LogError("WebhookApp2: Failed to manage subscription");
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _httpClient?.Dispose();
        return Task.CompletedTask;
    }
}