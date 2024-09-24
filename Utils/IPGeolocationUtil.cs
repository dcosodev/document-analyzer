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

            // The request URI carries the API key, so it is never logged.
            var requestUri = $"{endpoint}?apiKey={apiKey}&ip={addressIP}";
            using (var client = new HttpClient())
            {
                var response = await client.GetAsync(requestUri);
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();

                dynamic jsonResponse = Newtonsoft.Json.JsonConvert.DeserializeObject(content);

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
