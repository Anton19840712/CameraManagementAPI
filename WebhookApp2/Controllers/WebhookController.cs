using Microsoft.AspNetCore.Mvc;
using WebhookApp2.Models;

namespace WebhookApp2.Controllers;

[ApiController]
[Route("api/[controller]")]
public class WebhookController : ControllerBase
{
    private readonly ILogger<WebhookController> _logger;

    public WebhookController(ILogger<WebhookController> logger)
    {
        _logger = logger;
    }

	/// <summary>
	/// �������� ��������� �� ���������������
	/// </summary>
	/// <param name="eventData"></param>
	/// <returns></returns>
	[HttpPost("events")]
    public IActionResult ReceiveEvent([FromBody] WebhookEventData eventData)
    {
        try
        {
            _logger.LogInformation("=== WEBHOOK APP 2 - EVENT RECEIVED ===");
            _logger.LogInformation("Event Type: {EventType}", eventData.EventType);
            _logger.LogInformation("Timestamp: {Timestamp}", eventData.Data.Timestamp);
            _logger.LogInformation("Camera ID: {CameraId}", eventData.Data.CameraId);
            _logger.LogInformation("Event ID: {EventId}", eventData.Data.EventId);
            _logger.LogInformation("Match: {Match}", eventData.Data.Match);
            _logger.LogInformation("Photo: {Photo}", eventData.Data.Photo);
            _logger.LogInformation("Metadata: {Metadata}", System.Text.Json.JsonSerializer.Serialize(eventData.Data.Metadata));
            _logger.LogInformation("==========================================");

            return Ok(new { status = "received", app = "WebhookApp2" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing webhook event");
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("health")]
    public IActionResult Health()
    {
        return Ok(new { status = "healthy", app = "WebhookApp2" });
    }
}