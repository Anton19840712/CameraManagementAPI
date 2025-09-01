using System.ComponentModel.DataAnnotations;

namespace CameraManagementAPI.Models;

/// <summary>
/// Модель подписки на события
/// </summary>
public class EventSubscription
{
    /// <summary>
    /// Уникальный идентификатор подписки
    /// </summary>
    public string Id { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Endpoint, получатель сработок
    /// </summary>
    [Required]
    public string Callback { get; set; } = "";

    /// <summary>
    /// Фильтр подписки
    /// </summary>
    public EventFilter Filter { get; set; } = new();

    /// <summary>
    /// Время создания подписки
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Активна ли подписка
    /// </summary>
    public bool IsActive { get; set; } = true;
}

/// <summary>
/// Фильтр для подписки на события
/// </summary>
public class EventFilter
{
    /// <summary>
    /// Фильтр действия
    /// </summary>
    public string? Action { get; set; }

    /// <summary>
    /// Фильтр типа объекта
    /// </summary>
    public string? Type { get; set; }

    /// <summary>
    /// Идентификатор камеры (* для всех камер)
    /// </summary>
    public string Id { get; set; } = "*";
}

/// <summary>
/// Запрос на создание подписки
/// </summary>
public class CreateSubscriptionRequest
{
    /// <summary>
    /// Endpoint, получатель сработок
    /// </summary>
    [Required]
    public string Callback { get; set; } = "";

    /// <summary>
    /// Фильтр подписки
    /// </summary>
    public EventFilter Filter { get; set; } = new();

    /// <summary>
    /// Идентификатор камеры
    /// </summary>
    public string Id { get; set; } = "*";
}

/// <summary>
/// Ответ на операции с подписками
/// </summary>
public class SubscriptionResponse<T>
{
    /// <summary>
    /// Статус выполнения запроса
    /// </summary>
    public string Status { get; set; } = "success";

    /// <summary>
    /// Данные ответа
    /// </summary>
    public T? Data { get; set; }

    /// <summary>
    /// Сообщение об ошибке
    /// </summary>
    public string? Error { get; set; }
}

/// <summary>
/// Событие от системы видеоаналитики
/// </summary>
public class VideoAnalyticsEvent
{
    /// <summary>
    /// Временная метка наступления события
    /// </summary>
    public long Timestamp { get; set; }

    /// <summary>
    /// Камера, по которой произошло событие
    /// </summary>
    public string CameraId { get; set; } = "";

    /// <summary>
    /// Кадр с потока, на котором обнаружено лицо
    /// </summary>
    public string Photo { get; set; } = "";

    /// <summary>
    /// Лицо, по которому был осуществлен поиск
    /// </summary>
    public string Thumbnail { get; set; } = "";

    /// <summary>
    /// Степень совпадения
    /// </summary>
    public double Match { get; set; }

    /// <summary>
    /// Зона на кадре с потока, в которой находится обнаруженное лицо
    /// </summary>
    public string Zone { get; set; } = "";

    /// <summary>
    /// Идентификатор вендора аналитики
    /// </summary>
    public int Vendor { get; set; }
}