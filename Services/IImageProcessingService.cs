using ImageAnalysisAPI.Models;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ImageAnalysisAPI.Services
{
    public interface IImageProcessingService
    {
        Task<List<PhotoResponse>> AnalyzeImages(string[] paths, string gps, IFormFile[] images, bool clientCamera, string clientIP, string type);
    }
}
