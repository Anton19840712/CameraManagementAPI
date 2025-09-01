using CameraManagementAPI.Models;
using CameraManagementAPI.Services;
using Microsoft.AspNetCore.Mvc;

namespace CameraManagementAPI.Controllers;

/// <summary>
/// Контроллер для управления подписками на события
/// </summary>
[ApiController]
[Route("events")]
public class EventsController : ControllerBase
{
    private readonly EventSubscriptionService _subscriptionService;
    private readonly ILogger<EventsController> _logger;

    public EventsController(EventSubscriptionService subscriptionService, ILogger<EventsController> logger)
    {
        _subscriptionService = subscriptionService;
        _logger = logger;
    }

    /// <summary>
    /// Создать подписку на события
    /// POST http://localhost:8080/events/subscriptions
    /// </summary>
    [HttpPost("subscriptions")]
    public ActionResult<SubscriptionResponse<EventSubscription>> CreateSubscription([FromBody] CreateSubscriptionRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new SubscriptionResponse<object>
            {
                Status = "error",
                Error = "Invalid request data"
            });
        }

        try
        {
            var subscription = _subscriptionService.CreateSubscription(request);
            
            var response = new SubscriptionResponse<EventSubscription>
            {
                Status = "success",
                Data = subscription
            };

            return StatusCode(201, response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create subscription");
            return StatusCode(500, new SubscriptionResponse<object>
            {
                Status = "error",
                Error = ex.Message
            });
        }
    }

    /// <summary>
    /// Получить информацию по всем подпискам на события
    /// GET http://localhost:8080/events/subscriptions
    /// </summary>
    [HttpGet("subscriptions")]
    public ActionResult<SubscriptionResponse<IEnumerable<EventSubscription>>> GetAllSubscriptions()
    {
        var subscriptions = _subscriptionService.GetAllSubscriptions();
        
        return Ok(new SubscriptionResponse<IEnumerable<EventSubscription>>
        {
            Status = "success",
            Data = subscriptions
        });
    }

    /// <summary>
    /// Получить информацию по конкретной подписке
    /// GET http://localhost:8080/events/subscriptions/{id}
    /// </summary>
    [HttpGet("subscriptions/{id}")]
    public ActionResult<SubscriptionResponse<EventSubscription>> GetSubscription(string id)
    {
        var subscription = _subscriptionService.GetSubscriptionById(id);
        
        if (subscription == null)
        {
            return NotFound(new SubscriptionResponse<object>
            {
                Status = "error",
                Error = $"Subscription with ID {id} not found"
            });
        }

        return Ok(new SubscriptionResponse<EventSubscription>
        {
            Status = "success",
            Data = subscription
        });
    }

    /// <summary>
    /// Отписаться от событий
    /// DELETE http://localhost:8080/events/subscriptions/{id}
    /// </summary>
    [HttpDelete("subscriptions/{id}")]
    public ActionResult<SubscriptionResponse<EventSubscription>> DeleteSubscription(string id)
    {
        var deletedSubscription = _subscriptionService.DeleteSubscription(id);
        
        if (deletedSubscription == null)
        {
            return NotFound(new SubscriptionResponse<object>
            {
                Status = "error",
                Error = $"Subscription with ID {id} not found"
            });
        }

        return Ok(new SubscriptionResponse<EventSubscription>
        {
            Status = "success",
            Data = deletedSubscription
        });
    }

    /// <summary>
    /// Генерировать тестовое событие (для демонстрации)
    /// POST http://localhost:8080/events/test
    /// </summary>
    [HttpPost("test")]
    public async Task<ActionResult> GenerateTestEvent([FromQuery] string? cameraId = null)
    {
        try
        {
            await _subscriptionService.GenerateTestEventAsync(cameraId ?? "882E965B-1038-4EA1-B6B1-66803EFC7C9C");
            
            return Ok(new SubscriptionResponse<object>
            {
                Status = "success",
                Data = "Test event generated and sent to subscribers"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate test event");
            return StatusCode(500, new SubscriptionResponse<object>
            {
                Status = "error",
                Error = ex.Message
            });
        }
    }

    /// <summary>
    /// Endpoint для приема событий от внешних систем
    /// POST http://localhost:8080/events/webhook
    /// </summary>
    [HttpPost("webhook")]
    public async Task<ActionResult> ReceiveEvent([FromBody] VideoAnalyticsEvent eventData)
    {
        try
        {
            _logger.LogInformation("Received event for camera {CameraId} with match {Match}", 
                eventData.CameraId, eventData.Match);
            
            await _subscriptionService.SendEventAsync(eventData);
            
            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to process received event");
            return StatusCode(500, "Failed to process event");
        }
    }
}