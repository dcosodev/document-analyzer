using Microsoft.AspNetCore.Http;

namespace ImageAnalysisAPI.Services
{
    public class ValidationService
    {
        public const long MaxFileSizeBytes = 10 * 1024 * 1024;
        public const int MaxFileCount = 10;

        // Verifica si la entrada contiene al menos una imagen o una ruta válida.
        public bool ValidateInput(string[] paths, IFormFile[] images)
        {
            var validPaths = paths?.Where(path => !string.IsNullOrWhiteSpace(path)).ToArray() ?? Array.Empty<string>();
            var validImages = images?.Where(image => image is not null && image.Length > 0).ToArray() ?? Array.Empty<IFormFile>();

            if (validPaths.Length + validImages.Length > MaxFileCount)
            {
                return false;
            }

            if (validImages.Any(image => image.Length > MaxFileSizeBytes))
            {
                return false;
            }

            return validPaths.Length > 0 || validImages.Length > 0;
        }
    }
}
