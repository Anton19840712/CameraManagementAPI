using CameraManagementAPI.Models;
using CameraManagementAPI.Services;
using Microsoft.AspNetCore.Mvc;

namespace CameraManagementAPI.Controllers;

/// <summary>
/// Контроллер для управления камерами
/// </summary>
[ApiController]
[Route("/")]
public class CameraController : ControllerBase
{
    private readonly CameraService _cameraService;
    private readonly ILogger<CameraController> _logger;

    public CameraController(CameraService cameraService, ILogger<CameraController> logger)
    {
        _cameraService = cameraService;
        _logger = logger;
    }

    /// <summary>
    /// Получение камер по региону
    /// GET http://localhost:8080?action=cameras&region=86
    /// </summary>
    [HttpGet]
    public ActionResult<IEnumerable<Camera>> GetCameras([FromQuery] string action, [FromQuery] int region)
    {
        if (action != "cameras")
        {
            return BadRequest("Only 'cameras' action is supported");
        }

        _logger.LogInformation("Fetching cameras for region {Region}", region);
        
        var cameras = _cameraService.GetCamerasByRegion(region);
        return Ok(cameras);
    }

    /// <summary>
    /// Получить камеру по ID
    /// </summary>
    [HttpGet("cameras/{cid}")]
    public ActionResult<Camera> GetCameraById(string cid)
    {
        var camera = _cameraService.GetCameraById(cid);
        if (camera == null)
        {
            return NotFound($"Camera with ID {cid} not found");
        }

        return Ok(camera);
    }

    /// <summary>
    /// Добавить новую камеру
    /// </summary>
    [HttpPost("cameras")]
    public ActionResult<Camera> CreateCamera([FromBody] Camera camera)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var createdCamera = _cameraService.AddCamera(camera);
        _logger.LogInformation("Created camera {Name} with ID {Cid}", createdCamera.Name, createdCamera.Cid);
        
        return CreatedAtAction(nameof(GetCameraById), new { cid = createdCamera.Cid }, createdCamera);
    }

    /// <summary>
    /// Обновить камеру
    /// </summary>
    [HttpPut("cameras/{cid}")]
    public ActionResult<Camera> UpdateCamera(string cid, [FromBody] Camera camera)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var success = _cameraService.UpdateCamera(cid, camera);
        if (!success)
        {
            return NotFound($"Camera with ID {cid} not found");
        }

        var updatedCamera = _cameraService.GetCameraById(cid);
        _logger.LogInformation("Updated camera {Name} with ID {Cid}", updatedCamera?.Name, cid);
        
        return Ok(updatedCamera);
    }

    /// <summary>
    /// Удалить камеру
    /// </summary>
    [HttpDelete("cameras/{cid}")]
    public ActionResult DeleteCamera(string cid)
    {
        var success = _cameraService.DeleteCamera(cid);
        if (!success)
        {
            return NotFound($"Camera with ID {cid} not found");
        }

        _logger.LogInformation("Deleted camera with ID {Cid}", cid);
        return NoContent();
    }
}