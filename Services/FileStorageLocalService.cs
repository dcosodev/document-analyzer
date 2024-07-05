using Microsoft.AspNetCore.Http;
using System;
using System.IO;
using System.Threading.Tasks;

namespace ImageAnalysisAPI.Services
{
    public class FileStorageLocalService
    {
        // Almacena un archivo subido temporalmente y devuelve una referencia al archivo almacenado.
        public async Task<FileInfo> StoreFileLocalAsync(IFormFile multipartFile)
        {
            // Crea un archivo temporal con un nombre único basado en el nombre original del archivo subido.
            var tempFilePath = Path.Combine(Path.GetTempPath(), $"upload-{multipartFile.FileName}");

            using (var stream = new FileStream(tempFilePath, FileMode.Create))
            {
                await multipartFile.CopyToAsync(stream);
            }

            return new FileInfo(tempFilePath);
        }

        // Elimina un archivo temporal.
        public void DeleteLocalFile(FileInfo file)
        {
            if (file.Exists)
            {
                file.Delete();
            }
        }
    }
}
