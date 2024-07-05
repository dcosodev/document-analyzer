using System;

namespace ImageAnalysisAPI.Services
{
    public class GenuineCheckService
    {
        // Determina si una imagen es considerada genuina basándose en si la marca y el modelo de la cámara están disponibles.
        public double CheckIfGenuine(string cameraMake, string cameraModel)
        {
            double genuineScore = 0;
            double genuineFactors = 2; // Factores totales que determinan la autenticidad

            // Incrementa el puntaje si la marca de la cámara está disponible
            if (!string.IsNullOrEmpty(cameraMake))
            {
                genuineScore += 1;
            }

            // Incrementa el puntaje si el modelo de la cámara está disponible
            if (!string.IsNullOrEmpty(cameraModel))
            {
                genuineScore += 1;
            }

            // Calcula el porcentaje de autenticidad basado en la información disponible
            double genuinePercentage = (genuineScore / genuineFactors) * 100;
            return genuinePercentage;
        }
    }
}
