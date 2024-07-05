using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using ImageAnalysisAPI.Models;

namespace ImageAnalysisAPI.Utils
{
    public static class MetadataReaderUtil
    {
        public static PhotoResponse ReadMetadata(FileInfo file)
        {
            var response = new PhotoResponse
            {
                PhotoName = file.Name
            };

            try
            {
                using (var image = Image.FromFile(file.FullName))
                {
                    string gpsLatitude = null;
                    string gpsLongitude = null;

                    foreach (var prop in image.PropertyItems)
                    {
                        switch (prop.Id)
                        {
                            case 0x010F: // Camera Make
                                response.CameraMake = GetStringFromProperty(prop);
                                break;
                            case 0x0110: // Camera Model
                                response.CameraModel = GetStringFromProperty(prop);
                                break;
                            case 0x9003: // DateTime Original
                                response.DatetimeOriginal = GetStringFromProperty(prop);
                                break;
                            case 0x9004: // DateTime Digitized
                                response.GpsDateTime = GetStringFromProperty(prop);
                                break;
                            case 0x0132: // DateTime Modified
                                response.DatetimeModified = GetStringFromProperty(prop);
                                break;
                            case 0x9286: // User Comment
                                response.EditingSoftware = GetStringFromProperty(prop);
                                break;
                            case 0x8827: // ISO Speed Ratings
                                response.LocationDetails = BitConverter.ToInt16(prop.Value, 0).ToString();
                                break;
                            case 0xA420: // Image Unique ID
                                response.GpsLocMeta = GetStringFromProperty(prop);
                                break;
                            case 0x0002: // GPS Latitude
                                gpsLatitude = GetGpsCoordinates(prop.Value);
                                break;
                            case 0x0004: // GPS Longitude
                                gpsLongitude = GetGpsCoordinates(prop.Value);
                                break;
                        }
                    }

                    if (gpsLatitude != null && gpsLongitude != null)
                    {
                        response.GpsLocMeta = $"{gpsLatitude},{gpsLongitude}";
                    }
                }
            }
            catch (Exception ex)
            {
                response.ErrorMessage = ex.Message;
            }

            return response;
        }

        private static string GetStringFromProperty(PropertyItem prop)
        {
            return System.Text.Encoding.UTF8.GetString(prop.Value).TrimEnd('\0');
        }

        private static string GetGpsCoordinates(byte[] bytes)
        {
            // Convert bytes to degrees, minutes, and seconds.
            var degrees = BitConverter.ToUInt32(bytes, 0) / (double)BitConverter.ToUInt32(bytes, 4);
            var minutes = BitConverter.ToUInt32(bytes, 8) / (double)BitConverter.ToUInt32(bytes, 12);
            var seconds = BitConverter.ToUInt32(bytes, 16) / (double)BitConverter.ToUInt32(bytes, 20);

            // Convert to decimal degrees.
            var decimalDegrees = degrees + (minutes / 60) + (seconds / 3600);

            return decimalDegrees.ToString(CultureInfo.InvariantCulture);
        }
    }
}
