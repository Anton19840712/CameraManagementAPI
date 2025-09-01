using System.Collections.Generic;
using CameraManagementAPI.Models;
using CameraManagementAPI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

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
	/// Получение всех камер
	/// </summary>
	[HttpGet("cameras/all")]
	public ActionResult<IEnumerable<Camera>> GetCameras()
	{
		var cameras = _cameraService.GetCamerasAll();
		return Ok(cameras);
	}
}