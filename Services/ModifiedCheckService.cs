// ModifiedCheckService.cs
using System;

namespace ImageAnalysisAPI.Services
{
    public class ModifiedCheckService
    {
        public double CheckIfModified(string editingSoftware, string dateTimeOriginal, string dateTimeModified)
        {
            double modifiedScore = 0;
            double modifiedFactors = 2;

            // Verifica si hay algún software de edición mencionado en los metadatos.
            if (!string.IsNullOrEmpty(editingSoftware))
            {
                modifiedScore += 0;
            }
            else
            {
                modifiedScore += 1;
            }

            // Compara las marcas de tiempo originales y modificadas.
            if (!string.IsNullOrEmpty(dateTimeOriginal))
            {
                if (!dateTimeOriginal.Equals(dateTimeModified))
                {
                    modifiedScore += 0;
                }
                else
                {
                    modifiedScore += 1;
                }
            }
            else
            {
                modifiedScore += 1;
            }

            // Calcula el porcentaje del puntaje de modificación basado en los factores evaluados.
            double modifiedPercentage = (modifiedScore / modifiedFactors) * 100;
            return modifiedPercentage;
        }
    }
}
