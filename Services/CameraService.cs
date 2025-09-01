using CameraManagementAPI.Models;

namespace CameraManagementAPI.Services;

/// <summary>
/// Сервис для работы с камерами
/// </summary>
public class CameraService
{
	// Камеры - это временное хранилище в памяти
	private readonly List<Camera> _cameras = new();

    public CameraService()
    {
        InitializeDefaultCameras();
    }

    /// <summary>
    /// Получить камеры по региону
    /// </summary>
    public IEnumerable<Camera> GetCamerasByRegion(int region)
    {
        return _cameras.Where(c => c.Region == region);
    }

    /// <summary>
    /// Получить камеру по ID
    /// </summary>
    public Camera? GetCameraById(string cid)
    {
        return _cameras.FirstOrDefault(c => c.Cid == cid);
    }

    /// <summary>
    /// Добавить камеру
    /// </summary>
    public Camera AddCamera(Camera camera)
    {
        if (string.IsNullOrEmpty(camera.Cid))
        {
            camera.Cid = Guid.NewGuid().ToString().ToUpper();
        }
        
        _cameras.Add(camera);
        return camera;
    }

    /// <summary>
    /// Обновить камеру
    /// </summary>
    public bool UpdateCamera(string cid, Camera updatedCamera)
    {
        var existingCamera = GetCameraById(cid);
        if (existingCamera == null) return false;

        // Обновляем все поля, кроме ID
        existingCamera.Name = updatedCamera.Name;
        existingCamera.Longitude = updatedCamera.Longitude;
        existingCamera.Latitude = updatedCamera.Latitude;
        existingCamera.Height = updatedCamera.Height;
        existingCamera.Descr = updatedCamera.Descr;
        existingCamera.Type = updatedCamera.Type;
        existingCamera.Model = updatedCamera.Model;
        existingCamera.Angle = updatedCamera.Angle;
        existingCamera.Azimuth = updatedCamera.Azimuth;
        existingCamera.Radius = updatedCamera.Radius;
        existingCamera.Addr = updatedCamera.Addr;
        existingCamera.Ipv4Addr = updatedCamera.Ipv4Addr;
        existingCamera.MacAddr = updatedCamera.MacAddr;
        existingCamera.Sn = updatedCamera.Sn;
        existingCamera.Webviewurl = updatedCamera.Webviewurl;
        existingCamera.Region = updatedCamera.Region;
        existingCamera.StreamHost = updatedCamera.StreamHost;
        existingCamera.StreamHttpPort = updatedCamera.StreamHttpPort;
        existingCamera.StreamName = updatedCamera.StreamName;
        existingCamera.VideoSystemId = updatedCamera.VideoSystemId;
        existingCamera.VideoSystemName = updatedCamera.VideoSystemName;
        existingCamera.Available = updatedCamera.Available;

        return true;
    }

    /// <summary>
    /// Удалить камеру
    /// </summary>
    public bool DeleteCamera(string cid)
    {
        var camera = GetCameraById(cid);
        if (camera == null) return false;

        _cameras.Remove(camera);
        return true;
    }

    /// <summary>
    /// Инициализация тестовых камер
    /// </summary>
    private void InitializeDefaultCameras()
    {
        _cameras.AddRange(new[]
        {
            new Camera
            {
                Cid = "882E965B-1038-4EA1-B6B1-66803EFC7C9C",
                Name = "Воробьёво",
                Longitude = 20.828997470038f,
                Latitude = 54.700509910672f,
                Height = 70,
                Descr = "Воробьвский парк на пересечении улиц Маяковского и Урицкого",
                Type = "Стационарная",
                Model = "Axis P1365",
                Angle = 80,
                Azimuth = 225,
                Radius = 100,
                Addr = "882E965B-1038-4EA1-B6B1-66803EFC7C9C",
                Ipv4Addr = "192.168.0.33",
                MacAddr = "00:26:57:00:1f:02",
                Sn = "dg1243234P24",
                Webviewurl = "http://is.ru/watch?v=2134253",
                Region = 86,
                StreamHost = "is.ru",
                StreamHttpPort = 8080,
                StreamName = "mwNFaS3Wrwo",
                VideoSystemId = 18,
                VideoSystemName = "Main Video System",
                Available = true
            },
            new Camera
            {
                Name = "Центральная площадь",
                Longitude = 20.830f,
                Latitude = 54.702f,
                Height = 80,
                Descr = "Камера на центральной площади",
                Type = "Поворотная",
                Model = "Hikvision DS-2DE7425IW-AE",
                Angle = 360,
                Azimuth = 0,
                Radius = 150,
                Addr = "Центральная площадь, 1",
                Ipv4Addr = "192.168.0.34",
                MacAddr = "00:26:57:00:1f:03",
                Sn = "hk5678901P25",
                Webviewurl = "http://is.ru/watch?v=5647382",
                Region = 86,
                StreamHost = "is.ru",
                StreamHttpPort = 8080,
                StreamName = "centralSquare",
                VideoSystemId = 18,
                VideoSystemName = "Main Video System",
                Available = true
            },
            new Camera
            {
                Name = "Парковка ТЦ Галерея",
                Longitude = 20.825f,
                Latitude = 54.695f,
                Height = 60,
                Descr = "Видеонаблюдение парковки торгового центра",
                Type = "Стационарная",
                Model = "Dahua IPC-HFW4431R-Z",
                Angle = 90,
                Azimuth = 180,
                Radius = 80,
                Addr = "ул. Торговая, 15",
                Ipv4Addr = "192.168.0.35",
                MacAddr = "00:26:57:00:1f:04",
                Sn = "dh9876543P26",
                Webviewurl = "http://is.ru/watch?v=9876543",
                Region = 86,
                StreamHost = "is.ru",
                StreamHttpPort = 8080,
                StreamName = "mallParking",
                VideoSystemId = 18,
                VideoSystemName = "Main Video System",
                Available = false
            }
        });
    }
}