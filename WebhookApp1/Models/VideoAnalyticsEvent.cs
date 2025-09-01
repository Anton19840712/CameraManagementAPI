namespace WebhookApp1.Models;

public class VideoAnalyticsEvent
{
    public long Timestamp { get; set; }
    public string CameraId { get; set; } = string.Empty;
    public string Photo { get; set; } = string.Empty;
    public double Match { get; set; }
    public string EventId { get; set; } = string.Empty;
    public string EventType { get; set; } = string.Empty;
    public Dictionary<string, object> Metadata { get; set; } = new();
}

public class WebhookEventData
{
    public string EventType { get; set; } = string.Empty;
    public VideoAnalyticsEvent Data { get; set; } = new();
}