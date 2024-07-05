using Microsoft.AspNetCore.Http;

namespace ImageAnalysisAPI.Models
{
    public class ImageRequest
    {
        public string Path { get; set; }
        public string Gps { get; set; }
        public IFormFile Image { get; set; }
    }
}
