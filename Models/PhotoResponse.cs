namespace ImageAnalysisAPI.Models
{
    public class PhotoResponse
    {
        public string ErrorMessage { get; set; } = string.Empty;
        public string PhotoName { get; set; } = string.Empty;
        public string CameraMake { get; set; } = string.Empty;
        public string CameraModel { get; set; } = string.Empty;
        public string GpsLocMeta { get; set; } = string.Empty;
        public string GpsNow { get; set; } = string.Empty;
        public string ClientIP { get; set; } = string.Empty;
        public string GpsDateTime { get; set; } = string.Empty;
        public string DatetimeOriginal { get; set; } = string.Empty;
        public string DatetimeModified { get; set; } = string.Empty;
        public string EditingSoftware { get; set; } = string.Empty;
        public string LocationDetails { get; set; } = string.Empty;
        public string CountryCode { get; set; } = string.Empty;
        public double Genuine { get; set; }
        public double Live { get; set; }
        public double Modified { get; set; }
        public string DataExtract { get; set; } = string.Empty;
        public string FaceData { get; set; } = string.Empty;
        public string FormRecognizer { get; set; } = string.Empty;
        public string GenuineExplanation { get; set; } = string.Empty;
        public string LiveExplanation { get; set; } = string.Empty;
        public string ModifiedExplanation { get; set; } = string.Empty;
    }
}
