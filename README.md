# Document Analyzer

Document Analyzer is a .NET API designed to analyze images and extract metadata, including geolocation information and camera details. 

## Features

- Image analysis to extract metadata.
- Verification of image authenticity.
- Detection of image modifications.
- Document data extraction using Azure Form Recognizer.
- File storage in Azure Blob Storage.

## Requirements

- .NET 6.0 or higher
- Azure account with access to:
  - Azure Blob Storage
  - Azure Form Recognizer
  - Azure IP Geolocation
  - Azure Computer Vision

## Setup

1. Clone the repository:

   ```bash
   git clone https://github.com/dcosodev/document-analyzer.git
   cd document-analyzer
   ```

2. Configure the connection parameters in the `appsettings.json` file:

   ```json
   {
     "Logging": {
       "LogLevel": {
         "Default": "Information",
         "Microsoft.AspNetCore": "Warning"
       }
     },
     "AllowedHosts": "*",
     "Azure": {
       "AI": {
         "Key": "your_azure_key",
         "Endpoint": "https://your_azure_endpoint.cognitiveservices.azure.com/"
       },
       "DocumentIntelligence": {
         "Key": "your_form_recognizer_key",
         "Endpoint": "https://your_form_recognizer_endpoint.cognitiveservices.azure.com/"
       },
       "Storage": {
         "ConnectionString": "your_azure_blob_storage_connection_string"
       }
     },
     "IPGeolocation": {
       "ApiKey": "your_ip_geolocation_key",
       "Endpoint": "https://api.ipgeolocation.io/ipgeo"
     }
   }
   ```

## Usage

1. Build and run the project:

   ```bash
   dotnet build
   dotnet run
   ```

2. Use an API client like Postman to interact with the API. For example, to analyze an image, send a POST request to `/api/ImageAnalysis/analyze` with the required parameters.

## API Endpoints

- `POST /api/ImageAnalysis/analyze`
  - Analyzes images to extract metadata and check for authenticity and modifications.
  - Parameters:
    - `paths` (optional): Array of image paths.
    - `gps` (optional): GPS data.
    - `images` (optional): Array of image files.
    - `clientCamera` (optional): Boolean flag indicating if a client camera was used.
    - `clientIP` (optional): Client IP address.
    - `type` (required): Type of document to analyze.

## Contributing

Contributions are welcome! Please open an issue or submit a pull request for any changes or enhancements.

## License

This project is licensed under the MIT License.
```

This README provides an overview of the project, setup instructions, usage guidelines, and API endpoint information. You can customize it further based on your specific requirements.
