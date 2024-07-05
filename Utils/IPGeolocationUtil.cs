using System;
using System.Net.Http;
using System.Threading.Tasks;
using ImageAnalysisAPI.Models;

namespace ImageAnalysisAPI.Utils
{
    public static class IPGeolocationUtil
    {
        public static async Task<GeoDetails> GetGeoDetailsAsync(string addressIP, string apiKey, string endpoint)
        {
            if (string.IsNullOrEmpty(apiKey) || string.IsNullOrEmpty(endpoint))
            {
                throw new ArgumentException("API Key or Endpoint is missing.");
            }

            var requestUri = $"{endpoint}?apiKey={apiKey}&ip={addressIP}";
            using (var client = new HttpClient())
            {
                Console.WriteLine($"Request URI: {requestUri}");
                var response = await client.GetAsync(requestUri);
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Response: {content}");

                dynamic jsonResponse = Newtonsoft.Json.JsonConvert.DeserializeObject(content);
                Console.WriteLine($"IP: {jsonResponse.ip}");
                Console.WriteLine($"Latitude: {jsonResponse.latitude}");
                Console.WriteLine($"Longitude: {jsonResponse.longitude}");

                return new GeoDetails
                {
                    LocIP = jsonResponse.ip,
                    CountryCode = jsonResponse.country_code2,
                    LocationDetails = $"{jsonResponse.city}, {jsonResponse.state_prov}, {jsonResponse.country_name}",
                    Latitude = jsonResponse.latitude,
                    Longitude = jsonResponse.longitude
                };
            }
        }
    }
}
