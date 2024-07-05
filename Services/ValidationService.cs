using Microsoft.AspNetCore.Http;

namespace ImageAnalysisAPI.Services
{
    public class ValidationService
    {
        // Verifica si la entrada contiene al menos una imagen o una ruta válida.
        public bool ValidateInput(string[] paths, IFormFile[] images)
        {
            bool hasValidPath = paths != null && paths.Length > 0 && paths.Any(path => !string.IsNullOrEmpty(path));
            bool hasValidImage = images != null && images.Length > 0 && images.Any(image => image.Length > 0);

            return hasValidPath || hasValidImage;
        }
    }
}
