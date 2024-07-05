using Microsoft.AspNetCore.Mvc;
using ImageAnalysisAPI.Services;
using ImageAnalysisAPI.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ImageAnalysisAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ImageAnalysisController : ControllerBase
    {
        private readonly IImageProcessingService _imageProcessingService;
        private readonly ILogger<ImageAnalysisController> _logger;

        public ImageAnalysisController(IImageProcessingService imageProcessingService, ILogger<ImageAnalysisController> logger)
        {
            _imageProcessingService = imageProcessingService;
            _logger = logger;
        }

        [HttpPost("imageAnalyse")]
        public async Task<IActionResult> AnalyzeImages(
            [FromForm] string[] paths = null,
            [FromForm] string gps = "",
            [FromForm] IFormFile[] images = null,
            [FromForm] string clientIP = "",
            [FromForm] bool clientCamera = false,
            [FromForm] string type = "")
        {
            try
            {
                _logger.LogInformation("Starting image analysis...");
                var pathsToAnalyze = paths ?? new string[0];
                var imagesToAnalyze = images ?? new IFormFile[0];

                if (pathsToAnalyze.Length == 0 && imagesToAnalyze.Length == 0)
                {
                    _logger.LogWarning("No images or paths provided.");
                    throw new ArgumentException("No images or paths provided. Please provide image files or paths to images.");
                }

                var responses = await _imageProcessingService.AnalyzeImages(pathsToAnalyze, gps, imagesToAnalyze, clientCamera, clientIP, type);
                _logger.LogInformation("Image analysis completed successfully.");
                return Ok(responses);
            }
            catch (ArgumentException ex)
            {
                var errorResponse = new ErrorResponse($"Invalid argument: {ex.Message}");
                _logger.LogError(ex, errorResponse.ErrorMessage);
                return BadRequest(errorResponse);
            }
            catch (UriFormatException ex)
            {
                var errorResponse = new ErrorResponse($"Invalid URI: {ex.Message}");
                _logger.LogError(ex, errorResponse.ErrorMessage);
                return BadRequest(errorResponse);
            }
            catch (HttpRequestException ex)
            {
                var errorResponse = new ErrorResponse($"HTTP request failed: {ex.Message}");
                _logger.LogError(ex, errorResponse.ErrorMessage);
                return StatusCode(StatusCodes.Status503ServiceUnavailable, errorResponse);
            }
            catch (Exception ex)
            {
                var errorResponse = new ErrorResponse($"An unexpected error occurred: {ex.Message}");
                _logger.LogError(ex, errorResponse.ErrorMessage);
                return StatusCode(StatusCodes.Status500InternalServerError, errorResponse);
            }
        }
    }
}
