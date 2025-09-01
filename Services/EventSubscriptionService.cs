using CameraManagementAPI.Models;
using System.Text.Json;

namespace CameraManagementAPI.Services;

/// <summary>
/// Сервис для работы с подписками на события
/// </summary>
public class EventSubscriptionService
{
    private readonly List<EventSubscription> _subscriptions = new();
    private readonly HttpClient _httpClient;
    private readonly ILogger<EventSubscriptionService> _logger;

    public EventSubscriptionService(HttpClient httpClient, ILogger<EventSubscriptionService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    /// <summary>
    /// Создать подписку на события
    /// </summary>
    public EventSubscription CreateSubscription(CreateSubscriptionRequest request)
    {
        var subscription = new EventSubscription
        {
            Callback = request.Callback,
            Filter = request.Filter
        };

        // Если указан конкретный ID камеры, используем его в фильтре
        if (!string.IsNullOrEmpty(request.Id) && request.Id != "*")
        {
            subscription.Filter.Id = request.Id;
        }

        _subscriptions.Add(subscription);
        _logger.LogInformation("Created subscription {Id} for callback {Callback}", subscription.Id, subscription.Callback);
        
        return subscription;
    }

    /// <summary>
    /// Получить все активные подписки
    /// </summary>
    public IEnumerable<EventSubscription> GetAllSubscriptions()
    {
        return _subscriptions.Where(s => s.IsActive);
    }

    /// <summary>
    /// Получить подписку по ID
    /// </summary>
    public EventSubscription? GetSubscriptionById(string id)
    {
        return _subscriptions.FirstOrDefault(s => s.Id == id && s.IsActive);
    }

    /// <summary>
    /// Удалить подписку
    /// </summary>
    public EventSubscription? DeleteSubscription(string id)
    {
        var subscription = GetSubscriptionById(id);
        if (subscription != null)
        {
            subscription.IsActive = false;
            _logger.LogInformation("Deleted subscription {Id}", id);
        }
        return subscription;
    }

    /// <summary>
    /// Отправить событие всем подходящим подписчикам
    /// </summary>
    public async Task SendEventAsync(VideoAnalyticsEvent eventData)
    {
        var relevantSubscriptions = GetRelevantSubscriptions(eventData);
        
        var sendTasks = relevantSubscriptions.Select(async subscription =>
        {
            try
            {
                await SendEventToSubscriber(subscription, eventData);
                _logger.LogInformation("Event sent successfully to {Callback}", subscription.Callback);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send event to {Callback}", subscription.Callback);
            }
        });

        await Task.WhenAll(sendTasks);
    }

    /// <summary>
    /// Получить подписки, которые должны получить это событие
    /// </summary>
    private IEnumerable<EventSubscription> GetRelevantSubscriptions(VideoAnalyticsEvent eventData)
    {
        return _subscriptions.Where(s => 
            s.IsActive && 
            (s.Filter.Id == "*" || s.Filter.Id == eventData.CameraId));
    }

    /// <summary>
    /// Отправить событие конкретному подписчику
    /// </summary>
    private async Task SendEventToSubscriber(EventSubscription subscription, VideoAnalyticsEvent eventData)
    {
        var json = JsonSerializer.Serialize(eventData, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
        
        using var response = await _httpClient.PostAsync(subscription.Callback, content);
        
        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException($"Failed to send event to {subscription.Callback}. Status: {response.StatusCode}");
        }
    }

    /// <summary>
    /// Генерировать тестовые события (для демонстрации)
    /// </summary>
    public async Task GenerateTestEventAsync(string cameraId = "882E965B-1038-4EA1-B6B1-66803EFC7C9C")
    {
        var testEvent = new VideoAnalyticsEvent
        {
            Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
            CameraId = cameraId,
            Photo = $"/face1/uploads/parsiv-test/history/{DateTime.Now:yyyy-MM-dd}/test_photo.jpg",
            Thumbnail = $"/face1/uploads/parsiv-test/history/{DateTime.Now:yyyy-MM-dd}/test_thumb.jpg",
            Match = 0.818800,
            Zone = "676,358,769,451",
            Vendor = 18
        };

        await SendEventAsync(testEvent);
    }
}