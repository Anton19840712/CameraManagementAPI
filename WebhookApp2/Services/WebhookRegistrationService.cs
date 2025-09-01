using System.Text;
using System.Text.Json;

namespace WebhookApp2.Services;

public class WebhookRegistrationService : IHostedService
{
    private readonly ILogger<WebhookRegistrationService> _logger;
    private readonly IConfiguration _configuration;
    private readonly HttpClient _httpClient;

    public WebhookRegistrationService(ILogger<WebhookRegistrationService> logger, IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
        _httpClient = new HttpClient();
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await Task.Delay(6000, cancellationToken); // Ждем запуска сервиса (немного дольше чем App1)

        var cameraManagementUrl = _configuration["CameraManagementUrl"] ?? "http://localhost:7080";
        var webhookUrl = _configuration["WebhookUrl"] ?? "http://localhost:7082/api/webhook/events";

        await RegisterWebhook(cameraManagementUrl, webhookUrl);
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
}