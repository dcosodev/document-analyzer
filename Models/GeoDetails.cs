namespace ImageAnalysisAPI.Models
{
    public class GeoDetails
    {
        public string LocIP { get; set; }
        public string CountryCode { get; set; }
        public string LocationDetails { get; set; }
        public string Latitude { get; set; }  // Añadir esta propiedad
        public string Longitude { get; set; } // Añadir esta propiedad

        public GeoDetails(string locIP, string countryCode, string locationDetails, string latitude, string longitude)
        {
            LocIP = locIP;
            CountryCode = countryCode;
            LocationDetails = locationDetails;
            Latitude = latitude;  // Añadir esta línea
            Longitude = longitude; // Añadir esta línea
        }

        // Default constructor
        public GeoDetails() { }
    }
}
