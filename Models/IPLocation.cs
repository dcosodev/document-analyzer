namespace ImageAnalysisAPI.Models
{
    public class IPLocation
    {
        public string Location { get; set; }

        public IPLocation(string latitude, string longitude)
        {
            Location = $"{latitude},{longitude}";
        }
    }
}
