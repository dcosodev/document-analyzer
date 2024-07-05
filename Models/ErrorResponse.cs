namespace ImageAnalysisAPI.Models
{
    public class ErrorResponse
    {
        public string ErrorMessage { get; set; }

        public ErrorResponse(string errorMessage)
        {
            ErrorMessage = errorMessage;
        }

        // Default constructor
        public ErrorResponse() { }
    }
}
