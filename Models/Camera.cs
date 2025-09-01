using System.ComponentModel.DataAnnotations;

namespace CameraManagementAPI.Models;

/// <summary>
/// Модель камеры
/// </summary>
public class Camera
{
    /// <summary>
    /// Уникальный идентификатор камеры
    /// </summary>
    public string Cid { get; set; } = Guid.NewGuid().ToString().ToUpper();

    /// <summary>
    /// Наименование камеры
    /// </summary>
    [Required]
    public string Name { get; set; } = "";

    /// <summary>
    /// Долгота
    /// </summary>
    public float Longitude { get; set; }

    /// <summary>
    /// Широта
    /// </summary>
    public float Latitude { get; set; }

    /// <summary>
    /// Высота
    /// </summary>
    public int Height { get; set; }

    /// <summary>
    /// Описание
    /// </summary>
    public string Descr { get; set; } = "";

    /// <summary>
    /// Тип камеры (стационарная/поворотная)
    /// </summary>
    public string Type { get; set; } = "Стационарная";

    /// <summary>
    /// Модель камеры
    /// </summary>
    public string Model { get; set; } = "";

    /// <summary>
    /// Угол
    /// </summary>
    public int Angle { get; set; }

    /// <summary>
    /// Азимут
    /// </summary>
    public int Azimuth { get; set; }

    /// <summary>
    /// Радиус
    /// </summary>
    public int Radius { get; set; }

    /// <summary>
    /// Адрес
    /// </summary>
    public string Addr { get; set; } = "";

    /// <summary>
    /// IP-адрес камеры
    /// </summary>
    public string Ipv4Addr { get; set; } = "";

    /// <summary>
    /// MAC-адрес камеры
    /// </summary>
    public string MacAddr { get; set; } = "";

    /// <summary>
    /// Серийный номер
    /// </summary>
    public string Sn { get; set; } = "";

    /// <summary>
    /// Ссылка для просмотра видеопотока
    /// </summary>
    public string Webviewurl { get; set; } = "";

    /// <summary>
    /// Идентификатор региона системы
    /// </summary>
    public int Region { get; set; }

    /// <summary>
    /// Адрес сервера, предоставляющий видеопоток
    /// </summary>
    public string StreamHost { get; set; } = "";

    /// <summary>
    /// Порт сервера, предоставляющий видеопоток
    /// </summary>
    public int StreamHttpPort { get; set; }

    /// <summary>
    /// Имя потока
    /// </summary>
    public string StreamName { get; set; } = "";

    /// <summary>
    /// Идентификатор системы
    /// </summary>
    public int VideoSystemId { get; set; }

    /// <summary>
    /// Наименование системы
    /// </summary>
    public string VideoSystemName { get; set; } = "";

    /// <summary>
    /// Доступность камеры
    /// </summary>
    public bool Available { get; set; } = true;
}